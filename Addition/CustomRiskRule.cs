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
        public string Category { get; set; }
        public string Model { get; set; }
        [XmlIgnore]
        public int Maturity { get; set; }
        [XmlElement(ElementName = "Maturity")]
        public string MaturityString
        {
            get
            {
                return this.Maturity.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Maturity = 0;
                }
                else
                {
                    this.Maturity = int.Parse(value);
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
        public string Documentation { get; set; }
        public List<string> Details { get; set; }

        [XmlArray("RuleComputations")]
        [XmlArrayItem(ElementName = "Computation")]
        public List<CustomRuleComputation> RuleComputations { get; set; } = new List<CustomRuleComputation>();
        #endregion

        #region Methods
        public static CustomRiskRule GetFromRiskRule<T>(RuleBase<T> rule)
        {
            if (rule == null)
                return null;
            CustomRiskRule output = new CustomRiskRule()
            {
                Id = rule.RiskId,
                Category = rule.Category.ToString(),
                Model = rule.Model.ToString(),
                Description = rule.Description,
                Details = rule.Details,
                Documentation = rule.Documentation,
                Maturity = rule.MaturityLevel,
                ReportLocation = rule.ReportLocation,
                Solution = rule.Solution,
                TechnicalExplanation = rule.TechnicalExplanation,
                Title = rule.Title
            };
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
            ReportLocation =  "The detail can be found in <a href=\"#" + ReportLocationHelper.Target + "\">" + ReportLocationHelper.TargetTitle + "</a>";
        }
        #endregion
    }
}
