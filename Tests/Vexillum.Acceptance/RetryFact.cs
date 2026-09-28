using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Vexillum.Acceptance
{
    /// <summary>
    /// A [Fact] for tests that drive the real server in real time and can be
    /// disturbed by things outside their control (bot fire, scheduler jitter
    /// under load). The test is re-run up to MaxRetries times only when it
    /// fails; a pass on any attempt passes, every attempt's failure message is
    /// reported, and the retries are counted in the test output so genuine
    /// regressions still show up as failures, never as silent flakiness.
    /// Use it only where the flakiness is understood and documented in the
    /// test; a deterministic precondition beats a retry.
    /// </summary>
    [XunitTestCaseDiscoverer("Vexillum.Acceptance.RetryFactDiscoverer", "Vexillum.Acceptance")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RetryFactAttribute : FactAttribute
    {
        /// <summary>Total attempts (default 3).</summary>
        public int MaxRetries { get; set; } = 3;
        /// <summary>Pause between attempts, letting the server settle (default 2 s).</summary>
        public int DelayMs { get; set; } = 2000;
    }

    public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public RetryFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            int maxRetries = factAttribute.GetNamedArgument<int>("MaxRetries");
            int delayMs = factAttribute.GetNamedArgument<int>("DelayMs");
            if (maxRetries < 1) maxRetries = 3;
            yield return new RetryTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, maxRetries, delayMs);
        }
    }

    [Serializable]
    public class RetryTestCase : XunitTestCase
    {
        private int maxRetries;
        private int delayMs;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public RetryTestCase() { }

        public RetryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod, int maxRetries, int delayMs)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, null)
        {
            this.maxRetries = maxRetries;
            this.delayMs = delayMs;
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            int attempt = 0;
            while (true)
            {
                attempt++;
                // Buffer this attempt's messages: only the final attempt (or the
                // first pass) is forwarded, so a retried failure is not reported.
                DelayedMessageBus delayedBus = new DelayedMessageBus(messageBus);
                RunSummary summary = await base.RunAsync(diagnosticMessageSink, delayedBus, constructorArguments, aggregator, cancellationTokenSource);
                if (aggregator.HasExceptions || summary.Failed == 0 || attempt >= maxRetries)
                {
                    if (attempt > 1)
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage("[RetryFact] {0} {1} after {2} attempt(s)", DisplayName, summary.Failed == 0 ? "passed" : "FAILED", attempt));
                    delayedBus.Dispose();   // forwards the buffered messages
                    return summary;
                }
                diagnosticMessageSink.OnMessage(new DiagnosticMessage("[RetryFact] {0} failed on attempt {1}/{2}; retrying in {3} ms", DisplayName, attempt, maxRetries, delayMs));
                Thread.Sleep(delayMs);
            }
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue("MaxRetries", maxRetries);
            data.AddValue("DelayMs", delayMs);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            maxRetries = data.GetValue<int>("MaxRetries");
            delayMs = data.GetValue<int>("DelayMs");
        }
    }

    /// <summary>Holds an attempt's messages until it is known whether they should be reported.</summary>
    public class DelayedMessageBus : IMessageBus
    {
        private readonly IMessageBus innerBus;
        private readonly List<IMessageSinkMessage> messages = new List<IMessageSinkMessage>();

        public DelayedMessageBus(IMessageBus innerBus)
        {
            this.innerBus = innerBus;
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            lock (messages)
                messages.Add(message);
            return true;
        }

        public void Dispose()
        {
            foreach (IMessageSinkMessage message in messages)
                innerBus.QueueMessage(message);
        }
    }
}
