using PingCastle.Addition.ReportEnteties;
using PingCastle.Addition.StructureEnteties;
using PingCastle.Addition.StructuresEnteties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PingCastle.Addition.ReportEnteties
{
    public class CustomTable
    {
        #region Properties
        public string Id { get; set; }

        public string NestedTablesDirectory { get; set; }

        [XmlArray("NestedColumns")]
        [XmlArrayItem("NestedColumn")]
        public List<NestedColumn> NestedColumns { get; set; }

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

        private readonly Dictionary<NestedColumn, Dictionary<string, bool>> dictNestedColumnFile = new Dictionary<NestedColumn, Dictionary<string, bool>>();
        #endregion


        #region Methods
        public void SetInitData(string baseDataDirectory)
        {
            try
            {
                foreach (var col in Columns)
                    dictCols[col.Header] = col;
                if (!string.IsNullOrEmpty(NestedTablesDirectory))
                {
                    if (NestedTablesDirectory.StartsWith(@".\"))
                        NestedTablesDirectory = baseDataDirectory + "\\" + NestedTablesDirectory.Substring(2);
                    if (Directory.Exists(NestedTablesDirectory))
                    {
                        foreach (var file in Directory.GetFiles(NestedTablesDirectory))
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            dictNestedTables[fileName] = true;
                        }

                        if (NestedColumns != null && NestedColumns.Count > 0)
                        {
                            foreach (var nestedColumn in NestedColumns)
                            {
                                if (!string.IsNullOrEmpty(nestedColumn.NestedColumnPath))
                                {
                                    dictNestedColumnFile[nestedColumn] = new Dictionary<string, bool>();
                                    if (nestedColumn.NestedColumnPath.StartsWith(@".\"))
                                        nestedColumn.NestedColumnPath = baseDataDirectory + "\\" + nestedColumn.NestedColumnPath.Substring(2);
                                    if (Directory.Exists(nestedColumn.NestedColumnPath))
                                    {
                                        foreach (var file in Directory.GetFiles(nestedColumn.NestedColumnPath))
                                        {
                                            string fileName = Path.GetFileNameWithoutExtension(file);
                                            dictNestedColumnFile[nestedColumn][fileName] = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'SetInitData' method on 'CustomTable':");
                Console.WriteLine(e);
            }
        }

        public void SetLinksToSections(Dictionary<string, CustomInformationSection> sections)
        {
            try
            {
                foreach (var keyLink in KeyLinks)
                {
                    if (sections.ContainsKey(keyLink.Target))
                        dictKeyLinks[keyLink.Value] = sections[keyLink.Target];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'SetLinksToSections' method on 'CustomTable':");
                Console.WriteLine(e);
            }
        }
        public CustomTableScores AddDetail(CustomRuleDetails detail, string delimiter)
        {
            CustomTableScores customTableScore = null;
            try
            {
                if (!File.Exists(detail.FilePath))
                    return null;
                var lines = File.ReadAllLines(detail.FilePath);
                if (lines.Length == 0)
                    return null;

                var column = string.Empty;
                var columnIndex = -1;
                switch (detail.Id)
                {
                    case "compliance_table_id":
                        customTableScore = new ComplinceScores();
                        column = "Severity";
                        break;
                    case "zircolite_table_id":
                        customTableScore = new ThreatHuntingScores();
                        column = "rule_level";
                        break;
                    case "yara_table_id":
                        customTableScore = new YaraScores();
                        break;
                    case "wesng_table_id":
                        customTableScore = new WesngScores();
                        break;
                    case "snaffler_table_id":
                        customTableScore = new SnafflerScores();
                        break;
                    default:
                        break;
                }
                var headers = lines[0].Split(new string[] { delimiter }, StringSplitOptions.None);
                if (!string.IsNullOrEmpty(column))
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        if (headers[i].ToLower() == column.ToLower())
                        {
                            columnIndex = i;
                            break;
                        }
                    }
                }

                var colsNum = headers.Length;
                string[][] data = new string[lines.Length][];
                for (int i = 0; i < lines.Length; i++) // build table 
                {
                    data[i] = new string[colsNum];
                    var lineParts = lines[i].Split(new string[] { delimiter }, StringSplitOptions.None);
                    var maxQ = Math.Min(colsNum, lineParts.Length);
                    for (int q = 0; q < maxQ; q++)
                    {
                        data[i][q] = lineParts[q].Trim();
                        if (columnIndex == q)
                        {
                            customTableScore = FillCustomTableScores(customTableScore, data[i][0], data[i][q], delimiter);
                        }
                        if (customTableScore is WesngScores wesngScores)
                        {
                            if (q == 3)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    wesngScores.Critical += value;
                            }
                            if (q == 4)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    wesngScores.Important += value;
                            }
                            if (q == 5)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    wesngScores.Low += value;
                            }
                            if (q == 6)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    wesngScores.Moderate += value;
                            }
                        }
                        if (customTableScore is SnafflerScores snafflerScores)
                        {
                            if (q == 2)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                {
                                    snafflerScores.Black += value;
                                }
                            }
                            if (q == 3)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    snafflerScores.Red += value;
                            }
                            if (q == 4)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    snafflerScores.Yellow += value;
                            }
                            if (q == 5)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    snafflerScores.Green += value;
                            }

                        }
                        if (customTableScore is YaraScores yaraScores)
                        {
                            if (q == 2)
                            {
                                if (Int32.TryParse(data[i][q], out int value))
                                    yaraScores.NumOfResults += value;
                            }
                        }
                    }
                }

                for (int i = 0; i < headers.Length; i++) // add columns that doesnt exist
                {
                    headers[i] = headers[i].Trim();
                    if (!dictCols.ContainsKey(headers[i]))
                    {
                        CustomTableColumn col = new CustomTableColumn() { Header = headers[i] };
                        dictCols[headers[i]] = col;
                        Columns.Add(col);
                    }
                }

                for (int row = 1; row < data.Length; row++) //run on each line
                {
                    if (!Keys.Contains(data[row][0]))
                        Keys.Add(data[row][0]);
                    for (int col = 0; col < data[row].Length; col++)
                    {
                        var custCol = dictCols[data[0][col]];
                        if (!custCol.Values.ContainsKey(data[row][0]))
                        {
                            custCol.Values[data[row][0]] = data[row][col];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddDetail' method on 'CustomTable':");
                Console.WriteLine(e);
            }

            return customTableScore;

        }
        public static List<string> GetTable(string filePath, string delimiter)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                    return null;
                List<string> output = new List<string>();

                List<string> headers = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Replace(": ", "#$%:%$#");
                }

                foreach (var part in lines[0].Split(new string[] { delimiter }, StringSplitOptions.None)) // Headers
                {
                    headers.Add(part.Trim().Replace(" ", "#$%%$#") + ": ");
                }
                for (int i = 1; i < lines.Length; i++) // Rows
                {
                    var lineParts = lines[i].Split(new string[] { delimiter }, StringSplitOptions.None);
                    StringBuilder builder = new StringBuilder();
                    for (int q = 0; q < lineParts.Length && q < headers.Count; q++)
                    {
                        builder.Append(headers[q] + lineParts[q] + " ");
                    }
                    output.Add(builder.ToString());
                }
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetTable' method on 'CustomTable':");
                Console.WriteLine(e);
                return null;
            }
        }

        public bool GetKeyLinkedSection(string target, out CustomInformationSection result)
        {
            try
            {
                if (dictKeyLinks.ContainsKey(target))
                {
                    result = dictKeyLinks[target];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetKeyLinkedSection' method on 'CustomTable':");
                Console.WriteLine(e);
            }
            result = null;
            return false;
        }

        public bool GetNestedTable(string name, string delimiter, out List<string> result)
        {
            try
            {
                if (!dictNestedTables.ContainsKey(name))
                {
                    result = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetNestedTable' method on 'CustomTable':");
                Console.WriteLine(e);
            }
            result = GetTable($"{NestedTablesDirectory}\\{name}.csv", delimiter);
            return true;
        }

        public bool GetNestedColumnPath(string column, string cellValue, out List<KeyValuePair<string, string>> result)
        {
            try
            {
                if (!dictNestedColumnFile.Any(c => c.Key.Column == column) || string.IsNullOrEmpty(cellValue))
                {
                    result = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetNestedColumnPath' method on 'CustomTable':");
                Console.WriteLine(e);
            }

            result = new List<KeyValuePair<string, string>>();
            var nestedColumn = dictNestedColumnFile.First(c => c.Key.Column == column);

            var values = cellValue.Split(new string[] { nestedColumn.Key.Delimiter }, StringSplitOptions.None);

            foreach (var value in values)
            {
                if (nestedColumn.Value.ContainsKey(value))
                {
                    var path = $"{nestedColumn.Key.NestedColumnPath}\\{value}.json";
                    if (File.Exists(path))
                    {
                        result.Add(new KeyValuePair<string, string>(value, path));
                    }
                }
            }
            return true;
        }

        private CustomTableScores FillCustomTableScores(CustomTableScores customTableScore, string key, string value, string delimiter)
        {
            if (customTableScore != null)
            {
                if (GetNestedTable(key, delimiter, out var targetTable))
                {
                    if (customTableScore is ComplinceScores complince)
                    {
                        switch (value.ToLower())
                        {
                            case "high":
                                complince.High += targetTable.Count;
                                break;
                            case "medium":
                                complince.Medium += targetTable.Count;
                                break;
                            case "low":
                                complince.Low += targetTable.Count;
                                break;
                            default:
                                break;
                        }
                        return complince;
                    }
                    if (customTableScore is ThreatHuntingScores threatHunting)
                    {
                        var computerList = new List<string>();
                        foreach (var tableRow in targetTable)
                        {
                            var computrName = Regex.Match(tableRow, @"(?<=Computer:)(.*?)(?=Channel:)").Value;
                            if(!computerList.Contains(computrName))
                            {
                                computerList.Add(computrName);
                            }
                        }
                        switch (value.ToLower())
                        {
                            case "critical":
                                threatHunting.Critical += computerList.Count;
                                break;
                            case "high":
                                threatHunting.High += computerList.Count;
                                break;
                            case "medium":
                                threatHunting.Medium += computerList.Count;
                                break;
                            case "low":
                                threatHunting.Low += computerList.Count;
                                break;
                            default:
                                break;
                        }
                        return threatHunting;
                    }
                }
            }

            return customTableScore;
        }

        #endregion
    }
}
