using PingCastle.Addition.StructureEnteties;
using PingCastle.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition.ReportEnteties
{
    public class CustomChart
    {
        #region Properties
        public string Id { get; set; }
        public CustomChartType ChartType { get; set; }
        public int Interval { get; set; } = 30;
        public int Columns { get; set; } = 36;
        public CustomChartUnit Units { get; set; } = CustomChartUnit.Days;
        public string BaseDate { get; set; }
        #endregion

        #region Fields
        private readonly SortedDictionary<int, int> dictNumeric = new SortedDictionary<int, int>();
        private readonly SortedDictionary<string, int> dictNominal = new SortedDictionary<string, int>();
        private int other = 0;
        private int max = 0;
        #endregion

        #region Methods
        public void AddDetail(CustomRuleDetails detail)
        {
            try
            {
                if (string.IsNullOrEmpty(detail.FilePath) || !File.Exists(detail.FilePath))
                    return;
                var lines = File.ReadAllLines(detail.FilePath);
                if (lines.Length < 2)
                    return;
                var firstLineParts = lines[0].Split(',');
                var secondLineParts = lines[1].Split(',');
                if (firstLineParts.Length != secondLineParts.Length)
                    return;
                #region Chart Data
                switch (ChartType)
                {
                    case CustomChartType.Calendar:
                        DateTime startDate;
                        if (string.IsNullOrEmpty(BaseDate) || !DateTime.TryParse(BaseDate, out startDate))
                        {
                            startDate = DateTime.Now;
                        }

                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (!DateTime.TryParse(firstLineParts[i].Trim(), out DateTime date))
                                return;
                            int x = 0;
                            switch (Units)
                            {
                                case CustomChartUnit.Days:
                                    x = (int)(startDate - date).TotalDays;
                                    break;
                                case CustomChartUnit.Weeks:
                                    x = (int)((startDate - date).TotalDays / 7);
                                    break;
                                case CustomChartUnit.Months:
                                    x = ((startDate.Year - date.Year) * 12) + startDate.Month - date.Month;
                                    break;
                                case CustomChartUnit.Years:
                                    x = startDate.Year - date.Year;
                                    break;
                            }
                            if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                return;
                            if (!dictNumeric.ContainsKey(x))
                                dictNumeric[x] = y;
                            else
                                dictNumeric[x] += y;
                        }

                        break;
                    case CustomChartType.Nominal:
                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (i < Columns - 1)
                            {
                                if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                    return;
                                dictNominal[firstLineParts[i].Trim()] = y;
                            }
                            else
                            {
                                if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                    return;
                                other += y;
                            }
                        }

                        break;
                    case CustomChartType.Numeric:
                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (!int.TryParse(firstLineParts[i].Trim(), out int x))
                                return;
                            if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                return;
                            if (!dictNumeric.ContainsKey(x))
                                dictNumeric[x] = y;
                            else
                                dictNumeric[x] += y;
                        }
                        break;
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'AddDetail' method on 'CustomChart':");
                Console.WriteLine(e);
            }
        }

        public string GetChartString()
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                NumberFormatInfo nfi = new NumberFormatInfo();
                var data = new SortedDictionary<int, int>();
                int cols = Columns;
                switch (ChartType)
                {
                    case CustomChartType.Numeric:
                    case CustomChartType.Calendar:
                        foreach (var item in dictNumeric)
                        {
                            int col = item.Key / Interval;
                            if (col >= Columns)
                            {
                                other += item.Value;
                                max = Math.Max(max, other);
                            }
                            else
                            {
                                if (!data.ContainsKey(col))
                                    data[col] = item.Value;
                                else
                                    data[col] += item.Value;
                                max = Math.Max(max, item.Value);
                            }
                        }
                        break;
                    case CustomChartType.Nominal:
                        cols = Math.Min(dictNominal.Count, cols);
                        int counter = 0;
                        foreach (var item in dictNominal)
                        {
                            max = Math.Max(max, item.Value);
                            data[counter++] = item.Value;
                        }
                        break;
                }

                nfi.NumberDecimalSeparator = ".";
                double horizontalStep = 25 * 36 / (cols + 1);

                if (max > 10000)
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

                for (int i = 0; i < cols; i++) // set empty cols to '0'
                {
                    if (!data.ContainsKey(i))
                        data[i] = 0;
                }
                builder.Append(@"<div id='pdwdistchart");
                builder.Append(Id);
                builder.Append(@"'><svg viewBox='0 0 1000 400'>");
                builder.Append(@"<g transform=""translate(40,20)"">");
                // horizontal scale
                builder.Append(@"<g transform=""translate(0,290)"" fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""middle"">");
                builder.Append(@"<path class=""domain"" stroke=""#000"" d=""M0.5,0V0.5H950V0""></path>");
                if (ChartType == CustomChartType.Calendar || ChartType == CustomChartType.Numeric)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        double v = 13.06 + (i) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                            (i * Interval) + "-" + ((i + 1) * Interval) + " " + ReportHelper.GetEnumDescription(Units) + @"</text></g>");
                    }
                    {
                        double v = 13.06 + (cols) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
                    }
                }
                else if (ChartType == CustomChartType.Nominal)
                {
                    int i = 0;
                    foreach (var item in dictNominal)
                    {
                        double v = 13.06 + (i) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                            item.Key + @"</text></g>");
                        i++;
                    }
                    {
                        double v = 13.06 + (cols) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
                    }
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
                    //if (tooltips != null && tooltips.ContainsKey(i))
                    //    tooltip = tooltips[i];
                    builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                    builder.Append(ReportHelper.Encode(tooltip));
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
                    //if (tooltips != null)
                    //{
                    //    foreach (var t in tooltips)
                    //    {
                    //        if (t.Key > cols)
                    //            tooltip += t.Value + "\r\n";
                    //    }
                    //}
                    if (string.IsNullOrEmpty(tooltip))
                        tooltip = value.ToString();
                    builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                    builder.Append(Report.ReportHelper.Encode(tooltip));
                    builder.Append(@"""></rect>");
                }
                builder.Append(@"</g></svg></div>");
                return builder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetChartString' method on 'CustomChart':");
                Console.WriteLine(e);
                return "";
            }

        }

        public static string GetChart(string filePath, CustomChart chartObject)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || chartObject == null)
                    return "";
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                    return null;
                var firstLineParts = lines[0].Split(',');
                var secondLineParts = lines[1].Split(',');
                if (firstLineParts.Length != secondLineParts.Length)
                    return null;
                #region Chart Data
                SortedDictionary<int, int> dictNumeric = new SortedDictionary<int, int>();
                SortedDictionary<string, int> dictNominal = new SortedDictionary<string, int>();
                var data = new SortedDictionary<int, int>();
                //int highest = 0;
                int max = 0;
                int other = 0;
                int cols = chartObject.Columns;
                switch (chartObject.ChartType)
                {
                    case CustomChartType.Calendar:
                        DateTime startDate;
                        if (string.IsNullOrEmpty(chartObject.BaseDate) || !DateTime.TryParse(chartObject.BaseDate, out startDate))
                        {
                            startDate = DateTime.Now;
                        }

                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (!DateTime.TryParse(firstLineParts[i].Trim(), out DateTime date))
                                return null;
                            int x = 0;
                            switch (chartObject.Units)
                            {
                                case CustomChartUnit.Days:
                                    x = (int)(startDate - date).TotalDays;
                                    break;
                                case CustomChartUnit.Weeks:
                                    x = (int)((startDate - date).TotalDays / 7);
                                    break;
                                case CustomChartUnit.Months:
                                    x = ((startDate.Year - date.Year) * 12) + startDate.Month - date.Month;
                                    break;
                                case CustomChartUnit.Years:
                                    x = startDate.Year - date.Year;
                                    break;
                            }
                            if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                return null;
                            if (!dictNumeric.ContainsKey(x))
                                dictNumeric[x] = y;
                            else
                                dictNumeric[x] += y;
                        }
                        foreach (var item in dictNumeric)
                        {
                            int col = item.Key / chartObject.Interval;
                            if (col >= cols)
                            {
                                other += item.Value;
                                max = Math.Max(max, other);
                            }
                            else
                            {
                                if (!data.ContainsKey(col))
                                    data[col] = item.Value;
                                else
                                    data[col] += item.Value;
                                max = Math.Max(max, item.Value);
                            }
                        }
                        break;
                    case CustomChartType.Nominal:
                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (i < cols - 1) // need to check
                            {
                                if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                    return null;
                                dictNominal[firstLineParts[i].Trim()] = y;
                            }
                            else
                            {
                                if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                    return null;
                                other += y;
                            }
                        }
                        int counter = 0;
                        foreach (var item in dictNominal)
                        {
                            max = Math.Max(max, item.Value);
                            data[counter++] = item.Value;
                        }
                        cols = Math.Min(dictNominal.Count, cols);
                        break;
                    case CustomChartType.Numeric:
                        for (int i = 0; i < firstLineParts.Length; i++)
                        {
                            if (!int.TryParse(firstLineParts[i].Trim(), out int x))
                                return null;
                            if (!int.TryParse(secondLineParts[i].Trim(), out int y))
                                return null;
                            if (!dictNumeric.ContainsKey(x))
                                dictNumeric[x] = y;
                            else
                                dictNumeric[x] += y;
                        }
                        foreach (var item in dictNumeric)
                        {
                            int col = item.Key / chartObject.Interval;
                            if (col >= cols)
                            {
                                other += item.Value;
                                max = Math.Max(max, other);
                            }
                            else
                            {
                                if (!data.ContainsKey(col))
                                    data[col] = item.Value;
                                else
                                    data[col] += item.Value;
                                max = Math.Max(max, item.Value);
                            }
                        }
                        break;
                }
                #endregion



                StringBuilder builder = new StringBuilder();
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                double horizontalStep = 25 * 36 / (cols + 1);


                //foreach (var entry in input)
                //{
                //    data.Add(entry.X, entry.Y);
                //    //highest = Math.Max(highest, entry.X);
                //    max = Math.Max(max, entry.Y);
                //}
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

                for (int i = 0; i < cols; i++) // set empty cols to '0'
                {
                    if (!data.ContainsKey(i))
                        data[i] = 0;
                }
                //for (int i = cols; i <= highest; i++) // fill 'other' col 
                //{
                //    if (data.ContainsKey(i))
                //        other += data[i];
                //}
                builder.Append(@"<div id='pdwdistchart");
                builder.Append(chartObject.Id);
                builder.Append(@"'><svg viewBox='0 0 1000 400'>");
                builder.Append(@"<g transform=""translate(40,20)"">");
                // horizontal scale
                builder.Append(@"<g transform=""translate(0,290)"" fill=""none"" font-size=""10"" font-family=""sans-serif"" text-anchor=""middle"">");
                builder.Append(@"<path class=""domain"" stroke=""#000"" d=""M0.5,0V0.5H950V0""></path>");
                if (chartObject.ChartType == CustomChartType.Calendar || chartObject.ChartType == CustomChartType.Numeric)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        double v = 13.06 + (i) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                            (i * chartObject.Interval) + "-" + ((i + 1) * chartObject.Interval) + " " + ReportHelper.GetEnumDescription(chartObject.Units) + @"</text></g>");
                    }
                    {
                        double v = 13.06 + (cols) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
                    }
                }
                else if (chartObject.ChartType == CustomChartType.Nominal)
                {
                    int i = 0;
                    foreach (var item in dictNominal)
                    {
                        double v = 13.06 + (i) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">" +
                            item.Key + @"</text></g>");
                        i++;
                    }
                    {
                        double v = 13.06 + (cols) * horizontalStep;
                        builder.Append(@"<g class=""tick"" opacity=""1"" transform=""translate(" + v.ToString(nfi) + @",30)""><line stroke=""#000"" y2=""0""></line><text fill=""#000"" y=""3"" dy="".15em"" dx=""-.8em"" transform=""rotate(-65)"">Other</text></g>");
                    }
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
                    //if (tooltips != null && tooltips.ContainsKey(i))
                    //    tooltip = tooltips[i];
                    builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                    builder.Append(ReportHelper.Encode(tooltip));
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
                    //if (tooltips != null)
                    //{
                    //    foreach (var t in tooltips)
                    //    {
                    //        if (t.Key > cols)
                    //            tooltip += t.Value + "\r\n";
                    //    }
                    //}
                    if (string.IsNullOrEmpty(tooltip))
                        tooltip = value.ToString();
                    builder.Append(@"<rect class=""bar"" fill=""#Fa9C1A"" x=""" + v.ToString(nfi) + @""" width=""" + w.ToString(nfi) + @""" y=""" + (290 - size).ToString(nfi) + @""" height=""" + (size).ToString(nfi) + @""" data-toggle=""tooltip"" title=""");
                    builder.Append(Report.ReportHelper.Encode(tooltip));
                    builder.Append(@"""></rect>");
                }
                builder.Append(@"</g></svg></div>");
                return builder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'GetChart' method on 'CustomChart':");
                Console.WriteLine(e);
                return "";
            }
            
        }
        #endregion
    }


    public enum CustomChartType
    {
        Calendar,
        Numeric,
        Nominal
    }
    public enum CustomChartUnit
    {
        [Description("")]
        None,
        [Description("days")]
        Days,
        [Description("weeks")]
        Weeks,
        [Description("months")]
        Months,
        [Description("years")]
        Years

    }
}
