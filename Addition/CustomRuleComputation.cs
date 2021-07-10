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
                return Score.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Score = 0;
                }
                else
                {
                    Score = int.Parse(value);
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
                return Threshold.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Threshold = 0;
                }
                else
                {
                    Threshold = int.Parse(value);
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
                return Order.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Order = 0;
                }
                else
                {
                    Order = int.Parse(value);
                }
            }
        }
        #endregion
    }
}
