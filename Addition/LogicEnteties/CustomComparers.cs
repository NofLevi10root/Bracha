using System;
using System.Collections.Generic;
using System.Text;

namespace PingCastle.Addition.LogicEnteties
{
    public class MaturityComparer : Comparer<int>
    {
        public override int Compare(int x, int y)
        {
            return -x.CompareTo(y);
        }
    }
}
