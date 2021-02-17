using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomRuleComputation
    {
        #region Properties
        public string Type { get; set; }
        [XmlIgnore]
        public int Score { get; set; }
        [XmlElement(ElementName = "Score")]
        public string ScoreString
        {
            get
            {
                return this.Score.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Score = 0;
                }
                else
                {
                    this.Score = int.Parse(value);
                }
            }
        }
        [XmlIgnore]
        public int Threshold { get; set; }
        [XmlElement(ElementName = "Threshold")]
        public string ThresholdString
        {
            get
            {
                return this.Threshold.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Threshold = 0;
                }
                else
                {
                    this.Threshold = int.Parse(value);
                }
            }
        }
        [XmlIgnore]
        public int Order { get; set; }
        [XmlElement(ElementName = "Order")]
        public string OrderString
        {
            get
            {
                return this.Order.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Order = 0;
                }
                else
                {
                    this.Order = int.Parse(value);
                }
            }
        }
        #endregion
    }
}
