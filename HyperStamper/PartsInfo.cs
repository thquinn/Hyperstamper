using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HyperStamper
{
    public class PartsInfo
    {
        public static readonly string STRUCTURAL_FRAME = "XXXXX\r\nXOOOX\r\nXOOOX\r\nXOOOX\r\nXXXXX\r\n\r\nXOOOX\r\nOOOOO\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\n\r\nXOOOX\r\nOOOOO\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\n\r\nXOOOX\r\nOOOOO\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\n\r\nXXXXX\r\nXOOOX\r\nXOOOX\r\nXOOOX\r\nXXXXX";
        public static readonly string DOCKING_CLAMP = "XXXXX\r\nXXXXX\r\nXXXXX\r\nXXXXX\r\nXXXXX\r\n\r\nOOOOO\r\nOOOOO\r\nOOXOO\r\nOOOOO\r\nOOOOO\r\n\r\nOOOOO\r\nOOOOO\r\nXXXXX\r\nOOOOO\r\nOOOOO\r\n\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\nOOOOO\r\nOOOOO\r\n\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\nOOOOO\r\nOOOOO";

        // Maps from any part to a list of its rotations in canonical order -- rotations whose bit arrays evaluate to
        // lower numbers are more canonical than those whose bit arrays evaluate to larger numbers.
        public Dictionary<Part, List<Part>> partRotations;
        // Returns true if the part (canonical rotation) is a substructure of the product at any rotation, false otherwise.
        public Dictionary<Part, bool> isSubproductMemo;
        // The product we're trying to create by combining parts.
        public Part product;

        public PartsInfo(String productString)
        {
            // Initialize dictionaries.
            partRotations = new Dictionary<Part, List<Part>>();
            isSubproductMemo = new Dictionary<Part, bool>();

            // TODO: Check that the product completely fills the area given.

            // Read the product string.
            List<bool> bools = new List<bool>();
            byte length = 0, width = 0, height = 0;
            string[] zSlices = productString.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            height = (byte)(zSlices.Length);
            foreach (string zSlice in zSlices)
            {
                string[] ySlices = zSlice.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                width = (byte)(ySlices.Length);
                foreach (string ySlice in ySlices)
                {
                    length = (byte)(ySlice.Length);
                    foreach (char c in ySlice)
                        if (c == 'X')
                            bools.Add(true);
                        else if (c == 'O')
                            bools.Add(false);
                }
            }
            BitArray bitArray = new BitArray(bools.Count);
            for (int i = 0; i < bitArray.Length; i++)
                bitArray.Set(i, bools[i]);
            product = new Part(length, width, height, bitArray);
            // Set the product to the canonical rotation of itself.
            Analyze(product);
            product = partRotations[product][0];
        }

        // Analyzes all relevant information about the given part and stores it to prevent repeat analysis.
        public void Analyze(Part part)
        {
            // TODO: Check that the part is shifted as close to the axes as possible, throw an error otherwise.

            // If we've already analyzed this part, we don't need to do it again.
            if (partRotations.ContainsKey(part))
                return;

            // Find all rotations of part and insert into partRotations with each rotation as key.
            List<Part> rotations = part.GetRotations();
            foreach (Part rotation in rotations)
                partRotations.Add(rotation, rotations);


        }

        // Returns the canonical rotations of all parts that can be produced by welding the two inputs together that
        // are substructures of the product.
        public List<Part> AllSubproductCombinations(Part first, Part second)
        {
            // TODO: Implement.
            return null;
        }

        // Check if each rotation is a part of the product (or is the entire product). If any are, they all are. Store
        // the result in isSubproductMemo under the canonical rotation.
        public bool IsSubproduct(Part part)
        {
            if (isSubproductMemo.ContainsKey(part))
                return isSubproductMemo[part];

            // TODO: Implement.
            return false;
        }
    }
}
