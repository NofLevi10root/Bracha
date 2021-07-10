using PingCastle.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomRuleDetails
    {
        #region Properties
        public CustomDetailsType Type { get; set; }

        public string FilePath { get; set; }

        public string Id { get; set; }
        #endregion
    }
    public enum CustomDetailsType
    {
        Table,
        SharedTable,
        Chart,
        SharedChart
    }  
}
