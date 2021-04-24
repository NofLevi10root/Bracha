using PingCastle.Data;
using PingCastle.Healthcheck;
using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    [XmlRoot("CustomHealthCheckData")]
    public class CustomHealthCheckData
    {
        #region Properties
        [XmlElement("Domain")]
        public string Domain { get; set; }

        [XmlArray("Categories")]
        [XmlArrayItem(ElementName = "Category")]
        public List<CustomRiskRuleCategory> Categories { get; set; }

        [XmlArray("Models")]
        [XmlArrayItem(ElementName = "Model")]
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

        [XmlArray("Tables")]
        [XmlArrayItem(ElementName = "Table")]
        public List<CustomTable> Tables { get; set; }
        [XmlArray("Charts")]
        [XmlArrayItem(ElementName = "Chart")]
        public List<CustomChart> Charts { get; set; }
        #endregion

        #region Fields
        private string dataDirectory;
        private readonly Dictionary<string, CustomRiskRuleCategory> dictCategories;
        private readonly Dictionary<string, CustomRiskModelCategory> dictModels;
        private readonly Dictionary<string, CustomRiskRule> dictRiskRules;
        private readonly Dictionary<string, CustomTable> dictTables;
        private readonly Dictionary<string, CustomInformationSection> dictSections;
        private readonly Dictionary<string, CustomChart> dictCharts;
        #endregion

        #region Constructors
        private CustomHealthCheckData()
        {
            Categories = new List<CustomRiskRuleCategory>();
            Models = new List<CustomRiskModelCategory>();
            RiskRules = new List<CustomRiskRule>();
            HealthRules = new List<CustomHealthCheckRiskRule>();
            InformationSections = new List<CustomInformationSection>();
            Tables = new List<CustomTable>();
            Charts = new List<CustomChart>();

            dictCategories = new Dictionary<string, CustomRiskRuleCategory>();
            dictModels = new Dictionary<string, CustomRiskModelCategory>();
            dictRiskRules = new Dictionary<string, CustomRiskRule>();
            dictTables = new Dictionary<string, CustomTable>();
            dictSections = new Dictionary<string, CustomInformationSection>();
            dictCharts = new Dictionary<string, CustomChart>();
        }
        #endregion

        #region Methods
        #region XML
        public static bool LoadXML(string filename, out CustomHealthCheckData result)
        {
            result = new CustomHealthCheckData();
            try
            {
                using (Stream fs = File.OpenRead(filename))
                {
                    //XmlDocument xmlDoc = LoadXmlDocument(fs, filename);
                    XmlDocument xmlDoc = LoadXmlDocument(filename);
                    XmlSerializer xs = new XmlSerializer(typeof(CustomHealthCheckData));
                    result = (CustomHealthCheckData)xs.Deserialize(new XmlNodeReader(xmlDoc));
                }

            }
            catch(Exception e)
            {
                ShowMessage($"Failed loading data from : {filename}", ConsoleColor.Red);
                ShowMessage($"{e.Message}", ConsoleColor.Yellow);
                ShowMessage($"{e.StackTrace}", ConsoleColor.Red);
                return false;
            }
            result.dataDirectory = Path.GetDirectoryName(filename);
            return true;
        }
        
        private static XmlDocument LoadXmlDocument(string fileName)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add("", $"{Directory.GetCurrentDirectory()}\\Addition\\DataScheme.xsd");
                settings.ValidationType = ValidationType.Schema;

                XmlReader reader = XmlReader.Create(fileName, settings);
                XmlDocument document = new XmlDocument();
                document.PreserveWhitespace = true;
                document.Load(reader);

                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                // the following call to Validate succeeds.
                document.Validate(eventHandler);
                return document;
            }
            catch (Exception e)
            {
                throw new PingCastleDataException(fileName, "Unable to parse the xml (" + e.Message + ")");
            }
        }
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    break;
            }
        }
        #endregion
        public void FillData(HealthcheckData healthData)
        {
            #region Add Sections
            foreach (var section in InformationSections)
                dictSections.Add(section.Id, section);
            #endregion
            #region Add Tables
            foreach (var table in Tables)
            {
                dictTables.Add(table.Id, table);
                table.SetInitData();
                table.SetLinksToSections(dictSections);
            }
            #endregion

            #region Add Charts
            foreach (var chart in Charts)
            {
                dictCharts.Add(chart.Id, chart);
            }
            #endregion
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
            #region Add Risk Rules To Dictionary
            foreach (var riskRule in RiskRules)
            {
                if (!dictRiskRules.ContainsKey(riskRule.Id))
                    dictRiskRules.Add(riskRule.Id, riskRule);
                foreach(string model in riskRule.Models)
                {
                    riskRule.AddModelToDictionary(model);
                    if (dictModels.ContainsKey(model))
                    {
                        riskRule.AddCategory(dictModels[model].Category);
                    }
                }
            }
            #endregion
            #region Fill Health Risk Rules Data
            foreach (var healthRule in HealthRules)
            {
                if (GetRiskRule(healthRule.RiskId, out var rule))
                {
                    rule.SetReportLocation();
                    rule.SetDocumentation();
                    foreach(var category in rule.Categories)
                    {
                        healthRule.AddCategory(category);
                    }
                    foreach (var model in rule.Models)
                    {
                        healthRule.AddModel(model);
                    }
                    #region Get Details
                    if (healthRule.RuleDetails != null)
                    {
                        foreach(var detail in healthRule.RuleDetails)
                        {
                            if (detail.FilePath.StartsWith(@".\"))
                                detail.FilePath = dataDirectory + "\\" + detail.FilePath.Substring(2);
                            if(detail.Type == CustomDetailsType.SharedChart)
                            {
                                if (string.IsNullOrEmpty(detail.Id))
                                    continue;

                                if (dictCharts.ContainsKey(detail.Id))
                                    dictCharts[detail.Id].AddDetail(detail);
                            }
                            else if(detail.Type == CustomDetailsType.SharedTable)
                            {
                                if (string.IsNullOrEmpty(detail.Id))
                                    continue;
                                if (dictTables.ContainsKey(detail.Id))
                                    dictTables[detail.Id].AddDetail(detail);

                            }
                        }
                        
                    }
                    #endregion
                    #region Add Point To Category
                    foreach(var categoryName in healthRule.Categories)
                    {
                        if (Enum.IsDefined(typeof(RiskRuleCategory), categoryName)) // add points
                        {
                            switch (categoryName)
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
                            var category = dictCategories[categoryName];
                            category.Score = Math.Min(100, category.Score + healthRule.Points);
                        }
                    }
                    #endregion
                }
            }
            #endregion
            

        }
        public void MergeData(HealthcheckData healthData)
        {
            healthData.MaturityLevel = GetMaturityLevel(healthData.MaturityLevel);
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
        public int CountCategoryHealthRules(string category)
        {
            int output = 0;
            foreach (var rule in HealthRules)
            {
                if(rule.CheckIsInCategory(category))
                    output++;
            }
            return output;
        }
        public int CountCategoryRiskRules(string category)
        {
            int output = 0;
            foreach (var rule in RiskRules)
            {
                if(rule.CheckIsInCategory(category))
                    output++;
            }
            return output;
        }
        private int GetMaturityLevel(int oldMaturity)
        {
            int min = oldMaturity;
            foreach(var rule in HealthRules)
            {
                if(GetRiskRule(rule.RiskId, out var hcrule))
                {
                    min = Math.Min(min, hcrule.Maturity);
                }
            }
            return min;
        }
        public List<string> GetCustomTableRow(string table, string key)
        {
            List<string> output = new List<string>();
            if (dictTables.ContainsKey(table))
            {
                for(int col = 1; col < dictTables[table].Columns.Count; col++)
                {
                    if (dictTables[table].Columns[col].Values.ContainsKey(key))

                        output.Add(dictTables[table].Columns[col].Values[key]);
                }
            }
            return output;
        }
        public bool GetRiskRule(string ruleId, out CustomRiskRule riskRule)
        {
            if(dictRiskRules.ContainsKey(ruleId))
            {
                riskRule = dictRiskRules[ruleId];
                return true;
            }
            riskRule = null;
            return false;
        }
        public bool GetSection(string id, out CustomInformationSection section)
        {
            if (dictSections.ContainsKey(id))
            {
                section = dictSections[id];
                return true;
            }
            section = null;
            return false;
        }
        public bool GetTable(string id, out CustomTable table)
        {
            if (dictTables.ContainsKey(id))
            {
                table = dictTables[id];
                return true;
            }
            table = null;
            return false;
        }
        public bool GetChart(string id, out CustomChart chart)
        {
            if (dictCharts.ContainsKey(id))
            {
                chart = dictCharts[id];
                return true;
            }
            chart = null;
            return false;
        }
        public bool GetCategory(string id, out CustomRiskRuleCategory category)
        {
            if (dictCategories.ContainsKey(id))
            {
                category =  dictCategories[id];
                return true;
            }
            category = null;
            return false;
        }

        public static void ShowMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        #endregion
    }
}
