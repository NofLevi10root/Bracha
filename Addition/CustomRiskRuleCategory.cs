using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomRiskRuleCategory
    {
        #region Properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string DetailsId { get; set; }
        public string Explanation { get; set; }
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
        #endregion
    }
}
