using PingCastle.Addition.ReportEnteties;
using PingCastle.Addition.StructureEnteties;
using PingCastle.Addition.StructuresEnteties;
using PingCastle.Data;
using PingCastle.Healthcheck;
using PingCastle.Properties;
using PingCastle.Report;
using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using static PingCastle.Report.ReportBase;

namespace PingCastle.Addition.LogicEnteties
{
    [XmlRoot("CustomHealthCheckData")]
    public class CustomHealthCheckData : ICustomData
    {
        #region Properties
        [XmlElement("Domain")]
        public string Domain { get; set; }

        [XmlElement("CustomDelimiter")]
        public string CustomDelimiter { get; set; } = ",";

        [XmlArray("Categories")]
        [XmlArrayItem(ElementName = "Category")]
        public List<CustomRiskRuleCategory> Categories { get; set; } = new List<CustomRiskRuleCategory>();

        [XmlArray("Models")]
        [XmlArrayItem(ElementName = "Model")]
        public List<CustomRiskModelCategory> Models { get; set; } = new List<CustomRiskModelCategory>();

        [XmlArray("RiskRules")]
        [XmlArrayItem(ElementName = "RiskRule")]
        public List<CustomRiskRule> RiskRules { get; set; } = new List<CustomRiskRule>();

        [XmlArray("HealthcheckRiskRules")]
        [XmlArrayItem(ElementName = "HealthcheckRiskRule")]
        public List<CustomHealthCheckRiskRule> HealthRules { get; set; } = new List<CustomHealthCheckRiskRule>();

        [XmlArray("InformationSections")]
        [XmlArrayItem(ElementName = "InformationSection")]
        public List<CustomInformationSection> InformationSections { get; set; } = new List<CustomInformationSection>();

        [XmlArray("Tables")]
        [XmlArrayItem(ElementName = "Table")]
        public List<CustomTable> Tables { get; set; } = new List<CustomTable>();

        [XmlArray("Charts")]
        [XmlArrayItem(ElementName = "Chart")]
        public List<CustomChart> Charts { get; set; } = new List<CustomChart>();

        [XmlIgnore]
        public bool IsEmpty { get; set; }

        [XmlIgnore]
        public int GlobalScore { get; set; }

        [XmlIgnore]
        public ComplinceScores ComplinceScores { get; set; }

        [XmlIgnore]
        public ThreatHuntingScores ThreatHuntingScores { get; set; }

        [XmlIgnore]
        public YaraScores YaraScores { get; set; }

        [XmlIgnore]
        public WesngScores WesngScores { get; set; }

        [XmlIgnore]
        public SnafflerScores SnafflerScores { get; set; }
        #endregion

        #region Fields
        private string dataDirectory;
        private readonly Dictionary<string, CustomRiskRuleCategory> dictCategories = new Dictionary<string, CustomRiskRuleCategory>();
        private readonly Dictionary<string, CustomRiskModelCategory> dictModels = new Dictionary<string, CustomRiskModelCategory>();
        private readonly Dictionary<string, CustomRiskRule> dictRiskRules = new Dictionary<string, CustomRiskRule>();
        private readonly Dictionary<string, CustomTable> dictTables = new Dictionary<string, CustomTable>();
        private readonly Dictionary<string, CustomInformationSection> dictSections = new Dictionary<string, CustomInformationSection>();
        private readonly Dictionary<string, CustomChart> dictCharts = new Dictionary<string, CustomChart>();
        private CustomMethodsReferenceManager refsManager = new CustomMethodsReferenceManager();
        #endregion

        #region Constructors
        private CustomHealthCheckData()
        {
        }
        #endregion

        #region Methods
        #region XML

        public static CustomHealthCheckData CreateEmpty()
        {
            return new CustomHealthCheckData() { IsEmpty = true };
        }
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
                    result.dataDirectory = Path.GetDirectoryName(filename);
                    return true;
                }

            }
            catch (Exception e)
            {
                ShowMessage($"Failed loading data from : {filename}", ConsoleColor.Red);
                ShowMessage($"{e.Message}", ConsoleColor.Yellow);
                ShowMessage($"{e.StackTrace}", ConsoleColor.Red);
                return false;
            }
        }

        private static XmlDocument LoadXmlDocument(string fileName)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();

                settings.Schemas.Add("", XmlReader.Create(new StringReader(Resources.DataScheme)));
                settings.ValidationType = ValidationType.Schema;
                settings.CheckCharacters = false;


                XmlReader reader = XmlReader.Create(fileName, settings);

                XmlDocument document = new XmlDocument();
                document.PreserveWhitespace = true;
                document.Load(reader);

                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                // the following call to Validate succeeds.
                document.Validate(eventHandler);
                return document;
            }
            catch (XmlSchemaValidationException e)
            {
                throw new PingCastleDataException(fileName, $"Unable to parse the xml ({e.Message})\nPosition [Line: {e.LineNumber} | Char: {e.LinePosition}]");
            }
            catch (XmlException e)
            {
                throw new PingCastleDataException(fileName, $"Unable to parse the xml ({e.Message})\nPosition [Line: {e.LineNumber} | Char: {e.LinePosition}]");
            }
            catch (Exception e)
            {
                throw new PingCastleDataException(fileName, $"Unable to parse the xml ({e.Message})");
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
            try
            {
                #region Add Sections
                foreach (var section in InformationSections)
                    dictSections.Add(section.Id, section);
                #endregion
                #region Add Tables
                foreach (var table in Tables)
                {
                    dictTables.Add(table.Id, table);
                    table.SetInitData(dataDirectory);
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
                    foreach (string model in riskRule.Models)
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
                ComplinceScores = new ComplinceScores();
                ThreatHuntingScores = new ThreatHuntingScores();
                YaraScores = new YaraScores();
                WesngScores = new WesngScores();
                SnafflerScores = new SnafflerScores();
                foreach (var healthRule in HealthRules)
                {
                    if (GetRiskRule(healthRule.RiskId, out var rule))
                    {
                        rule.SetReportLocation();
                        rule.SetDocumentation();
                        foreach (var category in rule.Categories)
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
                            foreach (var detail in healthRule.RuleDetails)
                            {
                                if (detail.FilePath.StartsWith(@".\"))
                                    detail.FilePath = dataDirectory + "\\" + detail.FilePath.Substring(2);
                                if (detail.Type == CustomDetailsType.SharedChart)
                                {
                                    if (string.IsNullOrEmpty(detail.Id))
                                        continue;

                                    if (dictCharts.ContainsKey(detail.Id))
                                        dictCharts[detail.Id].AddDetail(detail);
                                }
                                else if (detail.Type == CustomDetailsType.SharedTable)
                                {
                                    if (string.IsNullOrEmpty(detail.Id))
                                        continue;
                                    if (dictTables.ContainsKey(detail.Id))
                                    {
                                        var customTableScores = dictTables[detail.Id].AddDetail(detail, CustomDelimiter);
                                        if (customTableScores != null)
                                        {
                                            if (customTableScores is ComplinceScores complince)
                                            {
                                                ComplinceScores = complince;
                                            }
                                            if (customTableScores is ThreatHuntingScores zircolite)
                                            {
                                                ThreatHuntingScores = zircolite;
                                            }
                                            if (customTableScores is YaraScores yara)
                                            {
                                                YaraScores = yara;
                                            }
                                            if (customTableScores is WesngScores wesng)
                                            {
                                                WesngScores = wesng;
                                            }
                                            if (customTableScores is SnafflerScores snaffler)
                                            {
                                                SnafflerScores = snaffler;
                                            }
                                        }
                                    }

                                }
                            }

                        }
                        #endregion
                        #region Add Point To Category
                        foreach (var categoryName in healthRule.Categories)
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
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'FillData' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
        }
        public void MergeData(HealthcheckData healthData)
        {
            try
            {
                healthData.MaturityLevel = GetMaturityLevel(healthData.MaturityLevel);
                #region Set Global Score
                healthData.GlobalScore = Math.Max(healthData.AnomalyScore, healthData.PrivilegiedGroupScore);
                healthData.GlobalScore = Math.Max(healthData.GlobalScore, healthData.StaleObjectsScore);
                healthData.GlobalScore = Math.Max(healthData.GlobalScore, healthData.TrustScore);
                foreach (var category in Categories)
                {
                    GlobalScore = Math.Max(GlobalScore, category.Score);
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'MergeData' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
        }
        private int GetMaturityLevel(int oldMaturity)
        {
            int min = oldMaturity;
            try
            {
                foreach (var rule in HealthRules)
                {
                    if (GetRiskRule(rule.RiskId, out var hcrule))
                    {
                        min = Math.Min(min, hcrule.Maturity);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetMaturityLevel' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
            return min;
        }
        public void AddToMaturityLevelsDict(Dictionary<int, List<string>> maturityDict)
        {
            foreach (var rule in HealthRules)
            {
                if (!GetRiskRule(rule.RiskId, out var hcrule))
                    continue;
                int level = hcrule.Maturity;
                if (!maturityDict.ContainsKey(level))
                    maturityDict[level] = new List<string>();
                maturityDict[level].Add(hcrule.Id);
            }
        }

        public void AddHealthRulesToCurrentMaturityLevel(List<string> level, SortedDictionary<int, List<object>> levelRules)
        {
            foreach (var rule in HealthRules)
            {
                if (level.Contains(rule.RiskId))
                {
                    if (!levelRules.ContainsKey(rule.Points))
                        levelRules.Add(rule.Points, new List<object>());
                    levelRules[rule.Points].Add(rule);
                }
            }
        }

        public void SetRefsManager(CustomMethodsReferenceManager referenceManager)
        {
            refsManager = referenceManager;
        }

        public static void ShowMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        #region Health Rules
        public int CountCategoryHealthRules(string category)
        {
            try
            {

                int output = 0;
                foreach (var rule in HealthRules)
                {
                    if (rule.Categories.Contains(category))
                    {
                        if (rule.RuleDetails != null)
                        {
                            foreach (var detail in rule.RuleDetails)
                            {
                                if (dictTables.ContainsKey(detail.Id))
                                {
                                    output += dictTables[detail.Id].Keys.Count;
                                }
                            }

                        }
                    }
                    else if (rule.CheckIsInCategory(category))
                        output++;
                }
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'CountCategoryHealthRules' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
                return 0;
            }

        }

        public bool CheckHasRule(string category)
        {
            foreach (CustomHealthCheckRiskRule rule in HealthRules)
            {
                if (rule.CheckIsInCategory(category))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Risk Rules
        public int CountCategoryRiskRules(string category)
        {
            try
            {
                int output = 0;
                foreach (var rule in RiskRules)
                {
                    if (rule.CheckIsInCategory(category))
                        output++;
                }
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'CountCategoryHealthRules' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
                return 0;
            }
        }

        public bool GetRiskRule(string ruleId, out CustomRiskRule riskRule)
        {
            try
            {
                if (dictRiskRules.ContainsKey(ruleId))
                {
                    riskRule = dictRiskRules[ruleId];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetRiskRule' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
            riskRule = null;
            return false;
        }

        #endregion

        #region Tables
        public List<string> GetCustomTableRow(string table, string key)
        {
            try
            {
                List<string> output = new List<string>();
                if (dictTables.ContainsKey(table))
                {
                    for (int col = 1; col < dictTables[table].Columns.Count; col++)
                    {
                        if (dictTables[table].Columns[col].Values.ContainsKey(key))

                            output.Add(dictTables[table].Columns[col].Values[key]);
                    }
                }
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetCustomTableRow' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
                return null;
            }
        }
        public bool GetTable(string id, out CustomTable table)
        {
            try
            {
                if (dictTables.ContainsKey(id))
                {
                    table = dictTables[id];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetTable' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }

            table = null;
            return false;
        }

        public CustomTable GenerateTableHeaders(string id)
        {
            if (GetTable(id, out var custTable))
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (!string.IsNullOrEmpty(custTable.Columns[i].Tooltip))
                        refsManager.AddHeaderTextRef(custTable.Columns[i].Header, custTable.Columns[i].Tooltip);
                    else
                        refsManager.AddHeaderTextRef(custTable.Columns[i].Header);
                }
                return custTable;
            }
            return null;
        }

        public void GenerateTableRowCells(string id, string key)
        {
            if (GetTable(id, out var custTable))
            {
                for (int i = 1; i < custTable.Columns.Count; i++)
                {
                    if (custTable.Columns[i].Values.ContainsKey(key))
                        refsManager.AddCellTextRef(custTable.Columns[i].Values[key]);
                    else
                        refsManager.AddCellTextRef("");
                }
            }
        }


        public void GenerateTableKeyModals<T, T2>(string id, IEnumerable<T> enumerable, Func<T, T2> func, Predicate<T> predicate = null)
        {
            if (GetTable(id, out _) && enumerable != null)
            {
                foreach (T item in enumerable)
                {
                    if (predicate == null || predicate(item))
                    {
                        AddTableKeyModal(id, func(item));
                    }
                }
            }

        }

        public void GenerateTableKeyModals<T, T2>(string id, IEnumerable<T> enumerable, Func<T, T2> case1, Func<T, T2> case2, bool isCase1, Predicate<T> predicate = null)
        {
            if (GetTable(id, out _) && enumerable != null)
            {
                foreach (T item in enumerable)
                {
                    if (predicate == null || predicate(item))
                    {
                        AddTableKeyModal(id, isCase1 ? case1(item) : case2(item));
                    }
                }
            }

        }

        public void AddTableKeyModal(string tableId, object cellValue)
        {
            try
            {
                if (cellValue == null || !GetTable(tableId, out CustomTable custTable))
                    return;
                string value = cellValue.ToString();
                if (custTable.GetKeyLinkedSection(value, out var targetSection))
                {
                    refsManager.AddBeginModalRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef(targetSection.Id), targetSection.Name, ShowModalType.XL);
                    GenerateAdvancedCustomSection(targetSection);
                    refsManager.AddEndModalRef(ShowModalType.XL);
                }
                else if (custTable.GetNestedTable(value, CustomDelimiter, out var targetTable))
                {
                    refsManager.AddBeginModalRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef($"table_{value}"), value, ShowModalType.XL);
                    AddCustomTableHtml(cellValue, targetTable, custTable);
                    refsManager.AddEndModalRef(ShowModalType.XL);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddTableKeyModal' method on 'ReportHealthCheckSingle':");
                Console.WriteLine(e);
            }
        }

        public void AddTableKeyCell(string id, object cellValue, string valueType = "string", string tooltip = null)
        {
            try
            {
                GetTable(id, out CustomTable custTable);
                if (cellValue == null)
                {
                    refsManager.AddCellTextRef("");
                    return;
                }
                string value = cellValue.ToString();
                if (custTable != null && custTable.GetKeyLinkedSection(value, out var targetSection))
                {
                    refsManager.AddRef(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    refsManager.AddRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef(targetSection.Id));
                    refsManager.AddRef(@""">");
                    refsManager.AddEncodedRef(value);
                    refsManager.AddRef("</a>");
                    AddCustomTooltip(tooltip);
                    refsManager.AddRef("</td>");
                }
                else if (custTable != null && custTable.GetNestedTable(value, CustomDelimiter, out var targetTable))
                {
                    refsManager.AddRef(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    refsManager.AddRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef($"table_{value}"));
                    refsManager.AddRef(@""">");
                    refsManager.AddEncodedRef(value);
                    refsManager.AddRef("</a>");
                    AddCustomTooltip(tooltip);
                    refsManager.AddRef("</td>");
                }
                else
                {
                    switch (valueType)
                    {
                        case "string":
                            refsManager.AddCellTextRef(value, tooltip: tooltip);
                            break;
                        case "number":
                            refsManager.AddCellNumRef((int)cellValue);
                            break;
                        case "header":
                            refsManager.AddHeaderTextRef(value);
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddTableKeyCell' method on 'ReportHealthCheckSingle':");
                Console.WriteLine(e);
            }
        }

        public void AddTableKeyCellIteration<T, T2>(string tableId, IEnumerable<T> enumerable, Func<T, T2> func)
        {
            foreach (var item in enumerable)
            {
                AddTableKeyCell(tableId, func(item));
            }
        }

        private void AddCustomTooltip(string tooltip)
        {
            try
            {
                if (!string.IsNullOrEmpty(tooltip))
                {
                    refsManager.AddBeginTooltipRef();
                    refsManager.AddRef(tooltip);
                    refsManager.AddEndTooltipRef();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddCustomTooltip' method on 'ReportHealthCheckSingle':");
                Console.WriteLine(e);
            }
        }

        private void AddCustomTableHtml(object cellValue, List<string> data, CustomTable custTable)
        {
            try
            {
                if (data.Count == 0)
                {
                    return;
                }
                var firstLineParts = data[0].Split(' ');
                if (firstLineParts.Length > 1 && firstLineParts[0].EndsWith(":"))
                {
                    var tokens = new List<string>();
                    for (int i = 0; i < firstLineParts.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(firstLineParts[i]) && firstLineParts[i].EndsWith(":"))
                        {
                            tokens.Add(firstLineParts[i]);
                        }
                    }
                    refsManager.AddRef(@"<div class=""row"">
			<div class=""col-md-12 table-responsive"">
				<table class=""table table-striped table-bordered"">
					<thead><tr>");
                    var headers = new List<string>();
                    foreach (var token in tokens)
                    {
                        refsManager.AddRef($"<th class='customTableColumn'>");
                        string parsedToken = token.Replace("#$%%$#", " ").Replace("#$%:%$#", ": ");
                        var header = parsedToken.Substring(0, parsedToken.Length - 1);
                        headers.Add(header);
                        refsManager.AddEncodedRef(header);
                        refsManager.AddRef("</th>");
                    }
                    refsManager.AddRef("</tr></thead><tbody>");
                    foreach (var d in data)
                    {
                        if (string.IsNullOrEmpty(d))
                            continue;
                        refsManager.AddRef("<tr>");
                        var t = d.Split(' ');
                        t = t.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        for (int i = 0, j = 0; i < t.Length && j <= tokens.Count; i++)
                        {
                            if (j < tokens.Count && t[i] == tokens[j])
                            {
                                if (j != 0)
                                {
                                    refsManager.AddRef("</td>");
                                }
                                j++;
                                refsManager.AddRef($"<td class='customTableColumn'>");
                            }
                            else
                            {
                                var value = t[i].Replace("#$%:%$#", ": ");
                                var resualt = string.Empty;
                                if (custTable != null)
                                {
                                    if (custTable.GetNestedColumnPath(headers[j - 1], value, out var spcialColumnValue))
                                    {
                                        foreach (var v in spcialColumnValue)
                                        {
                                            resualt += $@"<a target=""_blank"" href=""{v.Value}"">{v.Key}</a>";
                                            if (!(i == t.Length - 1))
                                            {
                                                resualt += ",";
                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(resualt))
                                {
                                    value = resualt;
                                }
                                refsManager.AddRef(value);
                                refsManager.AddRef(" ");
                            }
                        }
                        refsManager.AddRef("</td>");
                        refsManager.AddRef("</tr>");
                    }
                    if (!string.IsNullOrEmpty(custTable.MoreDetails))
                    {
                        string cellValueStr = (string)cellValue;
                        string computersFile = Path.Combine(custTable.MoreDetails, cellValue + ".csv");
                        refsManager.AddRef($@"</tbody></table><a class='moreDetailsLink' target=""_blank"" href=""{computersFile}""><b>More details</b></a></div></div>");
                    }
                    else
                        refsManager.AddRef("</tbody></table></div></div>");

                }
                else
                {
                    refsManager.AddRef("<p>");
                    refsManager.AddRef(String.Join("<br>\r\n", data.ToArray()));
                    refsManager.AddRef("</p>");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddCustomTableHtml' method on 'ReportHealthCheckSingle':");
                Console.WriteLine(e);
            }
        }

        public void AddGPOTableKeyCell(string tableId, IGPOReference cellValue, Dictionary<string, GPOInfo> GPOInfoDic)
        {
            try
            {
                GetTable(tableId, out CustomTable custTable);
                if (custTable != null && custTable.GetKeyLinkedSection(ReportHelper.Encode(cellValue.GPOName), out var targetSection))
                {
                    refsManager.AddRef(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    refsManager.AddRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef(targetSection.Id));
                    refsManager.AddRef(@""">");
                    refsManager.AddEncodedRef(cellValue.GPOName);
                    refsManager.AddRef("</a>");
                    if (!string.IsNullOrEmpty(cellValue.GPOId))
                    {
                        if (!GPOInfoDic.ContainsKey(cellValue.GPOId))
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Disabled]</span>");
                            return;
                        }
                        var refGPO = GPOInfoDic[cellValue.GPOId];
                        if (refGPO.IsDisabled)
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Disabled]</span>");
                        }
                        if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                        {
                            refsManager.AddBeginTooltipRef(true);
                            refsManager.AddRef("<div class='text-left'>Linked to:<br><ul>");
                            foreach (var i in refGPO.AppliedTo)
                            {
                                refsManager.AddRef("<li>");
                                refsManager.AddEncodedRef(i);
                                refsManager.AddRef("</li>");
                            }
                            refsManager.AddRef("</ul></div>");
                            refsManager.AddRef("<div class='text-left'>Technical id:<br>");
                            refsManager.AddEncodedRef(cellValue.GPOId);
                            refsManager.AddRef("</div>");
                            refsManager.AddEndTooltipRef();
                        }
                        else
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                            refsManager.AddBeginTooltipRef();
                            refsManager.AddRef("<div class='text-left'>Technical id:<br>");
                            refsManager.AddEncodedRef(cellValue.GPOId);
                            refsManager.AddRef("</div>");
                            refsManager.AddEndTooltipRef();
                        }
                    }
                    refsManager.AddRef("</td>");
                }
                else if (custTable != null && custTable.GetNestedTable(ReportHelper.Encode(cellValue.GPOName), CustomDelimiter, out var targetTable))
                {
                    refsManager.AddRef(@"<td class='text'><a data-toggle=""modal"" href=""#");
                    refsManager.AddRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef($"table_{ReportHelper.Encode(cellValue.GPOName)}"));
                    refsManager.AddRef(@""">");
                    refsManager.AddEncodedRef(cellValue.GPOName);
                    refsManager.AddRef("</a>");
                    if (!string.IsNullOrEmpty(cellValue.GPOId))
                    {
                        if (!GPOInfoDic.ContainsKey(cellValue.GPOId))
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Disabled]</span>");
                            return;
                        }
                        var refGPO = GPOInfoDic[cellValue.GPOId];
                        if (refGPO.IsDisabled)
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Disabled]</span>");
                        }
                        if (refGPO.AppliedTo != null && refGPO.AppliedTo.Count > 0)
                        {
                            refsManager.AddBeginTooltipRef(true);
                            refsManager.AddRef("<div class='text-left'>Linked to:<br><ul>");
                            foreach (var i in refGPO.AppliedTo)
                            {
                                refsManager.AddRef("<li>");
                                refsManager.AddEncodedRef(i);
                                refsManager.AddRef("</li>");
                            }
                            refsManager.AddRef("</ul></div>");
                            refsManager.AddRef("<div class='text-left'>Technical id:<br>");
                            refsManager.AddEncodedRef(cellValue.GPOId);
                            refsManager.AddRef("</div>");
                            refsManager.AddEndTooltipRef();
                        }
                        else
                        {
                            refsManager.AddRef(@" <span class=""font-weight-light"">[Not&nbsp;linked]</span>");
                            refsManager.AddBeginTooltipRef();
                            refsManager.AddRef("<div class='text-left'>Technical id:<br>");
                            refsManager.AddEncodedRef(cellValue.GPOId);
                            refsManager.AddRef("</div>");
                            refsManager.AddEndTooltipRef();
                        }
                    }
                    refsManager.AddRef("</td>");
                }
                else
                {
                    refsManager.AddGPONameRef(cellValue);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddGPOTableKeyCell' method on 'ReportHealthCheckSingle':");
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Sections
        public bool GetSection(string id, out CustomInformationSection section)
        {
            try
            {
                if (dictSections.ContainsKey(id))
                {
                    section = dictSections[id];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetSection' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
            section = null;
            return false;
        }

        public void GenerateAdvancedSection(string id)
        {
            if (GetSection(id, out var section))
            {
                GenerateAdvancedCustomSection(section);
                InformationSections.Remove(section);
            }
        }

        public void GenerateCustomInformationSections()
        {
            foreach (var section in InformationSections)
            {
                if (section.Show)
                {
                    CustomRiskRuleCategory sectionCategory = null;
                    foreach (var riskRule in RiskRules)
                    {
                        if (riskRule.SectionId == section.Id)
                        {
                            foreach (var categoty in riskRule.Categories)
                            {
                                var d = Categories.FirstOrDefault(c => c.Id == categoty);
                                if (d != null)
                                {
                                    sectionCategory = d;
                                }
                            }
                        }
                    }
                    if (sectionCategory != null)
                    {
                        refsManager.GenerateSectionRef(section.Id, section.Name, () =>
                        {
                            switch (sectionCategory.Id)
                            {
                                case "compliance_category_id":
                                    refsManager.AddParagraphRef(@"<p>Endpoint OS compliance check. Each endpoint is checked against a dedicated security authority baseline according to the OS version & Role.</p>");
                                    break;
                                case "zircolite_category_id":
                                    refsManager.AddParagraphRef(@"<p>Checking the Eventlog against the Sigma rules public repository & custom rules created by 10Root experts.</p>");
                                    break;
                                case "yara_category_id":
                                    refsManager.AddParagraphRef(@"<p>Scanning the endpoint for Yara rules matched files.</p>");
                                    break;
                                case "wesng_category_id":
                                    refsManager.AddParagraphRef(@"<p>Authenticated host vulnerability scanner based on OS patch level & MSRC DB.</p>");
                                    break;
                                case "snaffler_category_id":
                                    refsManager.AddParagraphRef(@"<p>Analyzing file's content and classify them according to data sensitivity</p>");
                                    break;
                            }
                            AddCustomCategoriesCharts(false, sectionCategory);
                            GenerateAdvancedCustomSection(section);
                        });
                    }
                }
            }
        }

        public void GenerateAdvancedCustomSection(CustomInformationSection section)
        {
            foreach (var child in section.Children)
            {
                switch (child.Type)
                {
                    case CustomSectionChildType.Table:
                        if (GetTable(child.Id, out var table))
                        {
                            refsManager.AddBeginTableRef(table.Id);

                            foreach (var col in table.Columns)
                            {
                                if (!string.IsNullOrEmpty(col.Tooltip))
                                    refsManager.AddHeaderTextRef(col.Header, col.Tooltip);
                                else
                                    refsManager.AddHeaderTextRef(col.Header);
                            }
                            refsManager.AddBeginTableDataRef();
                            foreach (var key in table.Keys)
                            {
                                refsManager.AddBeginRowRef();
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    if (i == 0)
                                    {
                                        AddTableKeyCell(table.Id, key);
                                    }
                                    else
                                    {
                                        if (table.Columns[i].Values.ContainsKey(key))
                                            refsManager.AddCellTextRef(table.Columns[i].Values[key]);
                                        else
                                            refsManager.AddCellTextRef("");
                                    }
                                }
                                refsManager.AddEndRowRef();
                            }
                            refsManager.AddEndTableRef();
                            GenerateTableKeyModals(table.Id, table.Keys, item => item);
                        }
                        break;
                    case CustomSectionChildType.Chart:
                        if (GetChart(child.Id, out var chart))
                            refsManager.AddRef(chart.GetChartString());
                        break;
                    case CustomSectionChildType.Paragraph:
                        if (!string.IsNullOrEmpty(child.Value))
                            refsManager.AddParagraphRef(child.Value);
                        break;
                    case CustomSectionChildType.SubSectionTitle:
                        if (!string.IsNullOrEmpty(child.Value))
                        {
                            refsManager.GenerateSubSectionRef(child.Value);
                        }
                        break;
                    case CustomSectionChildType.Modal:
                        if (!string.IsNullOrEmpty(child.Id) && !string.IsNullOrEmpty(child.Value) && child.Id != section.Id)
                        {
                            refsManager.AddRef(@"<td class='text'><a data-toggle=""modal"" href=""#");
                            refsManager.AddRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef(child.Id));
                            refsManager.AddRef(@""">");
                            refsManager.AddEncodedRef(child.Value);
                            refsManager.AddRef("</a></td>");

                            refsManager.AddBeginModalRef(refsManager.GenerateModalAdminGroupIdFromGroupNameRef(child.Id), child.Value, ShowModalType.XL);
                            if (GetSection(child.Id, out var modalSection))
                            {
                                GenerateAdvancedCustomSection(modalSection);
                            }
                            refsManager.AddEndModalRef();
                        }
                        break;
                }
            }
        }

        public void AddCustomCategoriesCharts(bool viewTitles, CustomRiskRuleCategory category)
        {
            var values = new Dictionary<int, int>();
            var id = category.Id;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            var data = new SortedDictionary<int, int>();
            int highest = 0;
            int max = 0;
            int division = 0;
            var columns = new List<string>();
            var colors = new List<string>();
            string uniqueColor = "#Fa9C1A";
            values = new Dictionary<int, int>();
            string axisX = "", axisY = "";
            columns = new List<string>() { "Critical", "High", "Medium", "Low" };
            switch (category.Id)
            {
                case "compliance_category_id":
                    division = 3;

                    values.Add(1, ComplinceScores.High);
                    values.Add(2, ComplinceScores.Medium);
                    values.Add(3, ComplinceScores.Low);
                    axisX = "Severity";
                    axisY = "Configurations";
                    break;
                case "zircolite_category_id":
                    division = 4;
                    values.Add(0, ThreatHuntingScores.Critical);
                    values.Add(1, ThreatHuntingScores.High);
                    values.Add(2, ThreatHuntingScores.Medium);
                    values.Add(3, ThreatHuntingScores.Low);
                    axisX = "Severity";
                    axisY = "Rules";
                    break;
                case "yara_category_id":
                    division = 1;
                    columns = new List<string>() { "count" };
                    values.Add(0, YaraScores.Count);
                    axisX = "Total";
                    axisY = "Rules";
                    break;
                case "wesng_category_id":
                    division = 4;
                    values.Add(0, WesngScores.Critical);
                    values.Add(1, WesngScores.High);
                    values.Add(2, WesngScores.Medium);
                    values.Add(3, WesngScores.Low);
                    axisX = "Severity";
                    axisY = "CVE’s";
                    break;
                case "snaffler_category_id":
                    division = 4;
                    values.Add(0, SnafflerScores.Critical);
                    values.Add(1, SnafflerScores.High);
                    values.Add(2, SnafflerScores.Medium);
                    values.Add(3, SnafflerScores.Low);
                    axisX = "Severity";
                    axisY = "Findings";
                    break;
                default:
                    break;
            }
            const double horizontalStep = 50;
            foreach (var entry in values)
            {
                data.Add(entry.Key, entry.Value);
                if (highest < entry.Key)
                    highest = entry.Key;
                if (max < entry.Value)
                    max = entry.Value;
            }
            // add missing data
            if (max > 10000)
                max = 10000;
            else if (max >= 5000)
                max = 10000;
            else if (max >= 1000)
                max = 5000;
            else if (max >= 500)
                max = 1000;
            else if (max >= 100)
                max = 500;
            else if (max >= 50)
                max = 100;
            else if (max >= 10)
                max = 50;
            else
                max = 10;

            for (int i = 0; i < division; i++)
            {
                if (!data.ContainsKey(i))
                    data[i] = 0;
            }
            //refsManager.AddRef(@"
            //<div class=""col-xs-12 col-md-6 col-sm-6"">
            //	<div class=""row"">
            //		<div class=""col-md-4 col-xs-8 col-sm-9"">");

            refsManager.AddRef(@"<div id='pdwdistchart' class=""catgoryChart""");
            refsManager.AddRef(id);

            refsManager.AddRef(@"<p class= ""categoryName"">");
            if (viewTitles)
            {
                refsManager.AddRef(category.Name);
                refsManager.AddRef(@"<p class=""categoryExplanation"">");
                refsManager.AddEncodedRef(category.Explanation);
                refsManager.AddRef(@"</p>");
            }

            refsManager.AddRef(@"</p>");
            if (viewTitles)
                refsManager.AddRef(@"<svg width= ""300%""; viewBox='0 0 1000 400'>");
            else
                refsManager.AddRef(@"<svg width= ""100%""; viewBox='0 0 1000 400'>");

            refsManager.AddRef(@"<g transform=""translate(40,20)"">");
            // horizontal scale
            refsManager.AddRef(@"<g transform=""translate(0,290)"" fill=""none"" font-size=""19"" font-family=""sans-serif"" text-anchor=""middle"">");
            refsManager.AddRef(@"<path  class=""domain"" stroke=""#000"" d=""M 0, 0 h250"" pathLength=""90""></path>");


            for (int i = 0; i < columns.Count; i++)
            {
                double v = 13.06 + (i) * horizontalStep;
                refsManager.AddRef(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                    columns[i] + @"</text></g>");
            }

            refsManager.AddRef(@"</g>");
            // vertical scale
            refsManager.AddRef(@"<g fill=""none"" font-size=""13"" font-family=""sans-serif"" text-anchor=""end"">");
            refsManager.AddRef(@"<path class=""domain"" pathLength=""40"" stroke=""#000"" d=""M-6,290.5H0.5V0.5H-6""></path>");
            for (int i = 0; i < 6; i++)
            {
                double v = 290 - i * 55;
                refsManager.AddRef(@"<g class=""tick"" opacity=""1"" transform=""translate(0," + v.ToString(nfi) + @")""><line stroke=""#000"" x2=""-6""></line><text fill=""#000"" x=""-9"" dy=""0.32em"">" +
                    (max / 5 * i) + @"</text></g>");
            }
            refsManager.AddRef(@"</g>");
            // bars
            for (int i = 0; i < division; i++)
            {
                double v = 3.28 + horizontalStep * (i);
                int value = 0;
                if (data.ContainsKey(i))
                    value = data[i];
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = columns[i] + " " + value.ToString();
                string fillColor = "#Fa9C1A";
                switch (columns[i])
                {
                    case "Critical": fillColor = "#e32c1e"; break;
                    case "High": fillColor = "#Fa9C1A"; break;
                    case "Medium": fillColor = "#f2fa1a"; break;
                    case "Low": fillColor = "#7cfa1a"; break;
                }
                refsManager.AddRef($@"<rect class=""bar"" fill={fillColor} x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                refsManager.AddEncodedRef(tooltip);
                refsManager.AddRef(@"""></rect>");
            }
            {
                double v = 3.28 + horizontalStep * (division);
                int value = 0;
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = string.Empty;
                if (string.IsNullOrEmpty(tooltip))
                    tooltip = value.ToString();
                refsManager.AddRef(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                refsManager.AddEncodedRef(tooltip);
                refsManager.AddRef(@"""></rect>");
            }
            refsManager.AddRef($@"<text x=""-12"" y=""-6"" id=""textId"">{axisY}</text><text x=""255"" y=""295"" id=""textId"">{axisX}</text></g>");
            refsManager.AddRef(@"</g></svg></div>");
        }

        public void GeneratePAWNEDPasswordsScection()
        {
            foreach (var section in InformationSections)
            {
                if(section.Id == "PAWNED_PASSWORDS_section_id")
                {
                    GenerateAdvancedCustomSection(section);                    
                }
            }
        }
        #endregion

        #region Charts
        public bool GetChart(string id, out CustomChart chart)
        {
            try
            {
                if (dictCharts.ContainsKey(id))
                {
                    chart = dictCharts[id];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetChart' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
            chart = null;
            return false;
        }
        #endregion


        #region Categories
        public bool GetCategory(string id, out CustomRiskRuleCategory category)
        {
            try
            {
                if (dictCategories.ContainsKey(id))
                {
                    category = dictCategories[id];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetCategory' method on 'CustomHealthCheckData':");
                Console.WriteLine(e);
            }
            category = null;
            return false;
        }

        public void GenerateCustomCategoriesSections(int globalScore)
        {
            foreach (var category in Categories)
            {
                refsManager.GenerateSectionRef(category.Id, category.Name, () =>
                {
                    refsManager.GenerateSubIndicatorRef(category.Name, globalScore, category.Score, category.Explanation);
                    refsManager.GenerateAdvancedIndicatorPanelRef("Detail" + category.Id, category.Name + "rule details", category.Id);
                });
            }
        }

        public void GenerateCustomCategoriesSubIndicators(int globalScore)
        {
            foreach (var category in Categories)
            {
                refsManager.GenerateSubIndicator5ArgsRef(category.Name, globalScore, category.Score, CountCategoryHealthRules(category.Id), category.Explanation);
            }
        }
        #endregion


        #endregion
    }
}
