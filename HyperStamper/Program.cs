// TODO:
// - There's a bug: set the number of twos in the structural frame example to 1
// - Change order of pairs: try to add on to largest piece first?
// - Add LRU/priority behavior to memo dictionaries
// - Fix that Part hash!
// - Add more intelligence to IsSubproduct: a lot of time is being spent on PartCollection with multiple parts that must occupy the same specific area of the product.

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
            
            Initialize(PartsInfo.STRUCTURAL_FRAME);

            // Make a test PartCollection that can be assembled into the product.
            Part square = new Part(5, 5, 5, "XXXXXOOOOOOOOOOOOOOOOOOOOXOOOXOOOOOOOOOOOOOOOOOOOOXOOOXOOOOOOOOOOOOOOOOOOOOXOOOXOOOOOOOOOOOOOOOOOOOOXXXXXOOOOOOOOOOOOOOOOOOOO");
            Part c = new Part(5, 5, 5, "XXXXOOOOOOOOOOOOOOOOOOOOOXOOOOOOOOOOOOOOOOOOOOOOOOXOOOOOOOOOOOOOOOOOOOOOOOOXOOOOOOOOOOOOOOOOOOOOOOOOXXXXOOOOOOOOOOOOOOOOOOOOO");
            Part two = new Part(5, 5, 5, "XXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part singleton = new Part(5, 5, 5, "XOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            square = partsInfo.Canonicalize(square);
            c = partsInfo.Canonicalize(c);
            two = partsInfo.Canonicalize(two);
            singleton = partsInfo.Canonicalize(singleton);
            PartCollection partCollection = new PartCollection();
            partCollection.Add(square);
            partCollection.Add(c, 2);
            partCollection.Add(two, 2);
            partCollection.Add(singleton, 2);
            

            /*
            Initialize(PartsInfo.DOCKING_CLAMP);

            // Make a test PartCollection that can be assembled into the product.
            Part clampBase = new Part(5, 5, 5, "XXXXXOOOOOOOOOOOOOOOOOOOOOOXOOOOOOOOOOOOOOOOOOOOOOXXXXXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part baseSlat = new Part(5, 5, 5, "XXXXXOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            Part singleton = new Part(5, 5, 5, "XOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
            clampBase = partsInfo.Canonicalize(clampBase);
            baseSlat = partsInfo.Canonicalize(baseSlat);
            singleton = partsInfo.Canonicalize(singleton);
            PartCollection partCollection = new PartCollection();
            partCollection.Add(clampBase);
            partCollection.Add(baseSlat, 4);
            partCollection.Add(singleton, 12);
            */

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
            PartCollection minPartCollection = null;

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
                        if (minPartCollection == null || newPartCollection.Quantity() <= minPartCollection.Quantity())
                            minPartCollection = newPartCollection;
                        // Otherwise, we still have multiple subproducts. Keep searching.
                        if (!visited.Contains(newPartCollection))
                            toVisit.Push(newPartCollection);
                    }
                }

                // Update console.
                Console.Clear();
                Console.WriteLine("Searched " + visited.Count + " part collections.");
                Console.WriteLine(toVisit.Count + " part collections in queue.");
                Console.WriteLine("Smallest collection seen so far had " + minPartCollection.Quantity() + " parts:\r\n" + minPartCollection.ToString());
                Console.WriteLine(partsInfo.memoedAnalyzeCalls + "/" + partsInfo.totalAnalyzeCalls + " analyze calls memoized. (" + decimal.Round((decimal)partsInfo.memoedAnalyzeCalls / partsInfo.totalAnalyzeCalls * 100, 2) + "%)");
                Console.WriteLine(partsInfo.memoedAllSubproductCalls + "/" + partsInfo.totalAllSubproductCalls + " all-subproduct calls memoized. (" + decimal.Round((decimal)partsInfo.memoedAllSubproductCalls / partsInfo.totalAllSubproductCalls * 100, 2) + "%)");
            }

            return false;
        }
    }
}
