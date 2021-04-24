﻿using PingCastle.Rules;
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
        public List<string> Categories { get; set; } = new List<string>();
        [XmlIgnore]
        public List<string> Models { get; set; } = new List<string>();
        public string RiskId { get; set; }
        public string Rationale { get; set; }
        [XmlArray("Details")]
        [XmlArrayItem("Detail")]
        public List<CustomRuleDetails> RuleDetails { get; set; } = new List<CustomRuleDetails>();
        #endregion

        #region Fields
        private readonly Dictionary<string, bool> dictCategories = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> dictModels = new Dictionary<string, bool>();
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
            foreach(var category in rule.Categories)
            {
                if (Enum.IsDefined(typeof(RiskRuleCategory), category))
                {
                    output.Category = (RiskRuleCategory)Enum.Parse(typeof(RiskRuleCategory), category);
                    break;
                }
            }
            foreach(var model in rule.Models)
            {
                if (Enum.IsDefined(typeof(RiskModelCategory), model))
                {
                    output.Model = (RiskModelCategory)Enum.Parse(typeof(RiskModelCategory), model);
                    break;
                }
            }
            return output;
        }

        public void AddCategory(string category)
        {
            if (dictCategories.ContainsKey(category))
                return;
            dictCategories.Add(category, true);
            Categories.Add(category);
        }

        public void AddModel(string model)
        {
            if (dictModels.ContainsKey(model))
                return;
            dictModels.Add(model, true);
            Models.Add(model);
        }

        public bool CheckIsInCategory(string category)
        {
            return dictCategories.ContainsKey(category);
        }

        public bool CheckIsInModel(string model)
        {
            return dictModels.ContainsKey(model);
        }
        #endregion
    }
}
