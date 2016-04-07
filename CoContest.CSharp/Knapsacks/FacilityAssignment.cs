using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Knapsacks
{
    public class FacilityAssignment
    {
        public int FacilityIndex { get; set; }
        public IEnumerable<Customer> AssignedCustomers { get; set; }
    }
}
