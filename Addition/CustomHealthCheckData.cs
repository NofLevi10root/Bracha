﻿using PingCastle.Data;
using PingCastle.Healthcheck;
using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    [XmlRoot("CustomHealthCheckData")]
    public class CustomHealthCheckData
    {
        #region Properties
        [XmlArray("RiskRuleCategories")]
        [XmlArrayItem(ElementName = "RiskRuleCategory")]
        public List<CustomRiskRuleCategory> Categories { get; set; }

        [XmlArray("RiskModelCategories")]
        [XmlArrayItem(ElementName = "RiskModelCategory")]
        public List<CustomRiskModelCategory> Models { get; set; }

        [XmlArray("RiskRules")]
        [XmlArrayItem(ElementName = "RiskRule")]
        public List<CustomRiskRule> RiskRules { get; set; }

        [XmlArray("HealthcheckRiskRules")]
        [XmlArrayItem(ElementName = "HealthcheckRiskRule")]
        public List<CustomHealthCheckRiskRule> HealthRules { get; set; }

        [XmlArray("InformationSections")]
        [XmlArrayItem(ElementName = "InformationSection")]
        public List<CustomInformationSection> InformationSections { get; set; }

        [XmlIgnore]
        public Dictionary<string, List<List<string>>> DictCustomTables { get; set; } // [tableName/SectionName][rows][cols]
        #endregion

        #region Fields
        private string dataDirectory;
        private readonly Dictionary<string, CustomRiskRuleCategory> dictCategories;
        private readonly Dictionary<string, CustomRiskModelCategory> dictModels;
        private readonly Dictionary<string, CustomRiskRule> dictRiskRules;    
        
        #endregion

        #region Constructors
        private CustomHealthCheckData()
        {
            Categories = new List<CustomRiskRuleCategory>();
            Models = new List<CustomRiskModelCategory>();
            RiskRules = new List<CustomRiskRule>();
            HealthRules = new List<CustomHealthCheckRiskRule>();

            dictCategories = new Dictionary<string, CustomRiskRuleCategory>();
            dictModels = new Dictionary<string, CustomRiskModelCategory>();
            dictRiskRules = new Dictionary<string, CustomRiskRule>();
            DictCustomTables = new Dictionary<string, List<List<string>>>();
        }
        #endregion

        #region Methods
        public static CustomHealthCheckData LoadXML(string filename)
        {
            var output = new CustomHealthCheckData();
            using (Stream fs = File.OpenRead(filename))
            {
                XmlDocument xmlDoc = LoadXmlDocument(fs, filename);
                XmlSerializer xs = new XmlSerializer(typeof(CustomHealthCheckData));
                output = (CustomHealthCheckData)xs.Deserialize(new XmlNodeReader(xmlDoc));
            }
            output.dataDirectory = Path.GetDirectoryName(filename);
            return output;
        }
        private static XmlDocument LoadXmlDocument(Stream report, string filenameForDebug)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            try
            {
                xmlDoc.Load(report);
            }
            catch (XmlException ex)
            {
                Trace.WriteLine("Invalid xml " + ex.Message);
                Trace.WriteLine("Trying to recover");
                StreamReader reader = new StreamReader(report);
                string xml = reader.ReadToEnd();
                try
                {
                    xmlDoc.LoadXml(xml);
                }
                catch (XmlException ex2)
                {
                    throw new PingCastleDataException(filenameForDebug, "Unable to parse the xml (" + ex2.Message + ")");
                }
            }
            return xmlDoc;
        }
        public void FillData(HealthcheckData healthData)
        {
            #region Add Categories To Dictionary
            foreach (var category in Categories)
            {
                if (!dictCategories.ContainsKey(category.Id))
                    dictCategories.Add(category.Id, category);
            }
            #endregion
            #region Add Models To Dictionary
            foreach (var model in Models)
            {
                if (!dictModels.ContainsKey(model.Id))
                    dictModels.Add(model.Id, model);
            }
            #endregion
            #region Add Risk Rules To Dictinary
            foreach (var riskRule in RiskRules)
            {
                if (!dictRiskRules.ContainsKey(riskRule.Id))
                    dictRiskRules.Add(riskRule.Id, riskRule);
                if (dictModels.ContainsKey(riskRule.Model))
                {
                    riskRule.Category = dictModels[riskRule.Model].Category;
                }
                    
            }
            #endregion
            #region Fill Health Risk Rules Data
            foreach (var healthRule in HealthRules)
            {
                var rule = GetRiskRule(healthRule.RiskId);
                if (rule != null)
                {
                    rule.SetReportLocation();
                    rule.SetDocumentation();
                    healthRule.Category = rule.Category;
                    healthRule.Model = rule.Model;
                    #region Get Details
                    if (healthRule.RuleDetails != null)
                    {
                        foreach(var detail in healthRule.RuleDetails)
                        {
                            if (detail.FilePath.StartsWith(@".\"))
                                detail.FilePath = dataDirectory + "\\" + detail.FilePath.Substring(2);
                            if(detail.Type == CustomDetailsType.Table)
                            {
                                healthRule.Details = detail.ParseToDetails();
                            }
                            else if(detail.Type == CustomDetailsType.SharedTable)
                            {
                                if (string.IsNullOrEmpty(detail.SectionId))
                                    continue;
                                if (!DictCustomTables.ContainsKey(detail.SectionId))
                                    DictCustomTables.Add(detail.SectionId, new List<List<string>>());
                                detail.AddSharedTableData(DictCustomTables[detail.SectionId]);
                            }
                        }
                        
                    }
                    #endregion
                    #region Add Point To Category
                    if (Enum.IsDefined(typeof(RiskRuleCategory), healthRule.Category)) // add points
                    {
                        switch (healthRule.Category)
                        {
                            case "Anomalies":
                                healthData.AnomalyScore = Math.Min(100, healthData.AnomalyScore + healthRule.Points);
                                break;
                            case "PrivilegedAccounts":
                                healthData.PrivilegiedGroupScore = Math.Min(100, healthData.PrivilegiedGroupScore + healthRule.Points);
                                break;
                            case "StaleObjects":
                                healthData.StaleObjectsScore = Math.Min(100, healthData.StaleObjectsScore + healthRule.Points);
                                break;
                            case "Trusts":
                                healthData.TrustScore = Math.Min(100, healthData.TrustScore + healthRule.Points);
                                break;
                        }
                    }
                    else
                    {
                        var category = dictCategories[healthRule.Category];
                        category.Score = Math.Min(100, category.Score + healthRule.Points);
                    }
                    #endregion
                }
            }
            #endregion
            #region Set Global Score
            healthData.GlobalScore = Math.Max(healthData.AnomalyScore, healthData.PrivilegiedGroupScore);
            healthData.GlobalScore = Math.Max(healthData.GlobalScore, healthData.StaleObjectsScore);
            healthData.GlobalScore = Math.Max(healthData.GlobalScore, healthData.TrustScore);
            foreach (var category in Categories)
            {
                healthData.GlobalScore = Math.Max(healthData.GlobalScore, category.Score);
            }
            #endregion

        }
        public void MergeData(HealthcheckData healthData)
        {
            healthData.MaturityLevel = GetMaturityLevel(healthData.MaturityLevel);
        }
        public int CountCategoryHealthRules(string category)
        {
            int output = 0;
            foreach (var rule in HealthRules)
            {
                if (rule.Category == category)
                    output++;
            }
            return output;
        }
        public int CountCategoryRiskRules(string category)
        {
            int output = 0;
            foreach (var rule in RiskRules)
            {
                if (rule.Category == category)
                    output++;
            }
            return output;
        }
        public CustomRiskRule GetRiskRule(string ruleId)
        {
            return dictRiskRules.ContainsKey(ruleId) ? dictRiskRules[ruleId] : null;
        }
        private int GetMaturityLevel(int oldMaturity)
        {
            int min = oldMaturity;
            foreach(var rule in HealthRules)
            {
                var hcrule = GetRiskRule(rule.RiskId);
                if(hcrule != null)
                {
                    min = Math.Min(min, hcrule.Maturity);
                }
            }
            return min;
        }
        public List<string> GetCustomTableHeaders(string table)
        {
            List<string> output = new List<string>();
            if (DictCustomTables.ContainsKey(table))
            {
                for (int i = 1; i < DictCustomTables[table][0].Count; i++)
                {
                    output.Add(DictCustomTables[table][0][i]);
                }
            }
            return output;
        }
        public List<string> GetCustomTableRow(string table, string key)
        {
            List<string> output = new List<string>();
            if (DictCustomTables.ContainsKey("operatingsystems"))
            {
                for (int row = 1; row < DictCustomTables["operatingsystems"].Count; row++) // find key (Correct row)
                {
                    if (key == DictCustomTables["operatingsystems"][row][0])
                    {
                        for (int col = 1; col < DictCustomTables["operatingsystems"][0].Count; col++)
                        {
                            output.Add(DictCustomTables["operatingsystems"][row][col]);
                        }
                    }
                }
            }
            return output;
        }
        #endregion
    }
}
