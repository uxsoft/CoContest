using CoContest.KnapsacksDecomposition;
using CoContest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.DrykAssociation
{
    public class FacilitySubsetNAssociation
    {
        public static IEnumerable<int[]> Solve(IEnumerable<Facility> facilities, IEnumerable<Customer> customers, double[,] distances)
        {
            var rng = new Random();
            var characteristicPermutation = customers.OrderByRandom(rng, c => -c.Demand);

            var totalDemand = customers.Sum(c => c.Demand);
            var demandRemaining = totalDemand * (1 + (rng.NextDouble() * 0.2));

            facilities
                .OrderByRandom(rng, f => -f.Cost + Enumerable.Range(0, customers.Count()).Sum(c => distances[c, f.Index]))
                .TakeWhile(f => (demandRemaining -= f.Capacity) > 0);


            List<Customer>[] associations = new List<Customer>[facilities.Count()];
            double[] filled = new double[facilities.Count()];
            foreach (var facility in facilities)
                associations[facility.Index] = new List<Customer>();

            foreach (Customer c in characteristicPermutation)
            {
                Facility bestFacility = facilities
                    .OrderBy(f => distances[c.Index, f.Index] + c.Demand * f.Cost / f.Capacity)
                    .First(f => f.Capacity > c.Demand + filled[f.Index]);
                associations[bestFacility.Index].Add(c);
                filled[bestFacility.Index] += c.Demand;
            }

            //reflow optimization


            yield return associations.Select((cs, f) => new FacilityAssignment() { AssignedCustomers = cs, FacilityIndex = f }).ToChromosome(customers.Count());
        }
    }
}
