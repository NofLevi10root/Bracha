﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PingCastle.Addition
{
    public class CustomInformationSection
    {
        #region Properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string Explanation { get; set; }
        [XmlArray("Children")]
        [XmlArrayItem("Child")]
        public List<CustomInformationSectionChild> Children { get; set; }
        #endregion
    }
}
