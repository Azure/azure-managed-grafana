using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace TestUtility
{
    public class UnitTestSessionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly UnitTestSessionContext _unitTestSessionContext;

        public UnitTestSessionTelemetryInitializer(UnitTestSessionContext unitTestSessionContext)
        {
            _unitTestSessionContext = unitTestSessionContext ?? throw new ArgumentNullException(nameof(unitTestSessionContext));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ISupportProperties propertyItem && propertyItem.Properties != null)
            {
                if (!string.IsNullOrEmpty(_unitTestSessionContext.TestClassName))
                {
                    propertyItem.Properties[nameof(_unitTestSessionContext.TestClassName)] = _unitTestSessionContext.TestClassName;
                }

                if (!string.IsNullOrEmpty(_unitTestSessionContext.UnitTestessionId))
                {
                    propertyItem.Properties[nameof(_unitTestSessionContext.UnitTestessionId)] = _unitTestSessionContext.UnitTestessionId;
                }

                if (!string.IsNullOrEmpty(_unitTestSessionContext.UnitTestStartTime))
                {
                    propertyItem.Properties[nameof(_unitTestSessionContext.UnitTestStartTime)] = _unitTestSessionContext.UnitTestStartTime;
                }
            }
        }
    }
}
