using CoContest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoContest.KnapsacksDecomposition
{
    public static class KnapsacksDecomposition
    {
        public static IEnumerable<IEnumerable<Customer>> Match(IEnumerable<Customer> options, double goal)
        {
            foreach (var option in options)
            {
                if (Math.Abs(goal - option.Demand) < Double.Epsilon)
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
        public static IEnumerable<IEnumerable<Customer>> OptimizedMatch(IEnumerable<Customer> options, double goal, double[,] distances)
        {
            foreach (var option in options)
            {
                if (Math.Abs(goal - option.Demand) < Double.Epsilon)
                    yield return Enumerable.Repeat(option, 1);
                else if (goal > option.Demand)
                {
                    options = options.Where(o => o != option);
                    foreach (var submatch in Match(options, goal - option.Demand)
                        .Where(s => s != null)
                        .Select(col => col.Union(Enumerable.Repeat(option, 1))))

                        yield return submatch;
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

        private static Random random = new Random();
        public static IEnumerable<int[]> OptimizedDivideAndConquer(IEnumerable<Facility> facilities,
            IEnumerable<Customer> customers, double[,] distances)
        {
            Stopwatch sw = new Stopwatch();
            var knapsacksPerFacility = new IEnumerable<FacilityAssignment>[facilities.Count()];

            facilities.AsParallel().ForAll(facility =>
            {
                var result = OptimizedMatch(customers.OrderBy(c => distances[c.Index, facility.Index]).ToList(), facility.Capacity, distances);
                knapsacksPerFacility[facility.Index] = result
                    .Select(a => new FacilityAssignment() { AssignedCustomers = a, FacilityIndex = facility.Index })
                    .ToArray();
            });

            Console.WriteLine("Computed facilities in {0}", sw.Elapsed);
            var solutions = Fit(knapsacksPerFacility, 0, Enumerable.Empty<FacilityAssignment>());
            var customerCount = customers.Count();
            foreach (var solution in solutions)
            {
                //reconstruct and rank it
                var chromosome = ToChromosome(solution, customerCount);
                yield return chromosome;
            }

        }

        public static IEnumerable<int[]> DivideAndConquer(IEnumerable<Facility> facilities, IEnumerable<Customer> customers)
        {
            var knapsacksPerFacility = new List<FacilityAssignment>[facilities.Count()];

            facilities.AsParallel().ForAll(facility =>
            {
                var result = Match(customers, facility.Capacity);
                knapsacksPerFacility[facility.Index] = result
                    .Select(a => new FacilityAssignment() { AssignedCustomers = a, FacilityIndex = facility.Index })
                    .ToList();
                Console.WriteLine($"Computed knapsack for {facility.Index} with {knapsacksPerFacility[facility.Index].Count} options");
            });

            knapsacksPerFacility = knapsacksPerFacility
                .OrderBy(l => l.Count)
                .ToArray();

            var solutions = Fit(knapsacksPerFacility, 0, Enumerable.Empty<FacilityAssignment>());
            Console.WriteLine($"Computing solutions...");
            var customerCount = customers.Count();
            foreach (var solution in solutions)
            {
                //reconstruct and rank it
                var chromosome = ToChromosome(solution.OrderBy(s => s.FacilityIndex), customerCount);
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
