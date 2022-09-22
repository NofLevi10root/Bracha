using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingCastle
{
    public static class SaveWORK
    {
        public static void InsertData()
        {
            InsertExecutionData();// WORK
            InsertEvasionData(); // WORK + ",";
            InsertCrirticalData(); //DOESNT WORK [ADD BUT NOT CLICKABLE
            InsertHighData(); // DOESNT WORK [ADD BUT NOT CLICKABLE]
            InsertMediumData(); //WORK
            InsertReconnaissanceData(); //IDK
            InsertResourceDevelopmentData();//IDK
            InsertInitialAccessData(); //IDK
            InsertPersistenceData(); //WORK
            InsertPrivilegeEscalationData(); // WORK
            InsertCredentialAccessData(); //WORK
            InsertDiscoveryData();//WORK
            InsertLateralMovementData(); //IDK
            InsertCollectionData(); //IDK
            InsertCommandAndControlData(); //WORK
            InsertExfiltrationData();
            InsertImpactData();//IDK
            InsertOtherData(); //IDK
            InsertUnknownData(); //IDK
            InsertLowData(); //WORK
        }
        public static void InsertExecutionData() //WORK
        {
            //Insert Execution
            int index = Globals.unionText.IndexOf("var ExecutionData = [") + "var ExecutionData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, Globals.executionData);
                Globals.unionText = newUnionText.Replace(@"dictData[""execution""] = ExfiltrationData;", @"dictData[""execution""] = ExecutionData;");
            }
        }

        public static void InsertEvasionData() //WORK
        {
            string newData = Globals.evasionData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var DefenseEvasionData = [") + "var DefenseEvasionData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertCrirticalData() //WORK
        {
            string newData = Globals.criticalData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var CriticalData = [") + "var CriticalData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertHighData() //WORK
        {
            string newData = Globals.highData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var HighData = [") + "var HighData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertMediumData() //WORK
        {
            string newData = Globals.mediumData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var MediumData = [") + "var MediumData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertLowData() //WORK
        {
            string newData = Globals.lowData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var LowData = [") + "var LowData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertPersistenceData() //WORK
        {
            string newData = Globals.persistenceData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var PersistenceData = [") + "var PersistenceData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertPrivilegeEscalationData() //WORK
        {
            string newData = Globals.privilegeEscalationData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var PrivilegeEscalationData = [") + "var PrivilegeEscalationData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertCredentialAccessData() //WORK
        {
            string newData = Globals.credentialAccessData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var CredentialAccessData = [") + "var CredentialAccessData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertDiscoveryData() //WORK
        {
            string newData = Globals.discoveryData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var DiscoveryData = [") + "var DiscoveryData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertCommandAndControlData() //WORK
        {
            string newData = Globals.commandAndControlData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var CommandAndControlData = [") + "var CommandAndControlData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }
        public static void InsertLateralMovementData() //WORK
        {
            string newData = Globals.lateralMovementData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var LateralMovementData = [") + "var LateralMovementData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertReconnaissanceData() //WORK
        {
            string newData = Globals.reconnaissanceData.Replace("]", "");
            int index = "var ReconnaissanceData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertResourceDevelopmentData() //WORK
        {
            string newData = Globals.resourceDevelopmentData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var ResourceDevelopmentData = [") + "var ResourceDevelopmentData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertCollectionData() //WORK
        {
            string newData = Globals.collectionData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var CollectionData = [") + "var CollectionData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertInitialAccessData() //WORK
        {
            string newData = Globals.initialAccessData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var InitialAccessData = [") + "var InitialAccessData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertExfiltrationData() //WORK
        {
            string newData = Globals.exfiltrationData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var ExfiltrationData = [") + "var ExfiltrationData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertImpactData() //WORK
        {
            string newData = Globals.impactData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var ImpactData = [") + "var ImpactData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertOtherData() //WORK
        {
            string newData = Globals.otherData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var OtherData = [") + "var OtherData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }

        public static void InsertUnknownData() //WORK
        {
            string newData = Globals.unknownData.Replace("]", "");
            int index = Globals.unionText.IndexOf("var UnknownData = [") + "var UnknownData = [".Length + 1;
            if (index > 0)
            {
                string newUnionText = Globals.unionText.Insert(index, newData);
                Globals.unionText = newUnionText;
            }
        }
    }
}
