using Microsoft.Extensions.Configuration;
using System;
using tSync.Model;

namespace tSync
{
    public static class Data
    {
        public static Tenant[] Tenants;

        public static string SimaticDemo = @"SIMATIC_RTLS_LM,DFT,02,000000028f48,0,0,1.00,3,2021-05-25T17:35:19+02:00
SIMATIC_RTLS_LM,DFT,02,000000028f52,84000,40000,1.00,3,2021-05-25T17:35:19+02:00
";

        public static void LoadTenantsFromConfig()
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("./Configuration/config.json").Build();
                Tenants = config.GetSection("tenants").Get<Tenant[]>();

                foreach (var tenant in Tenants)
                {
                    tenant.BuildRtlsDataSource();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Configuration error");
            }
        }
    }
}