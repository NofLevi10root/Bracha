using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingCastle.Addition.StructuresEnteties
{
    public class WesngScores: CustomTableScores
    {
        public int Critical { get; set; }

        public int Important { get; set; }

        public int Low { get; set; }

        public int Moderate { get; set; }
    }
}
