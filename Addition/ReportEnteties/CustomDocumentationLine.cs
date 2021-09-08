using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition.ReportEnteties
{
    public class CustomDocumentationLine
    {
        #region Properties
        public string Target { get; set; }
        public string Text { get; set; }
        public string SpanClass { get; set; }
        public string SpanText { get; set; }
        #endregion

        #region Methods
        public static string ParseToDocumentationLine(CustomDocumentationLine line)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(line.Target))
                {
                    builder.Append("<a class=\"hyperlink\" href=\"" + line.Target + "\">");
                }
                if (!string.IsNullOrEmpty(line.Text))
                {
                    builder.Append(line.Text);
                }
                builder.Append("</a>");
                if (!string.IsNullOrEmpty(line.SpanClass))
                {
                    builder.Append("<span class=\"" + line.SpanClass + "\">");
                    if (!string.IsNullOrEmpty(line.SpanText))
                    {
                        builder.Append(line.SpanText);
                    }
                    builder.Append("</span>");
                }
                return builder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem on 'ParseToDocumentationLine' method on 'CustomDocumentationLine':");
                Console.WriteLine(e);
                return "";
            }
        }
        #endregion
    }
}
