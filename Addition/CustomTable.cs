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
        [XmlArray("Columns")]
        [XmlArrayItem("Column")]
        public List<CustomTableColumn> Columns { get; set; } = new List<CustomTableColumn>();
        [XmlIgnore]
        public List<string> Keys { get; set; } = new List<string>();
        #endregion

        #region Fields
        private readonly Dictionary<string, CustomTableColumn> dictCols = new Dictionary<string, CustomTableColumn>();
        #endregion


        #region Methods
        public void SetInitData()
        {
            foreach (var col in Columns)
                dictCols[col.Header] = col;
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
                for (int q = 0; q < lineParts.Length; q++)
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
        #endregion
    }
}
