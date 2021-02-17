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
        #endregion
    }
}
