using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    /// <summary>
    /// Represents an individual in the population.
    /// </summary>
    class Gene
    {
        public bool[] data;
        // The total number of true's allowed in the data.
        public int cubeCount;
        
        public Gene(bool[] data, int cubeCount)
        {
            this.data = data;
            this.cubeCount = cubeCount;
        }

        public static Gene GetRandomGene(int cubeCount) {
            // TODO: make it actually random.
            string[] tinyPattern = new string[] { "XXXOXXOXXOO",
                                                  "XOXOXOXOXOX",
                                                  "XXXOXXOXXOO" };
            bool[] data = new bool[tinyPattern.Length * tinyPattern[0].Length];
            for (int i = 0; i < data.Length; i++ )
                data[i] = (tinyPattern[i / tinyPattern[0].Length][i % tinyPattern[0].Length] == 'X');
            return new Gene(data, cubeCount);
        }
    }
}
