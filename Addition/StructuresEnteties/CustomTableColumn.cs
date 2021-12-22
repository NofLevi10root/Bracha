using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition.StructureEnteties
{
    public class CustomTableColumn
    {
        #region Properties
        public string Header { get; set; }
        public string Tooltip { get; set; }
        [XmlIgnore]
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
        #endregion
    }
}
