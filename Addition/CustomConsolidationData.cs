using PingCastle.Healthcheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition
{
    public class CustomConsolidationData
    {
        #region Properties
        public Dictionary<string, CustomHealthCheckData> DomainsData { get; set; }
        #endregion

        #region Constructors
        public CustomConsolidationData()
        {
            DomainsData = new Dictionary<string, CustomHealthCheckData>();
        }
        #endregion

        #region Methods
        public bool AddData(string filename)
        {
            var data = CustomHealthCheckData.LoadXML(filename);
            if (!string.IsNullOrEmpty(data.Domain))
            {
                DomainsData[data.Domain] = data;
                return true;
            }
            else
                return false;
        }

        public void MergeDomainData(HealthcheckData domain)
        {
            if (DomainsData.ContainsKey(domain.DomainFQDN))
            {
                DomainsData[domain.DomainFQDN].MergeData(domain);
                DomainsData[domain.DomainFQDN].FillData(domain);
            }
        }
        #endregion
    }
}
