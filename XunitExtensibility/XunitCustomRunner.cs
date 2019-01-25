using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility
{
    public class XunitCustomRunner : XunitTestRunner
    {
        public XunitCustomRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) 
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
        }

        public async Task<string> RunAsync(RunSummary runSummary)
        {
            string output = string.Empty;
            var queueMessage = this.MessageBus.QueueMessage((IMessageSinkMessage)new TestStarting(this.Test));
            if (!queueMessage)
            {
                this.CancellationTokenSource.Cancel();
            }
            else
            {
                this.AfterTestStarting();
                if (!string.IsNullOrEmpty(this.SkipReason))
                {
                    ++runSummary.Skipped;
                    if (!this.MessageBus.QueueMessage((IMessageSinkMessage)new TestSkipped(this.Test, this.SkipReason)))
                        this.CancellationTokenSource.Cancel();
                }
                else
                {
                    this.Aggregator.Clear();
                    if (!this.Aggregator.HasExceptions)
                    {
                        Tuple<Decimal, string> tuple = await this.Aggregator.RunAsync<Tuple<Decimal, string>>((Func<Task<Tuple<Decimal, string>>>)(() => this.InvokeTestAsync(this.Aggregator)));
                        if (tuple != null)
                        {
                            runSummary.Time = tuple.Item1;
                            output = tuple.Item2;
                        }
                    }

                }

                this.BeforeTestFinished();
            }

            return output;
        }
    }
}
