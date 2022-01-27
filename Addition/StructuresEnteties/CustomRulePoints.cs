using PingCastle.Addition.StructuresEnteties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition.StructureEnteties
{
    public class CustomRulePoints
    {
        #region Properties
        public string Column { get; set; }

        [XmlArray("ColumnsScores")]
        [XmlArrayItem("ScoreType")]
        public List<CustomColumnScore> ColumnScore { get; set; }

        [XmlIgnore]
        public int? ColumnIndex { get; set; }
        #endregion
    }
}
