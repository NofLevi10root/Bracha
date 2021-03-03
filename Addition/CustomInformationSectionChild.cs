using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition
{
    public class CustomInformationSectionChild
    {
        #region Properties
        public CustomSectionChildType Type { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
        #endregion

        
    }
    #region Enums
    public enum CustomSectionChildType
    {
        Chart,
        Table,
        Paragraph,
        SubSectionTitle
    }
    #endregion
}
