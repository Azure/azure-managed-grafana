using Serilog.Sinks.TestCorrelator;
using Serilog;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Linq;
using System.Threading;
using Microsoft.ApplicationInsights.DataContracts;

namespace TestUtility
{
    public class TestBase : IDisposable
    {
        private readonly TelemetryConfiguration _appInsightsConfig = new TelemetryConfiguration()
        {
            ConnectionString = "InstrumentationKey=325fe9df-8b9a-412c-8f5d-fc268414d6ab;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/;ApplicationId=c17a4dee-fc11-44bd-81f7-597c85d453c3",
        };
        private DependencyTrackingTelemetryModule _depModule;
        private TelemetryClient _appInsightsClient;
        private readonly IOperationHolder<RequestTelemetry> _currentOperation;
        private bool _disposed = false;
        private readonly ITestCorrelatorContext _testCorrelatorContext;

        static TestBase()
        {
            TestExceptionHelper.EnableExceptionCapture();
        }

        public TestBase(ITestOutputHelper output, bool useAppInsights = true, [CallerFilePath] string sourceFile = "")
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            TestExceptionHelper.Register(sourceFile);
            string testClass = GetType().Name;
            GenerateLogger(testClass, useAppInsights, output);
            _testCorrelatorContext = TestCorrelator.CreateContext();
            string operationName = null;

            try
            {
                var testOutputType = output.GetType();
                FieldInfo cachedTestMember = testOutputType.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
                if (cachedTestMember != null)
                {
                    Test = (ITest)cachedTestMember.GetValue(output);


                    string[] parts = Test.DisplayName.Split('.');
                    TestClassName = parts[parts.Length - 2];
                    TestMethodName = parts[parts.Length - 1];

                    operationName = $"{TestClassName}-{TestMethodName}";
                }
            }
            catch
            {
                operationName = testClass;
            }

            if (!string.IsNullOrEmpty(operationName) && useAppInsights)
            {
                _currentOperation = _appInsightsClient.StartOperation<RequestTelemetry>(operationName);
            }
        }

        public ITest Test { get; }

        public ILogger Logger { get; private set; }

        public string TestClassName { get; }

        public string TestMethodName { get; }

        public string DateTimeStr { get; } = DateTime.UtcNow.ToString("MMddHmmss", CultureInfo.InvariantCulture);

        public bool? IsFailure { get; private set; }

        public Serilog.Events.LogEvent[] GetLogEvents() => TestCorrelator.GetLogEventsFromContextGuid(_testCorrelatorContext.Guid).ToArray();

        public void CheckLogPrefix(string expectingLogPrefic, int count = 1)
        {
            var logs = GetLogEvents();
            int matchingCount = logs.Count(l => l.MessageTemplate.Text.StartsWith(expectingLogPrefic, StringComparison.OrdinalIgnoreCase));
            if (matchingCount != count)
            {
                throw new InvalidOperationException($"There should be {count} of the log prefix, however there are {matchingCount} in actual. log prefix: {expectingLogPrefic}");
            }
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                var theExceptionThrownByTest = TestExceptionHelper.TestException;
                if (theExceptionThrownByTest != null)
                {
                    IsFailure = true;
                    Logger.Error(theExceptionThrownByTest, $"test_failure. {theExceptionThrownByTest.Message}");
                    
                    if (_currentOperation != null)
                    {
                        _currentOperation?.Telemetry?.Properties?.Add("FailureMessage", theExceptionThrownByTest.Message);
                    }
                }
                else
                {
                    IsFailure = false;
                }


                _testCorrelatorContext.Dispose();

                if (_appInsightsClient != null)
                {
                    if (IsFailure == true)
                    {
                        _currentOperation.Telemetry.Success = false;
                        _currentOperation.Telemetry.ResponseCode = "500";
                    }

                    _currentOperation?.Dispose();

                    _appInsightsClient?.Flush();
                    _appInsightsConfig?.Dispose();
                    _depModule?.Dispose();

                    // Wait while the telemetry data is being flushed
                    Thread.Sleep(3000);
                }

                _appInsightsClient = null;
                _depModule = null;
            }
            catch
            {
            }
        }


        private void GenerateLogger(string testClass, bool useAppInsights, ITestOutputHelper output = null)
        {
            UnitTestSessionContext unitTestSessionContext = new UnitTestSessionContext()
            {
                TestClassName = testClass,
            };

            var loggerConfig = new LoggerConfiguration();

            loggerConfig = loggerConfig.WriteTo.TestCorrelator();

            if (useAppInsights)
            {
                var builder = _appInsightsConfig.TelemetryProcessorChainBuilder;

                _appInsightsConfig.TelemetryInitializers.Add(new UnitTestSessionTelemetryInitializer(unitTestSessionContext));

                builder.Build();

                _depModule = new DependencyTrackingTelemetryModule();
                _depModule.Initialize(_appInsightsConfig);
                _appInsightsClient = new TelemetryClient(_appInsightsConfig);

                loggerConfig = loggerConfig.WriteTo.ApplicationInsights(_appInsightsClient, TelemetryConverter.Events);
            }

            if (output != null)
            {
                loggerConfig = loggerConfig.WriteTo.Xunit(output);
            }

            Logger = loggerConfig.Enrich.FromLogContext().CreateLogger();
        }
    }
}
