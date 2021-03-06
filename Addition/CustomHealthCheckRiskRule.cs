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
                return Points.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Points = 0;
                }
                else
                {
                    Points = int.Parse(value);
                }
            }
        }
        [XmlIgnore]
        public string Category { get; set; }
        [XmlIgnore]
        public string Model { get; set; }
        public string RiskId { get; set; }
        public string Rationale { get; set; }
        [XmlArray("Details")]
        [XmlArrayItem("Detail")]
        public List<CustomRuleDetails> RuleDetails { get; set; } = new List<CustomRuleDetails>();
        #endregion

        #region Methods
        public static Healthcheck.HealthcheckRiskRule ParseToHealthcheckRiskRule(CustomHealthCheckRiskRule rule)
        {
            Healthcheck.HealthcheckRiskRule output = new Healthcheck.HealthcheckRiskRule
            {
                RiskId = rule.RiskId,
                Points = rule.Points,
                Rationale = rule.Rationale,
            };
            if (Enum.IsDefined(typeof(RiskRuleCategory), rule.Category))
                output.Category = (RiskRuleCategory)Enum.Parse(typeof(RiskRuleCategory), rule.Category);
            if (Enum.IsDefined(typeof(RiskModelCategory), rule.Model))
                output.Model = (RiskModelCategory)Enum.Parse(typeof(RiskModelCategory), rule.Model);
            return output;
        }
        #endregion
    }
}
