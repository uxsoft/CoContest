using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Models
{
    public class Customer
    {
        public Customer()
        {

        }

        public Customer(int index, double demand)
        {
            Index = index;
            Demand = demand;
        }

        public int Index { get; set; }
        public double Demand { get; set; }
    }
}
