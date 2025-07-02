using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tSync.Options
{
    public class MemoryCacheOptions
    {
        public int ExpirationInSeconds { get; set; } = 60;
    }
}
