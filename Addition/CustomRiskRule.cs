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
        public string Id { get; set; } = "";
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
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string TechnicalExplanation { get; set; } = "";
        public string Solution { get; set; } = "";
        public string ReportLocation { get; set; } = "";
        public string Documentation { get; set; } = "";
        public string DetailsPath { get; set; } = "";

        [XmlArray("RuleComputations")]
        [XmlArrayItem(ElementName = "Computation")]
        public List<CustomRuleComputation> RuleComputations { get; set; }
        #endregion
    }
}
