using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PingCastle.Addition
{
    public class CustomRuleDetails
    {
        #region Properties
        public CustomDetailsType Type { get; set; }
        public string FilePath { get; set; }
        #endregion

        #region Methods
        public List<string> ParseToDetails()
        {
            List<string> output = new List<string>();
            if (!File.Exists(FilePath))
                return null;
            switch (Type)
            {
                case CustomDetailsType.List:
                    output = GetList(File.ReadAllLines(FilePath));
                    break;
                case CustomDetailsType.Table:
                    output = GetTable(File.ReadAllLines(FilePath));
                    break;
            }
            return output;
        }
        private static List<string> GetList(string[] lines)
        {
            List<string> output = new List<string>();
            if (lines.Length == 0 || string.IsNullOrEmpty(lines[0].Trim()))
                return null;


            string header = lines[0].Trim().Replace(" ", "#$%%$#") + ": "; // Header
            for (int i = 1; i < lines.Length; i++) // Lines
            {
                output.Add(header + lines[i]);
            }
            return output;
        }
        private static List<string> GetTable(string[] lines)
        {
            List<string> output = new List<string>();

            List<string> headers = new List<string>();
            if (lines.Length > 0) // Headers
            {
                var lineParts = lines[0].Split(',');
                foreach (var part in lineParts)
                {
                    headers.Add(part.Trim().Replace(" ", "#$%%$#") + ": ");
                }
            }
            for (int i = 1; i < lines.Length; i++) // Rows
            {
                var lineParts = lines[i].Split(',');
                StringBuilder builder = new StringBuilder();
                for (int q = 0; q < lineParts.Length; q++)
                {
                    builder.Append(headers[q] + lineParts[q] + " ");
                }
                output.Add(builder.ToString());
            }
            return output;
        }
        private static string GetChart(List<ChartItem> input, string id = "chart1233", Dictionary<int, string> tooltips = null, int cols = 36) //later turn to private
        {   
            StringBuilder builder = new StringBuilder();
            NumberFormatInfo nfi = new NumberFormatInfo();
            #region Demo Data
            input = new List<ChartItem>();
            input.Add(new ChartItem() { Y = 50, X = 0 });
            input.Add(new ChartItem() { Y = 120, X = 12 });
            input.Add(new ChartItem() { Y = 160, X = 15 });
            tooltips = new Dictionary<int, string>();
            tooltips.Add(0, "welcome");
            #endregion



            nfi.NumberDecimalSeparator = ".";
            var data = new SortedDictionary<int, int>();
            int highest = 0;
            int max = 0;
            double horizontalStep = 25 * 36 / (cols+1);
            foreach (var entry in input)
            {
                data.Add(entry.X, entry.Y);
                highest = Math.Max(highest, entry.X);
                max = Math.Max(max, entry.Y);
            }
            // add missing data
            if (max > 10000) // max = max height
                max = 10000;
            else if (max >= 5000)
                max = 10000;
            else if (max >= 1000)
                max = 5000;
            else if (max >= 500)
                max = 1000;
            else if (max >= 100)
                max = 500;
            else if (max >= 50)
                max = 100;
            else if (max >= 10)
                max = 50;
            else
                max = 10;

            int other = 0;
            for (int i = 0; i < cols; i++) // set empty cols to '0'
            {
                if (!data.ContainsKey(i))
                    data[i] = 0;
            }
            for (int i = cols; i <= highest; i++) // fill 'other' col 
            {
                if (data.ContainsKey(i))
                    other += data[i];
            }
            builder.Append(@"<div id='pdwdistchart");
            builder.Append(id);
            builder.Append(@"'><svg viewBox='0 0 1000 400'>");
            builder.Append(@"<g transform=""translate(40,20)"">");
            // horizontal scale
            builder.Append(@"<g transform=""translate(0,290)"" fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""middle"">");
            builder.Append(@"<path class=""domain"" stroke=""#000"" d=""M0.5,0V0.5H950V0""></path>");
            for (int i = 0; i < cols; i++)
            {
                double v = 13.06 + (i) * horizontalStep;
                builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                    (i * 30) + "-" + ((i + 1) * 30) + @" days</text></g>");
            }
            {
                double v = 13.06 + (cols) * horizontalStep;
                builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
            }
            builder.Append(@"</g>");
            // vertical scale
            builder.Append(@"<g fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""end"">");
            builder.Append(@"<path class=""domain"" stroke=""#000"" d=""M-6,290.5H0.5V0.5H-6""></path>");
            for (int i = 0; i < 6; i++)
            {
                double v = 290 - i * 55;
                builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(0," + v.ToString(nfi) + @")""><line stroke=""#000"" x2=""-6""></line><text fill=""#000"" x=""-9"" dy=""0.32em"">" +
                    (max / 5 * i) + @"</text></g>");
            }
            builder.Append(@"</g>");
            // bars
            for (int i = 0; i < cols; i++)
            {
                double v = 3.28 + horizontalStep * (i);
                int value = 0;
                if (data.ContainsKey(i))
                    value = data[i];
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = value.ToString();
                if (tooltips != null && tooltips.ContainsKey(i))
                    tooltip = tooltips[i];
                builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                builder.Append(Report.ReportHelper.Encode(tooltip));
                builder.Append(@"""></rect>");
            }
            {
                double v = 3.28 + horizontalStep * (cols);
                int value = 0;
                value = other;
                double size = 290 * value / max;
                if (size > 290) size = 290;
                double w = horizontalStep - 3;
                string tooltip = string.Empty;
                if (tooltips != null)
                {
                    foreach (var t in tooltips)
                    {
                        if (t.Key > cols)
                            tooltip += t.Value + "\r\n";
                    }
                }
                if (string.IsNullOrEmpty(tooltip))
                    tooltip = value.ToString();
                builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                builder.Append(Report.ReportHelper.Encode(tooltip));
                builder.Append(@"""></rect>");
            }
            builder.Append(@"</g></svg></div>");
            return builder.ToString();
        }
        #endregion
    }
    class ChartItem
    {
        public int Y { get; set; }
        public int X { get; set; }
    }
    public enum CustomDetailsType
    {
        List,
        Table
    }
}
