using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    /// <summary>
    /// A per-product evaluator that searches for a buildable die cut patten.
    /// </summary>
    class GeneticDriver
    {
        // The length of the sheet to be die cut.
        private readonly int PATTERN_LENGTH = 11;
        // Genetic algorithm vairables
        private readonly int POPULATION_COUNT = 1;

        // Product parameters
        private PartsInfo partsInfo;
        private int cubeCount;

        // Caches
        private Dictionary<PartCollection, float> heuristicMemo;

        public GeneticDriver(string productString)
        {
            partsInfo = new PartsInfo(productString);
            cubeCount = partsInfo.product.GetCubeCount();
            heuristicMemo = new Dictionary<PartCollection, float>();
        }

        public void Run()
        {
            // Just a test run for now.
            Gene[] population = new Gene[POPULATION_COUNT];
            population[0] = Gene.GetRandomGene(cubeCount);
            Console.WriteLine("Heuristic: " + GetHeuristic(population[0]));
            Console.ReadLine();
        }

        private float GetHeuristic(Gene gene)
        {
            PartCollection partCollection = DieCutter.PartCollectionFromBoolArray(gene.data, PATTERN_LENGTH, partsInfo);
            if (partCollection == null)
                return -1;
            return Assembler.IsAssembleable(partsInfo, heuristicMemo, partCollection);
        }
    }
}
