using PingCastle.Healthcheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition
{
    public class CustomConsolidationData
    {
        #region Properties
        public Dictionary<string, CustomHealthCheckData> DomainsData { get; set; } = new Dictionary<string, CustomHealthCheckData>();
        public List<CustomRiskRuleCategory> Categories { get; set; } = new List<CustomRiskRuleCategory>();
        public List<CustomRiskModelCategory> Models { get; set; } = new List<CustomRiskModelCategory>();
        public List<CustomHealthCheckRiskRule> HealthRules { get; set; } = new List<CustomHealthCheckRiskRule>();
        public List<CustomRiskRule> RiskRules { get; set; } = new List<CustomRiskRule>();
        #endregion

        #region Fields
        private readonly Dictionary<string, CustomRiskRuleCategory> dictCategories = new Dictionary<string, CustomRiskRuleCategory>();
        private readonly Dictionary<string, CustomRiskModelCategory> dictModels = new Dictionary<string, CustomRiskModelCategory>();
        private readonly Dictionary<string, CustomHealthCheckRiskRule> dictHealthRules = new Dictionary<string, CustomHealthCheckRiskRule>();
        private readonly Dictionary<string, CustomRiskRule> dictRiskRules = new Dictionary<string, CustomRiskRule>();
        #endregion

        #region Constructors
        public CustomConsolidationData()
        {

        }
        #endregion

        #region Methods
        public bool AddData(string filename)
        {
            try
            {
                if(CustomHealthCheckData.LoadXML(filename, out var data))
                {
                    if (!string.IsNullOrEmpty(data.Domain))
                    {
                        DomainsData[data.Domain] = data;
                        return true;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Problem on 'AddData' method on 'CustomConsolidationData':");
                Console.WriteLine(e);
            }
            return false;
        }

        public void MergeDomainData(HealthcheckData domain)
        {
            try
            {
                if (DomainsData.ContainsKey(domain.DomainFQDN))
                {
                    DomainsData[domain.DomainFQDN].FillData(domain);
                    DomainsData[domain.DomainFQDN].MergeData(domain);
                    ConsolidateDomain(domain);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'MergeDomainData' method on 'CustomConsolidationData':");
                Console.WriteLine(e);
            }
        }

        private void ConsolidateDomain(HealthcheckData domain)
        {
            try
            {
                foreach (var category in DomainsData[domain.DomainFQDN].Categories)
                {
                    if (!dictCategories.ContainsKey(category.Id))
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
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'ConsolidateDomain' method on 'CustomConsolidationData':");
                Console.WriteLine(e);
            }
        }

        public bool GetRiskRule(string id, out CustomRiskRule riskRule)
        {
            try
            {
                if (dictRiskRules.ContainsKey(id))
                {
                    riskRule = dictRiskRules[id];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetRiskRule' method on 'CustomConsolidationData':");
                Console.WriteLine(e);
            }
            riskRule = null;
            return false;
        }
        #endregion
    }
}
