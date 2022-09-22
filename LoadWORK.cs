using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingCastle
{
    public static class LoadWORK
    {
        public static void LoadData(string fileText)
        {
            LoadExecutionData(fileText);
            LoadEvasionData(fileText);
            LoadCriticalData(fileText);
            LoadHighData(fileText);
            LoadMediumData(fileText);
            LoadReconnaissanceData(fileText);
            LoadResourceDevelopmentData(fileText);
            LoadInitialAccessData(fileText);
            LoadPersistenceData(fileText);
            LoadPrivilegeEscalationData(fileText);
            LoadCredentialAccessData(fileText);
            LoadDiscoveryData(fileText);
            LoadLateralMovementData(fileText);
            LoadCollectionData(fileText);
            LoadCommandAndControlData(fileText);
            LoadExfiltrationData(fileText);
            LoadImpactData(fileText);
            LoadOtherData(fileText);
            LoadUnknownData(fileText);
            LoadLowData(fileText);
        }
        public static void LoadExecutionData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var ExecutionData = [") + "var ExecutionData = [".Length;
            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var PersistenceData = [") - 1);
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if(subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.executionData += subString;
                    else
                        Globals.executionData += subString + ",";
                }
                
                //File.WriteAllText("test.txt", subString + "\n checkcheckcheck"); //WORKS
            }
        }

        public static void LoadEvasionData(string fileText) //WORK
        {

            int index = fileText.IndexOf("var DefenseEvasionData = [") + "var DefenseEvasionData = [".Length;

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var CredentialAccessData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.evasionData += subString;
                    else
                        Globals.evasionData += subString + ",";
                }
            }
        }

        public static void LoadCriticalData(string fileText) 
        {
            int index = fileText.IndexOf("var CriticalData = [") + "var CriticalData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var dictData = {};"));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if(subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.criticalData += subString;
                    else
                        Globals.criticalData += subString + ",";
                }
            }
        }

        public static void LoadHighData(string fileText) 
        {
            int index = fileText.IndexOf("var HighData = [") + "var HighData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var CriticalData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.highData += subString;
                    else
                        Globals.highData += subString + ",";
                }
            }
        }

        public static void LoadMediumData(string fileText) 
        {
            int index = fileText.IndexOf("var MediumData = [") + "var MediumData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var HighData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.mediumData += subString;
                    else
                        Globals.mediumData += subString + ",";
                }
            }
        }


        public static void LoadLowData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var LowData = [") + "var LowData = [".Length; 
            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var MediumData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.lowData += subString;
                    else
                        Globals.lowData += subString + ",";
                }
            }
        }

        public static void LoadPersistenceData(string fileText) 
        {
            int index = fileText.IndexOf("var PersistenceData = [") + "var PersistenceData = [".Length;

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var PrivilegeEscalationData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.persistenceData += subString;
                    else
                        Globals.persistenceData += subString + ",";
                }
            }
        }

        public static void LoadPrivilegeEscalationData(string fileText)
        {
            int index = fileText.IndexOf("var PrivilegeEscalationData = [") + "var PrivilegeEscalationData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var DefenseEvasionData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.privilegeEscalationData += subString;
                    else
                        Globals.privilegeEscalationData += subString + ",";
                }
            }
        }

        public static void LoadCredentialAccessData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var CredentialAccessData = [") + "var CredentialAccessData = [".Length; //for some reason persistence
            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var DiscoveryData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.credentialAccessData += subString;
                    else
                        Globals.credentialAccessData += subString + ",";
                }
            }
        }

        public static void LoadDiscoveryData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var DiscoveryData = [") + "var DiscoveryData = [".Length; //for some reason persistence

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var LateralMovementData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.discoveryData += subString;
                    else
                        Globals.discoveryData += subString + ",";
                }
            }
        }

        public static void LoadCommandAndControlData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var CommandAndControlData = [") + "var CommandAndControlData = [".Length; //for some reason persistence

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var ExfiltrationData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.commandAndControlData += subString;
                    else
                        Globals.commandAndControlData += subString + ",";
                }
            }
        }

        public static void LoadLateralMovementData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var LateralMovementData = [") + "var LateralMovementData = [".Length; //for some reason persistence

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var CollectionData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.lateralMovmentData += subString;
                    else
                        Globals.lateralMovmentData += subString + ",";
                }
            }
        }

        public static void LoadReconnaissanceData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var ReconnaissanceData = [") + "var ReconnaissanceData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var ResourceDevelopmentData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.reconnaissanceData += subString;
                    else
                        Globals.reconnaissanceData += subString + ",";
                }
            }
        }

        public static void LoadResourceDevelopmentData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var ResourceDevelopmentData = [") + "var ResourceDevelopmentData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var InitialAccessData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.resourceDevelopmentData += subString;
                    else
                        Globals.resourceDevelopmentData += subString + ",";
                }
            }
        }

        public static void LoadCollectionData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var CollectionData = [") + "var CollectionData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var CommandAndControlData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {

                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.collectionData += subString;
                    else
                        Globals.collectionData += subString + ",";
                }
            }
        }

        public static void LoadInitialAccessData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var InitialAccessData = [") + "var InitialAccessData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var ExecutionData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.initialAccessData += subString;
                    else
                        Globals.initialAccessData += subString + ",";
                }
            }
        }

        public static void LoadExfiltrationData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var ExfiltrationData = [") + "var ExfiltrationData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var ImpactData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.exfiltrationData += subString;
                    else
                        Globals.exfiltrationData += subString + ",";
                }
            }
        }

        public static void LoadImpactData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var ImpactData = [") + "var ImpactData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var OtherData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.impactData += subString;
                    else
                        Globals.impactData += subString + ",";
                }
            }
        }

        public static void LoadOtherData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var OtherData = [") + "var OtherData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var UnknownData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.otherData += subString;
                    else
                        Globals.otherData += subString + ",";
                }
            }
        }

        public static void LoadUnknownData(string fileText) //WORK
        {
            int index = fileText.IndexOf("var UnknownData = [") + "var UnknownData = [".Length; 

            if (index > 0)
            {
                string subString = fileText.Substring(index);
                string newSubString = subString.Substring(0, subString.IndexOf("var LowData = ["));
                subString = newSubString.Substring(0, newSubString.LastIndexOf("];"));
                if (subString.Contains("Rule level"))
                {
                    if (subString.LastIndexOf(",") > subString.LastIndexOf("}"))
                        Globals.unknownData += subString;
                    else
                        Globals.unknownData += subString + ",";
                }
            }
        }
    }
}
