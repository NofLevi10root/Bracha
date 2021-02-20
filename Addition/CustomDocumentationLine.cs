﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition
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
        public static string ParseToDocumentaionLine(CustomDocumentationLine line)
        {
            StringBuilder builder = new StringBuilder();
            if(!string.IsNullOrEmpty(line.Target))
            {
                builder.Append("<a href=\"" + line.Target + "\">");
            }
            if(!string.IsNullOrEmpty(line.Text))
            {
                builder.Append(line.Text);
            }
            builder.Append("</a>");
            if (!string.IsNullOrEmpty(line.SpanClass))
            {
                builder.Append("<span class=\"" + line.SpanClass + "\">");
                if(!string.IsNullOrEmpty(line.SpanText))
                {
                    builder.Append(line.SpanText);
                }
                builder.Append("</span>");
            }
            return builder.ToString();
        }
        #endregion
    }
}
