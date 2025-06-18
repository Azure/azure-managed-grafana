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

namespace TestUtility
{
    public class TestBase
    {
        private readonly TelemetryConfiguration _appInsightsConfig = new TelemetryConfiguration()
        {
            ConnectionString = "InstrumentationKey=325fe9df-8b9a-412c-8f5d-fc268414d6ab;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/;ApplicationId=c17a4dee-fc11-44bd-81f7-597c85d453c3",
        };
        private DependencyTrackingTelemetryModule _depModule;
        private TelemetryClient _appInsightsClient;
        private bool _disposed = false;
        private readonly ITestCorrelatorContext _testCorrelatorContext;

        public TestBase(ITestOutputHelper output, bool useAppInsights = false, [CallerFilePath] string sourceFile = "")
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
                }
            }
            catch
            {
                operationName = testClass;
            }
        }

        public ITest Test { get; }

        public ILogger Logger { get; private set; }

        public string TestClassName { get; }

        public string TestMethodName { get; }

        private void GenerateLogger(string testClass, bool useAppInsights, ITestOutputHelper output = null)
        {
            var loggerConfig = new LoggerConfiguration()
                .Enrich.WithProperty("TestClassName", testClass)
                .Enrich.WithProperty("UnitTestessionId", Guid.NewGuid().ToString())
                .Enrich.WithProperty("UnitTestStartTime", DateTime.UtcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));

            loggerConfig = loggerConfig.WriteTo.TestCorrelator();

            if (useAppInsights)
            {
                var builder = _appInsightsConfig.TelemetryProcessorChainBuilder;
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
