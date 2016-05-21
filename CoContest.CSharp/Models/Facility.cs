using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Models
{
    public class Facility
    {
        public Facility()
        {

        }

        public Facility(int index, double capacity, double cost)
        {
            Index = index;
            Capacity = capacity;
            Cost = cost;
        }

        public int Index { get; set; }
        public double Capacity { get; set; }
        public double Cost { get; set; }
    }
}
