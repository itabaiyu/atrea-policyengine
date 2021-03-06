﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Atrea.PolicyEngine.Processors;

namespace Atrea.PolicyEngine.Internal.ProcessorRunners
{
    internal class AsyncProcessorRunnerDecorator<T> : BaseProcessorRunnerDecorator<T>
    {
        private readonly IEnumerable<IAsyncProcessor<T>> _asyncProcessors;

        public AsyncProcessorRunnerDecorator(
            IAsyncProcessorRunner<T> asyncProcessorRunner,
            IEnumerable<IAsyncProcessor<T>> asyncProcessors) : base(
            asyncProcessorRunner) =>
            _asyncProcessors = asyncProcessors;

        protected override async Task RunProcessors(T item)
        {
            foreach (var asyncProcessor in _asyncProcessors)
            {
                await asyncProcessor.ProcessAsync(item);
            }
        }
    }
}