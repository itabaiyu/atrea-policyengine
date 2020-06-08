﻿using System.Threading.Tasks;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Examples.Mocks.Domain;
using Atrea.PolicyEngine.Examples.Mocks.Policies.Input;
using Atrea.PolicyEngine.Examples.Mocks.Policies.Output;
using Atrea.PolicyEngine.Examples.Mocks.Processors;
using Atrea.PolicyEngine.Policies.Input;

namespace Atrea.PolicyEngine.Examples.Examples
{
    public class NestedAsyncPolicyEngineExample : IAsyncExample
    {
        public async Task RunAsync()
        {
            var canadianFrenchTranslationEngine = BuildCanadianFrenchTranslationEngine();
            var englishTranslationEngine = BuildEnglishTranslationEngine();
            var numericTranslationEngine = BuildNumericTranslationEngine();

            var aggregateTranslationEngine = AsyncPolicyEngineBuilder<TranslatableItem>
                .Configure()
                // Only process items which have not yet been translated.
                .WithInputPolicies(NotYetTranslated)
                .WithParallelProcessors(
                    // Use the Canadian French, English, and Numeric translation engines to
                    // perform translations.
                    // Note: translation engines will run in parallel.
                    canadianFrenchTranslationEngine,
                    englishTranslationEngine,
                    numericTranslationEngine
                )
                // No output policies needed, since each individual engine handles its own
                // post-processing steps.
                .WithOutputPolicies()
                .Build();

            var translatableItem = new TranslatableItem();

            await aggregateTranslationEngine.ProcessAsync(translatableItem);
        }

        private static IAsyncPolicyEngine<TranslatableItem> BuildCanadianFrenchTranslationEngine() =>
            AsyncPolicyEngineBuilder<TranslatableItem>
                .Configure()
                .WithInputPolicies(
                    // Only process items which have not yet been translated and are translations
                    // that are either from Canadian French XOR to Canadian French.
                    NotYetTranslated,
                    new Xor<TranslatableItem>(FromCanadianFrench, ToCanadianFrench)
                ).WithAsyncProcessors(
                    // Use the GoogleTranslator, MicrosoftTranslator, and CacheTranslator to perform
                    // translations.
                    GoogleTranslator,
                    MicrosoftTranslator,
                    CacheTranslator
                ).WithOutputPolicies(
                    // After processing, publish the translation, mark the item as translated, and 
                    // send a translation success email to the user who requested it.
                    PublishTranslation,
                    MarkItemTranslated,
                    SendTranslationSuccessEmail
                )
                .Build();

        private static IAsyncPolicyEngine<TranslatableItem> BuildEnglishTranslationEngine() =>
            AsyncPolicyEngineBuilder<TranslatableItem>
                .Configure()
                .WithInputPolicies(
                    // Only process items which have not yet been translated and are translations
                    // that are from US English AND to UK English, OR are from UK English AND to US English.
                    NotYetTranslated,
                    new Or<TranslatableItem>(
                        new And<TranslatableItem>(FromUsEnglish, ToUkEnglish),
                        new And<TranslatableItem>(FromUkEnglish, ToUsEnglish)
                    )
                ).WithAsyncProcessors(
                    // Use the SingleWordTranslator and DictionaryTranslator to perform translations.
                    SingleWordTranslator,
                    DictionaryTranslator
                ).WithOutputPolicies(
                    // After processing, publish the translation, mark the item as translated, and 
                    // send a translation success email to the user who requested it.
                    PublishTranslation,
                    MarkItemTranslated
                )
                .Build();

        private static IAsyncPolicyEngine<TranslatableItem> BuildNumericTranslationEngine() =>
            AsyncPolicyEngineBuilder<TranslatableItem>
                .Configure()
                .WithInputPolicies(
                    // Only process items which have not yet been translated and are translations which
                    // contain numeric text.
                    NotYetTranslated,
                    ContainsNumericText
                ).WithAsyncProcessors(
                    // Use the SingleWordTranslator to perform translations.
                    SingleWordTranslator
                ).WithOutputPolicies(
                    // After processing, publish the translation, mark the item as translated, and 
                    // send a translation success email to the user who requested it.
                    PublishTranslation,
                    MarkItemTranslated
                )
                .Build();

        #region input policies

        private static readonly NotYetTranslated NotYetTranslated = new NotYetTranslated();
        private static readonly FromUkEnglish FromUkEnglish = new FromUkEnglish();
        private static readonly FromUsEnglish FromUsEnglish = new FromUsEnglish();
        private static readonly FromCanadianFrench FromCanadianFrench = new FromCanadianFrench();
        private static readonly ToUkEnglish ToUkEnglish = new ToUkEnglish();
        private static readonly ToUsEnglish ToUsEnglish = new ToUsEnglish();
        private static readonly ToCanadianFrench ToCanadianFrench = new ToCanadianFrench();
        private static readonly ContainsNumericText ContainsNumericText = new ContainsNumericText();

        #endregion

        #region processors

        private static readonly GoogleTranslator GoogleTranslator = new GoogleTranslator();
        private static readonly MicrosoftTranslator MicrosoftTranslator = new MicrosoftTranslator();
        private static readonly CacheTranslator CacheTranslator = new CacheTranslator();
        private static readonly SingleWordTranslator SingleWordTranslator = new SingleWordTranslator();
        private static readonly DictionaryTranslator DictionaryTranslator = new DictionaryTranslator();

        #endregion

        #region output policies

        private static readonly PublishTranslation PublishTranslation = new PublishTranslation();
        private static readonly MarkItemTranslated MarkItemTranslated = new MarkItemTranslated();

        private static readonly SendTranslationSuccessEmail SendTranslationSuccessEmail =
            new SendTranslationSuccessEmail();

        #endregion
    }
}