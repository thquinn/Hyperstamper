// TODO:
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
        static void Main(string[] args)
        {
            GeneticDriver driver = new GeneticDriver(PartsInfo.TINY_STRUCTURAL_FRAME);
            driver.Run();
        }
    }
}
