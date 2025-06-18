using System;
using System.Globalization;

namespace TestUtility
{
    public class UnitTestSessionContext
    {
        public string TestClassName { get; set; }

        public string UnitTestessionId { get; set; } = Guid.NewGuid().ToString();

        public string UnitTestStartTime { get; set; } = DateTime.UtcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }
}
