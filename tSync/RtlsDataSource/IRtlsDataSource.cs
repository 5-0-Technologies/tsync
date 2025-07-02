using System.Collections.Generic;
using tSync.Model;

namespace tSync.RtlsDataSource
{
    public interface IRtlsDataSource
    {
        public IEnumerable<LocalizationRecord> GetLocalization();

        public IRtlsDataSource Build(params object[] data);

        /// <summary>
        /// Interval of data granularidy by datasource type
        /// </summary>
        public double Interval { get; }
    }
}