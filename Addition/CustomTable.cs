using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomTable
    {
        #region Properties
        public string Id { get; set; }
        public string NestedTablesDirectory { get; set; }
        [XmlArray("Columns")]
        [XmlArrayItem("Column")]
        public List<CustomTableColumn> Columns { get; set; } = new List<CustomTableColumn>();

        [XmlArray("KeyLinks")]
        [XmlArrayItem("KeyLink")]
        public List<CustomTableKeyLink> KeyLinks { get; set; } = new List<CustomTableKeyLink>();
        [XmlIgnore]
        public List<string> Keys { get; set; } = new List<string>();
        #endregion

        #region Fields
        private readonly Dictionary<string, CustomTableColumn> dictCols = new Dictionary<string, CustomTableColumn>();
        private readonly Dictionary<string, CustomInformationSection> dictKeyLinks = new Dictionary<string, CustomInformationSection>();
        private readonly Dictionary<string, bool> dictNestedTables = new Dictionary<string, bool>();
        #endregion


        #region Methods
        public void SetInitData(string baseDataDirectory)
        {
            foreach (var col in Columns)
                dictCols[col.Header] = col;
            if(!string.IsNullOrEmpty(NestedTablesDirectory))
            {
                if (NestedTablesDirectory.StartsWith(@".\"))
                    NestedTablesDirectory = baseDataDirectory + "\\" + NestedTablesDirectory.Substring(2);
                if(Directory.Exists(NestedTablesDirectory))
                {
                    foreach(var file in Directory.GetFiles(NestedTablesDirectory))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        dictNestedTables[fileName] = true;
                    }
                }
            }
        }

        public void SetLinksToSections(Dictionary<string, CustomInformationSection> sections)
        {
            foreach(var keyLink in KeyLinks)
            {
                if (sections.ContainsKey(keyLink.Target))
                    dictKeyLinks[keyLink.Value] = sections[keyLink.Target];
            }
        }
        public void AddDetail(CustomRuleDetails detail)
        {
            if (!File.Exists(detail.FilePath))
                return;
            var lines = File.ReadAllLines(detail.FilePath);
            if (lines.Length == 0)
                return;

            var headers = lines[0].Split(',');
            var colsNum = headers.Length;
            string[][] data = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++) // build table 
            {
                data[i] = new string[colsNum];
                var lineParts = lines[i].Split(',');
                var maxQ = Math.Min(colsNum, lineParts.Length);
                for (int q = 0; q < maxQ; q++)
                {
                    data[i][q] = lineParts[q].Trim();
                }
            }

            for(int i = 0; i < headers.Length; i++) // add columns that doesnt exist
            {
                headers[i] = headers[i].Trim();
                if(!dictCols.ContainsKey(headers[i]))
                {
                    CustomTableColumn col = new CustomTableColumn() { Header = headers[i]};
                    dictCols[headers[i]] = col;
                    Columns.Add(col);
                }
            }

            for (int row = 1; row < data.Length; row++) //run on each line
            {
                if (!Keys.Contains(data[row][0]))
                    Keys.Add(data[row][0]);
                for(int col = 0; col < data[row].Length; col++)
                {
                    var custCol = dictCols[data[0][col]];
                    if (!custCol.Values.ContainsKey(data[row][0]))
                    {
                        custCol.Values[data[row][0]] = data[row][col];
                    }
                }
            }
        }
        public static List<string> GetTable(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return null;
            List<string> output = new List<string>();

            List<string> headers = new List<string>();

            foreach (var part in lines[0].Split(',')) // Headers
            {
                headers.Add(part.Trim().Replace(" ", "#$%%$#") + ": ");
            }
            for (int i = 1; i < lines.Length; i++) // Rows
            {
                var lineParts = lines[i].Split(',');
                StringBuilder builder = new StringBuilder();
                for (int q = 0; q < lineParts.Length && q < headers.Count; q++)
                {
                    builder.Append(headers[q] + lineParts[q] + " ");
                }
                output.Add(builder.ToString());
            }
            return output;
        }

        public bool GetKeyLinkedSection(string target, out CustomInformationSection result)
        {
            if(dictKeyLinks.ContainsKey(target))
            {
                result = dictKeyLinks[target];
                return true;
            }
            result = null;
            return false;
        }

        public bool GetNestedTable(string name, out List<string> result)
        {
            if(!dictNestedTables.ContainsKey(name))
            {
                result = null;
                return false;
            }
            result = GetTable($"{NestedTablesDirectory}\\{name}.csv");
            return true;
        }
        #endregion
    }
}
