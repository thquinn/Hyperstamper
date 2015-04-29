using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    class Program
    {
        static PartsInfo partsInfo;
        static HashSet<PartCollection> visited;

        static void Main(string[] args)
        {
            Initialize(PartsInfo.DOCKING_CLAMP);

            // Make a test PartCollection that can be assembled into the product.
            Part clamp = new Part(5, 5, 5, "XXXXXOOOOOOOOOOOOOOOOOOOOXOOOXOOOOOOOOOOOOOOOOOOOOXOOOXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part column = new Part(5, 5, 5, "XOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part bottom = new Part(5, 5, 5, "XXXXXXXXXXXXXXXXXXXXXXXXXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part corner = new Part(5, 5, 5, "XOOOOOOOOOOOOOOOOOOOOOOOOXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            clamp = partsInfo.Canonicalize(clamp);
            column = partsInfo.Canonicalize(column);
            bottom = partsInfo.Canonicalize(bottom);
            corner = partsInfo.Canonicalize(corner);
            PartCollection partCollection = new PartCollection();
            partCollection.Add(clamp);
            partCollection.Add(column);
            partCollection.Add(bottom);
            partCollection.Add(corner, 4);

            // Do an iterative search on the collection of parts to see if it can be combined into the product.
            Console.WriteLine(IsAssembleable(partCollection).ToString().ToLower());
            Console.Read();
        }

        // Clear all dictionaries and initialize a run to assemble an entirely new product.
        static void Initialize(string productString)
        {
            partsInfo = new PartsInfo(productString);
            visited = new HashSet<PartCollection>();
        }

        // Returns true if the collection of parts can be assembled into the initialized product, false if it can't.
        static bool IsAssembleable(PartCollection partCollection)
        {
            partsInfo.Analyze(partCollection);
            Stack<PartCollection> toVisit = new Stack<PartCollection>();
            toVisit.Push(partCollection);

            while (toVisit.Count > 0)
            {
                // Search.
                PartCollection current = toVisit.Pop();
                visited.Add(current);
                Tuple<Part, Part>[] pairs = current.GetPairs();
                foreach (Tuple<Part, Part> pair in pairs)
                {
                    HashSet<Part> subproductCombinations = partsInfo.AllSubproductCombinations(pair.Item1, pair.Item2);
                    foreach (Part subproductCombination in subproductCombinations)
                    {
                        PartCollection newPartCollection = new PartCollection(current, pair.Item1, pair.Item2, subproductCombination);
                        // If there's only part in the collection, it's our product.
                        if (newPartCollection.Quantity() == 1)
                            return true;
                        // Otherwise, we still have multiple subproducts. Keep searching.
                        if (!visited.Contains(newPartCollection))
                            toVisit.Push(newPartCollection);
                    }
                }
            }

            return false;
        }
    }
}
