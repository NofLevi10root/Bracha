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
        public int Score { get; set; } = 0;
        #endregion
    }
}
