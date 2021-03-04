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
        public List<CustomRiskRuleCategory> Categories { get; set; }
        public List<CustomRiskModelCategory> Models { get; set; }
        public List<CustomHealthCheckRiskRule> HealthRules { get; set; }
        public List<CustomRiskRule> RiskRules { get; set; }
        #endregion

        #region Fields
        private Dictionary<string, CustomRiskRuleCategory> dictCategories;
        private Dictionary<string, CustomRiskModelCategory> dictModels;
        private Dictionary<string, CustomHealthCheckRiskRule> dictHealthRules;
        private Dictionary<string, CustomRiskRule> dictRiskRules;
        #endregion

        #region Constructors
        public CustomConsolidationData()
        {
            DomainsData = new Dictionary<string, CustomHealthCheckData>();
            Categories = new List<CustomRiskRuleCategory>();
            Models = new List<CustomRiskModelCategory>();
            HealthRules = new List<CustomHealthCheckRiskRule>();
            RiskRules = new List<CustomRiskRule>();

            dictCategories = new Dictionary<string, CustomRiskRuleCategory>();
            dictModels = new Dictionary<string, CustomRiskModelCategory>();
            dictHealthRules = new Dictionary<string, CustomHealthCheckRiskRule>();
            dictRiskRules = new Dictionary<string, CustomRiskRule>();
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
                DomainsData[domain.DomainFQDN].FillData(domain);
                DomainsData[domain.DomainFQDN].MergeData(domain);
                ConsolidateDomain(domain);
            }
        }

        private void ConsolidateDomain(HealthcheckData domain)
        {
            foreach(var category in DomainsData[domain.DomainFQDN].Categories)
            {
                if(!dictCategories.ContainsKey(category.Id))
                {
                    dictCategories[category.Id] = category;
                    Categories.Add(category);
                }
            }

            foreach (var model in DomainsData[domain.DomainFQDN].Models)
            {
                if (!dictModels.ContainsKey(model.Id))
                {
                    dictModels[model.Id] = model;
                    Models.Add(model);
                }
            }

            foreach (var hcrule in DomainsData[domain.DomainFQDN].HealthRules)
            {
                if (!dictHealthRules.ContainsKey(hcrule.RiskId))
                {
                    dictHealthRules[hcrule.RiskId] = hcrule;
                    HealthRules.Add(hcrule);
                }
            }
            foreach (var rule in DomainsData[domain.DomainFQDN].RiskRules)
            {
                if (!dictRiskRules.ContainsKey(rule.Id))
                {
                    dictRiskRules[rule.Id] = rule;
                    RiskRules.Add(rule);
                }
            }
        }

        public bool GetRiskRule(string id, out CustomRiskRule riskRule)
        {
            if (dictRiskRules.ContainsKey(id))
            {
                riskRule =  dictRiskRules[id];
                return true;
            }
            riskRule = null;
            return false;
        }
        #endregion
    }
}
