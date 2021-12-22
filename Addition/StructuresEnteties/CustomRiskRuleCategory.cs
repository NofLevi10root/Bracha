using PingCastle.Addition.ReportEnteties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition.StructureEnteties
{
    public class CustomRiskRuleCategory
    {
        #region Properties
        public string Id { get; set; }

        public string Name { get; set; }

        public string Explanation { get; set; }

        [XmlIgnore]
        public int Score { get; set; } = 0;
        #endregion

        #region Methods
        public static string ParseCategoriesToTableHeaders(List<CustomRiskRuleCategory> categories)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (var category in categories) // transfer to static method in customdata
                {
                    builder.Append(@"<th>" + category.Name + @"</th>");
                }
                return builder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'ParseCategoriesToTableHeaders' method on 'CustomRiskRuleCategory':");
                Console.WriteLine(e);
                return null;
            }
        }

        public static void AddCategoriesToRiskModelDictionary(Dictionary<string, List<CustomRiskModelCategory>> riskModelDict, List<CustomRiskRuleCategory> categories)
        {
            foreach (var category in categories)
            {
                riskModelDict[category.Id] = new List<CustomRiskModelCategory>();
            }
        }
        #endregion
    }
}
