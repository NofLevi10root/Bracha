using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomHealthCheckRiskRule
    {
        #region Properties
        [XmlIgnore]
        public int Points { get; set; }
        [XmlElement(ElementName = "Points")]
        public string PointsString
        {
            get
            {
                return this.Points.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Points = 0;
                }
                else
                {
                    this.Points = int.Parse(value);
                }
            }
        }
        public string Category { get; set; }
        public string Model { get; set; }
        public string RiskId { get; set; }
        public string Rationale { get; set; }
        [XmlIgnore]
        public List<string> Details { get; set; }
        [XmlElement("Details")]
        public CustomRuleDetails RuleDetails { get; set; }
        #endregion

        #region Methods
        public static Healthcheck.HealthcheckRiskRule ParseToHealthcheckRiskRule(CustomHealthCheckRiskRule rule)
        {
            Healthcheck.HealthcheckRiskRule output = new Healthcheck.HealthcheckRiskRule();
            output.RiskId = rule.RiskId;
            output.Points = rule.Points;
            output.Rationale = rule.Rationale;
            output.Details = rule.Details;
            if (Enum.IsDefined(typeof(RiskRuleCategory), rule.Category))
                output.Category = (RiskRuleCategory)Enum.Parse(typeof(RiskRuleCategory), rule.Category);
            if (Enum.IsDefined(typeof(RiskModelCategory), rule.Model))
                output.Model = (RiskModelCategory)Enum.Parse(typeof(RiskModelCategory), rule.Model);
            return output;
        }
        #endregion
    }
}
