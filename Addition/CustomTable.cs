using System;
using System.Collections.Generic;
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
        #endregion
    }
}
