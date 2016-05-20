using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.Knapsacks
{
    public class SolutionX
    {
        public static IEnumerable<IEnumerable<Customer>> MatchX(IEnumerable<Customer> options, double goal)
        {
            foreach (var option in options)
            {
                if (Math.Abs(goal - option.Demand) < Double.Epsilon)
                    yield return Enumerable.Repeat(option, 1);
                else if (goal > option.Demand)
                {
                    options = options.Where(o => o != option);
                    foreach (var submatch in MatchX(options, goal - option.Demand)
                        .Where(s => s != null)
                        .Select(col => col.Union(Enumerable.Repeat(option, 1))))
                        yield return submatch.ToList();
                }
            }
        }


        public static IEnumerable<int[]> Solve(IEnumerable<Facility> facilities, IEnumerable<Customer> customers, double[,] distances)
        {
            Stopwatch sw = new Stopwatch();
            var knapsacksPerFacility = new List<FacilityAssignment>[facilities.Count()];

            facilities.OrderBy(facility => facility.Capacity).AsParallel().ForAll(facility =>
              {
                  var result = MatchX(new HashSet<Customer>(customers), facility.Capacity);
                  knapsacksPerFacility[facility.Index] = result
                      .Select(a => new FacilityAssignment() { AssignedCustomers = a, FacilityIndex = facility.Index })
                      .ToList();
                  Console.WriteLine($"Computed knapsack for {facility.Index} with {knapsacksPerFacility[facility.Index].Count} options");
              });

            knapsacksPerFacility = knapsacksPerFacility
                .OrderBy(l => l.Count)
                .ToArray();

            sw.Stop();
            Console.WriteLine("Computed facilities in {0}", sw.Elapsed);

            var solutions = KnapsacksDecomposition.Fit(knapsacksPerFacility, 0, Enumerable.Empty<FacilityAssignment>());
            Console.WriteLine($"Computing solutions...");
            var customerCount = customers.Count();
            foreach (var solution in solutions)
            {
                //reconstruct and rank it
                var chromosome = KnapsacksDecomposition.ToChromosome(solution, customerCount);
                yield return chromosome;
            }
        }

    }
}
