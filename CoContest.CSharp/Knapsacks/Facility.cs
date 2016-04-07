using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Knapsacks
{
    public class Facility
    {
        public Facility()
        {

        }

        public Facility(int index, double capacity)
        {
            Index = index;
            Capacity = capacity;
        }

        public int Index { get; set; }
        public double Capacity { get; set; }
    }
}
