using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    /// <summary>
    /// Runs a DFS on all possible part combinations of a PartCollection to check if a product is assemblable.
    /// </summary>
    class Assembler
    {
        // Returns true if the collection of parts can be assembled into the initialized product, false if it can't.
        public static float IsAssembleable(PartsInfo partsInfo, Dictionary<PartCollection, float> heuristicMemo, PartCollection partCollection)
        {
            if (partCollection.GetTotalCubeCount() != partsInfo.product.GetCubeCount())
                return -1;
            return IsAssembleableHelper(partsInfo, heuristicMemo, partCollection);
        }
        private static float IsAssembleableHelper(PartsInfo partsInfo, Dictionary<PartCollection, float> heuristicMemo, PartCollection partCollection)
        {
            // Console output.
            Console.Clear();
            if (partsInfo.totalAnalyzeCalls > 0 && partsInfo.totalAllSubproductCalls > 0)
            {
                Console.WriteLine(partsInfo.memoedAnalyzeCalls + "/" + partsInfo.totalAnalyzeCalls + " analyze calls memoized. (" + decimal.Round((decimal)partsInfo.memoedAnalyzeCalls / partsInfo.totalAnalyzeCalls * 100, 2) + "%)");
                Console.WriteLine(partsInfo.memoedAllSubproductCalls + "/" + partsInfo.totalAllSubproductCalls + " all-subproduct calls memoized. (" + decimal.Round((decimal)partsInfo.memoedAllSubproductCalls / partsInfo.totalAllSubproductCalls * 100, 2) + "%)");
            }

            // Memoization and base case.
            if (heuristicMemo.ContainsKey(partCollection))
                return heuristicMemo[partCollection];
            if (partCollection.Quantity() == 1)
            {
                heuristicMemo.Add(partCollection, float.MaxValue);
                return float.MaxValue;
            }

            // Recursion.
            partsInfo.Analyze(partCollection);
            Tuple<Part, Part>[] pairs = partCollection.GetPairs();
            float maxHeuristic = 0;
            foreach (Tuple<Part, Part> pair in pairs)
            {
                HashSet<Part> subproductCombinations = partsInfo.AllSubproductCombinations(pair.Item1, pair.Item2);
                foreach (Part subproductCombination in subproductCombinations)
                {
                    PartCollection newPartCollection = new PartCollection(partCollection, pair.Item1, pair.Item2, subproductCombination);
                    float heuristic = IsAssembleableHelper(partsInfo, heuristicMemo, newPartCollection);
                    if (heuristic == float.MaxValue)
                    {
                        heuristicMemo.Add(partCollection, float.MaxValue);
                        return float.MaxValue;
                    }
                    if (heuristic > maxHeuristic)
                        maxHeuristic = heuristic;
                }
            }
            // This must be a leaf node.
            if (maxHeuristic == 0)
                maxHeuristic = partCollection.GetHeuristic();
            heuristicMemo.Add(partCollection, maxHeuristic);
            return maxHeuristic;
        }
    }
}
