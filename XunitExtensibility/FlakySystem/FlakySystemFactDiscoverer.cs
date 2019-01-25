using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility.FlakySystem
{
    public class FlakySystemFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public FlakySystemFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var maxRetries = factAttribute.GetNamedArgument<int>("RetryOnFailureCount");
            var exceptionToIgnoreTest = factAttribute.GetNamedArgument<Type>("ExceptionTypeToIgnore");

            yield return new FlakySystemTestCases(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),  testMethod,  maxRetries, exceptionToIgnoreTest);
        }
    }
}