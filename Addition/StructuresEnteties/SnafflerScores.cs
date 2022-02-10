using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingCastle.Addition.StructuresEnteties
{
    public class SnafflerScores : CustomTableScores
    {
        public int Critical { get; set; }

        public int High { get; set; }

        public int Medium { get; set; }

        public int Low { get; set; }
    }
}
