using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomRiskRule
    {
        #region Properties
        public string Id { get; set; }
        [XmlArray("Models")]
        [XmlArrayItem("Model")]
        public List<string> Models { get; set; } = new List<string>();
        [XmlIgnore]
        public List<string> Categories { get; set; } = new List<string>();
        [XmlIgnore]
        public int Maturity { get; set; }
        [XmlElement(ElementName = "Maturity")]
        public string MaturityString
        {
            get
            {
                return Maturity.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Maturity = 0;
                }
                else
                {
                    Maturity = int.Parse(value);
                }
            }
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TechnicalExplanation { get; set; }
        public string Solution { get; set; }
        [XmlIgnore]
        public string ReportLocation { get; set; }
        [XmlElement("ReportLocation")]
        public CustomReportLocation ReportLocationHelper { get; set; }

        [XmlIgnore]
        public string Documentation { get; set; }

        [XmlArray("Documentation")]
        [XmlArrayItem("Line")]
        public List<CustomDocumentationLine> DocumentationHelper { get; set; }

        public List<string> Details { get; set; }

        [XmlArray("RuleComputations")]
        [XmlArrayItem(ElementName = "Computation")]
        public List<CustomRuleComputation> RuleComputations { get; set; } = new List<CustomRuleComputation>();
        #endregion

        #region Fields
        private readonly Dictionary<string, bool> dictCategories = new Dictionary<string, bool>(); 
        private readonly Dictionary<string, bool> dictModels = new Dictionary<string, bool>(); 
        #endregion

        #region Methods
        public static CustomRiskRule GetFromRuleBase<T>(RuleBase<T> rule)
        {
            if (rule == null)
                return null;
            CustomRiskRule output = new CustomRiskRule()
            {
                Id = rule.RiskId,
                Description = rule.Description,
                Details = rule.Details,
                Documentation = rule.Documentation,
                Maturity = rule.MaturityLevel,
                ReportLocation = rule.ReportLocation,
                Solution = rule.Solution,
                TechnicalExplanation = rule.TechnicalExplanation,
                Title = rule.Title
            };
            output.AddCategory(rule.Category.ToString());
            output.AddModel(rule.Model.ToString());

            foreach(var attr in rule.RuleComputation)
            {
                output.RuleComputations.Add(new CustomRuleComputation()
                {
                    Order = attr.Order,
                    Score = attr.Score,
                    Threshold = attr.Threshold,
                    Type = attr.ComputationType.ToString()
                }) ;
            }
            return output;
        }

        public string GetComputationModelString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < RuleComputations.Count; i++)
            {
                if (i > 0)
                    sb.Append("\r\nthen ");
                var rule = RuleComputations[i];
                switch (rule.Type)
                {
                    case "TriggerOnThreshold":
                        sb.Append(rule.Score);
                        sb.Append(" points if the occurence is greater or equals than ");
                        sb.Append(rule.Threshold);
                        break;
                    case "TriggerOnPresence":
                        if (rule.Score > 0)
                        {
                            sb.Append(rule.Score);
                            sb.Append(" points if present");
                        }
                        else
                        {
                            sb.Append("Informative rule (0 point)");
                        }
                        break;
                    case "PerDiscover":
                        sb.Append(rule.Score);
                        sb.Append(" points per discovery");
                        break;
                    case "PerDiscoverWithAMinimumOf":
                        sb.Append(rule.Score);
                        sb.Append(" points per discovery with a minimal of ");
                        sb.Append(rule.Threshold);
                        sb.Append(" points");
                        break;
                    case "TriggerIfLessThan":
                        sb.Append(rule.Score);
                        sb.Append(" points if the occurence is strictly lower than ");
                        sb.Append(rule.Threshold);
                        break;
                }
            }
            return sb.ToString();
        }
        public void SetReportLocation()
        {
            if (ReportLocationHelper != null && ReportLocationHelper.Target != null)
            ReportLocation = "The detail can be found in <a class=\"hyperlink\" href=\"#" + ReportLocationHelper.Target + "\">" + ReportLocationHelper.Text + "</a>";
        }
        public void SetDocumentation()
        {
            if(DocumentationHelper != null)
            {
                string[] lines = new string[DocumentationHelper.Count];
                for(int i = 0; i < lines.Length; i++)
                {
                    lines[i] = CustomDocumentationLine.ParseToDocumentationLine(DocumentationHelper[i]);
                }
                Documentation =  string.Join("<br>", lines);
            }
        }


        public void AddCategory(string category)
        {
            if (dictCategories.ContainsKey(category))
                return;
            dictCategories.Add(category, true);
            Categories.Add(category);
        }

        public void AddModel(string model)
        {
            if (dictModels.ContainsKey(model))
                return;
            dictModels.Add(model, true);
            Models.Add(model);
        }
        public void AddModelToDictionary(string model)
        {
            if (!dictModels.ContainsKey(model))
                dictModels.Add(model, true);
        }

        public bool CheckIsInCategory(string category)
        {
            return dictCategories.ContainsKey(category);
        }

        public bool CheckIsInModel(string model)
        {
            return dictModels.ContainsKey(model);
        }
        #endregion
    }
}
