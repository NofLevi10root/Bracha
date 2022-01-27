using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PingCastle.Addition.StructuresEnteties
{
    public class CustomColumnScore
    {
        #region Properties
        public string Type { get; set; }

        [XmlIgnore]
        public int Score { get; set; }

        [XmlElement(ElementName = "Score")]
        public string ScoreString
        {
            get
            {
                return Score.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Score = 0;
                }
                else
                {
                    Score = int.Parse(value);
                }
            }
        }

        #endregion
    }
}
