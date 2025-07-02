using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace tSync.Model
{
    public static class Providers
    {
        public const string ThingPark = "ThingPark";
        public const string Quuppa = "Quuppa";
        public const string Spin = "Spin";
        public const string Simulator = "Simulator";
        public const string Precog = "Precog";
        public const string RFControls = "RFControls";
        public const string CommanderApi = "CommanderApi";
        public const string Cisco = "Cisco";

        public static IEnumerable<string> GetProviders()
        {
            return typeof(Providers).GetFields().Where(f => f.IsLiteral && !f.IsInitOnly).Select(f => (string)f.GetValue(null));
        }
    }
}
