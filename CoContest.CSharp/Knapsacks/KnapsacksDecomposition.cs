using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Knapsacks
{
    public static class KnapsacksDecomposition
    {
        public static IEnumerable<IEnumerable<Customer>> Match(IEnumerable<Customer> options, double goal)
        {
            foreach (var option in options)
            {
                if (goal == option.Demand)
                    yield return Enumerable.Repeat(option, 1);
                else if (goal > option.Demand)
                {
                    options = options.Where(o => o != option);
                    foreach (var submatch in Match(options, goal - option.Demand)
                        .Where(s => s != null)
                        .Select(col => col.Union(Enumerable.Repeat(option, 1))))
                        yield return submatch.ToList();
                }
            }
        }

        public static IEnumerable<IEnumerable<FacilityAssignment>> Fit(IEnumerable<FacilityAssignment>[] knapsacksForFacility, int i,
    IEnumerable<FacilityAssignment> pickedAssignments)
        {
            if (i < knapsacksForFacility.Length)
                foreach (var assignment in knapsacksForFacility[i])
                {
                    if (!pickedAssignments.SelectMany(a => a.AssignedCustomers)
                            .Intersect(assignment.AssignedCustomers)
                            .Any())
                    {
                        foreach (var subAssignment in Fit(knapsacksForFacility, i + 1, pickedAssignments.Union(Enumerable.Repeat(assignment, 1))))
                            yield return subAssignment;
                    }
                }
            else yield return pickedAssignments;
        }

        public static IEnumerable<int[]> DivideAndConquer(IEnumerable<Facility> facilities, IEnumerable<Customer> customers)
        {
            var knapsacksPerFacility = new List<FacilityAssignment>[facilities.Count()];

            foreach (var facility in facilities)
            {
                var result = Match(customers, facility.Capacity);
                knapsacksPerFacility[facility.Index] = result
                    .Select(a => new FacilityAssignment() { AssignedCustomers = a, FacilityIndex = facility.Index })
                    .ToList();
                Console.WriteLine($"Computed knapsack for {facility.Index} with {knapsacksPerFacility[facility.Index].Count} options");
            }

            knapsacksPerFacility = knapsacksPerFacility
                .OrderBy(l => l.Count)
                .ToArray();

            var solutions = Fit(knapsacksPerFacility, 0, Enumerable.Empty<FacilityAssignment>());
            Console.WriteLine($"Computing solutions...");
            var customerCount = customers.Count();
            foreach (var solution in solutions)
            {
                //reconstruct and rank it
                var chromosome = ToChromosome(solution, customerCount);
                yield return chromosome;
            }
        }

        public static int[] ToChromosome(this IEnumerable<FacilityAssignment> solution, int customerCount)
        {
            int[] chromosome = new int[customerCount];
            foreach (var assignment in solution)
                foreach (var customer in assignment.AssignedCustomers)
                    chromosome[customer.Index] = assignment.FacilityIndex;
            return chromosome;
        }
    }
}
