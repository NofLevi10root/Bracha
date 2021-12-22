using PingCastle.Addition.ReportEnteties;
using PingCastle.Addition.StructureEnteties;
using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition.LogicEnteties
{
    public interface ICustomData
    {
        List<CustomRiskRuleCategory> Categories { get; set; }
        List<CustomRiskModelCategory> Models { get; set; }
        List<CustomHealthCheckRiskRule> HealthRules { get; set; }
        List<CustomRiskRule> RiskRules { get; set; }
        bool IsEmpty { get; set; }

        bool GetRiskRule(string id, out CustomRiskRule riskRule);
        void SetRefsManager(CustomMethodsReferenceManager referenceManager);
    }
}
