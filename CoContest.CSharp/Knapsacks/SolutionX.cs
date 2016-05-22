using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoContest;
using CoContest.Models;

namespace CoContest.KnapsacksDecomposition
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
                        .Select(col => col.Add(option)))
                        yield return submatch.ToList();
                }
            }
        }

        public static IEnumerable<IEnumerable<FacilityAssignment>> FitX(Facility[] facilities, double[,] distances, IEnumerable<FacilityAssignment> assignments, IEnumerable<Customer> options, int i)
        {
            if (i == facilities.Length)
            {
                yield return assignments;

            }
            else if (i < facilities.Length)
            {
                foreach (var assignment in MatchX(options.OrderBy(c => distances[c.Index, facilities[i].Index]), facilities[i].Capacity))
                {
                    var fa = new FacilityAssignment()
                    {
                        AssignedCustomers = assignment.ToList(),
                        FacilityIndex = facilities[i].Index
                    };

                    foreach (var solution in FitX(facilities, distances, assignments.Add(fa), options.Except(assignment), i + 1))
                        yield return solution;
                }
            }
        }


        public static IEnumerable<int[]> Solve(IEnumerable<Facility> facilities, IEnumerable<Customer> customers, double[,] distances)
        {
            Stopwatch sw = new Stopwatch();
            Random rng = new Random();

            var facilitiesArray = facilities
                .OrderByRandom(rng, f => f.Capacity)
                .ToArray();

            var solutions = FitX(facilitiesArray, distances, Enumerable.Empty<FacilityAssignment>(), customers, 0);

            sw.Stop();
            Console.WriteLine("Computed facilities in {0}", sw.Elapsed);

            Console.WriteLine($"Computing solutions...");
            var customerCount = customers.Count();
            foreach (var solution in solutions.Take(1000))
            {
                //reconstruct and rank it
                var chromosome = KnapsacksDecomposition.ToChromosome(solution, customerCount);
                yield return chromosome;
            }
        }

        public static bool ValidateChromosome(IEnumerable<Facility> facilities, IEnumerable<Customer> customers, int[] chromosome, IEnumerable<FacilityAssignment> solution)
        {
            var usedCustomers = solution.SelectMany(s => s.AssignedCustomers).Select(c => c.Index).Distinct();

            var facs = chromosome
                .Select((f, c) => new { Customer = c, Facility = f })
                .GroupBy(i => i.Facility)
                .Select(g =>
                {
                    double facCapacity = facilities.Single(f => f.Index == g.Key).Capacity;
                    double usedCapacity = g.Sum(c => customers.Single(i => i.Index == c.Customer).Demand);
                    return facCapacity > usedCapacity;
                }).ToList();
            return facs.Any(b => b == false);
        }

    }
}
