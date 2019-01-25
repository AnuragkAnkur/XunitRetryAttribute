using System;
using System.Threading;
using System.Threading.Tasks;
using JourneyTests.BusinessException;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility.FlakySystem
{
    public class FlakySystemTestsRunner : XunitTestCaseRunner
    {
        private readonly int _runCount;
        private readonly Type _exceptionToIgnoreTest;

        public FlakySystemTestsRunner(IXunitTestCase testCase, string displayName, string skipReason,
            object[] constructorArguments,
            object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource, int runCount, Type exceptionToIgnoreTest) 
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
        {
            this._runCount = runCount;
            _exceptionToIgnoreTest = exceptionToIgnoreTest;
        }


        public async Task<RunSummary> Run()
        {
            var runCount = 1;
            RunSummary summary = new RunSummary();

            var test = (ITest)new XunitTest(this.TestCase, this.DisplayName);
            var output = string.Empty;
            
            while (runCount <= _runCount)
            {
                await this.AfterTestCaseStartingAsync();
                
                summary = new RunSummary()
                {
                    Total = 1
                };

                this.Aggregator = new ExceptionAggregator(Aggregator);
                var customerRunner = new XunitCustomRunner(test, this.MessageBus, this.TestClass, this.ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes, this.Aggregator, CancellationTokenSource);
                output = await customerRunner.RunAsync(summary);
                await this.BeforeTestCaseFinishedAsync();
                
                var exception = this.Aggregator.ToException();
                
                if (exception == null)
                {
                    var testResultMessage = (TestResultMessage)new TestPassed(test, summary.Time, output);
                    if (!this.CancellationTokenSource.IsCancellationRequested && !this.MessageBus.QueueMessage((IMessageSinkMessage)testResultMessage))
                        this.CancellationTokenSource.Cancel();
                    break;
                }

                if (exception.GetType() == _exceptionToIgnoreTest)
                {
                    SkipReason = exception.Message;
                    var testResultMessage = (TestResultMessage)new TestSkipped(test, SkipReason);
                    if (!this.CancellationTokenSource.IsCancellationRequested && !this.MessageBus.QueueMessage((IMessageSinkMessage)testResultMessage))
                        this.CancellationTokenSource.Cancel();
                    break;
                }

                runCount++;
            }

            if (this.Aggregator.HasExceptions)
            {
                var ex = this.Aggregator.ToException();
                var testResult = (TestResultMessage)new TestFailed(test, summary.Time, output, ex);
                ++summary.Failed;

                if (!this.CancellationTokenSource.IsCancellationRequested && !this.MessageBus.QueueMessage((IMessageSinkMessage)testResult))
                    this.CancellationTokenSource.Cancel();
            }
            
            if (!this.MessageBus.QueueMessage((IMessageSinkMessage)new TestCaseFinished((ITestCase)this.TestCase, summary.Time, summary.Total, summary.Failed, summary.Skipped)))
                this.CancellationTokenSource.Cancel();

            return summary;
        }
    }
}
