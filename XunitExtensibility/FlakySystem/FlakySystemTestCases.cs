
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility.FlakySystem
{
    [Serializable]
    public class FlakySystemTestCases : XunitTestCase
    {
        private int _maxRetries;
        private readonly Type _exceptionToIgnoreTest;

        [Obsolete("Called by the de-serializer", true)]
        public FlakySystemTestCases()
        {
        }

        public FlakySystemTestCases(IMessageSink diagnosticMessageSink, TestMethodDisplay testMethodDisplay,
            ITestMethod testMethod, int maxRetries, Type exceptionToIgnoreTest)
            : base(diagnosticMessageSink, testMethodDisplay, TestMethodDisplayOptions.None, testMethod, testMethodArguments: null)
        {
            this._maxRetries = maxRetries;
            _exceptionToIgnoreTest = exceptionToIgnoreTest;
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var bus = new CustomMessageBus(messageBus);

            var runner = new FlakySystemTestsRunner((IXunitTestCase) this, this.DisplayName, this.SkipReason, constructorArguments,
                this.TestMethodArguments, bus, aggregator, cancellationTokenSource, this._maxRetries, _exceptionToIgnoreTest);

            var summary = await runner.Run();

            return summary;
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("MaxRetries", _maxRetries);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            _maxRetries = data.GetValue<int>("MaxRetries");
        }
    }
}