using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit.XunitExtensibility.FlakySystem
{
    [XunitTestCaseDiscoverer("JourneyTests.XunitExtensibility.FlakySystem.FlakySystemFactDiscoverer", "JourneyTests")]
    public class FlakySystemFactAttribute: FactAttribute
    {        
        public int RetryOnFailureCount { get; set; }

        public Type ExceptionTypeToIgnore { get; set; }

        public FlakySystemFactAttribute(int retryOnFailureCount = 1) : this(null, retryOnFailureCount)
        {
        }

        public FlakySystemFactAttribute(Type exceptionType, int retryOnFailureCount = 1) 
        {
            if (retryOnFailureCount <= 0)
            {
                retryOnFailureCount = 1;
            }

            RetryOnFailureCount = retryOnFailureCount;
            ExceptionTypeToIgnore = exceptionType;
        }
    }   
}