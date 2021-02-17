using PingCastle.Data;
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
        #endregion

        #region Fields
        private Dictionary<string, int> dictCategories;
        #endregion

        #region Constructors
        private CustomHealthCheckData()
        {
            Categories = new List<CustomRiskRuleCategory>();
            Models = new List<CustomRiskModelCategory>();
            RiskRules = new List<CustomRiskRule>();
            HealthRules = new List<CustomHealthCheckRiskRule>();
            dictCategories = new Dictionary<string, int>();
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

        public CustomRiskRule GetRiskRule(string ruleId)
        {
            foreach (var rule in RiskRules)
            {
                if (rule.Id == ruleId)
                {
                    return rule;
                }
            }
            return null;
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
        #endregion
    }
}
