using PingCastle.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition
{
    public class CustomRiskModelCategory
    {
        #region Properties
        public string Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        #endregion

        #region Constructors
        public CustomRiskModelCategory()
        {

        }
        public CustomRiskModelCategory(RiskModelCategory category)
        {
            Id = category.ToString();
            Description = Report.ReportHelper.GetEnumDescription(category);
        }
        #endregion
    }
}
