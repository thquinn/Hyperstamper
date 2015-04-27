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
        public static readonly string DOCKING_CLAMP = "XXXXX\r\nXXXXX\r\nXXXXX\r\nXXXXX\r\nXXXXX\r\n\r\nXOOOX\r\nOOOOO\r\nOOXOO\r\nOOOOO\r\nXOOOX\r\n\r\nXOOOX\r\nOOOOO\r\nXXXXX\r\nOOOOO\r\nXOOOX\r\n\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\nOOOOO\r\nOOOOO\r\n\r\nOOOOO\r\nOOOOO\r\nXOOOX\r\nOOOOO\r\nOOOOO";

        // Maps from any part to a list of its rotations in canonical order -- rotations whose bit arrays evaluate to
        // lower numbers are more canonical than those whose bit arrays evaluate to larger numbers.
        public Dictionary<Part, List<Part>> partRotations;
        // True if the key (a canonical Part) is a substructure of the product at any rotation.
        private Dictionary<Part, bool> isSubproductMemo;
        // Maps from the XORed hashes of two canonical Parts to a list of all subproducts that can be made by welding
        // them together.
        private Dictionary<int, List<Part>> allSubproductCombinationsMemo;
        // The product we're trying to create by combining parts.
        public Part product;

        public PartsInfo(String productString)
        {
            // Initialize dictionaries.
            partRotations = new Dictionary<Part, List<Part>>();
            isSubproductMemo = new Dictionary<Part, bool>();
            allSubproductCombinationsMemo = new Dictionary<int, List<Part>>();

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
            product = new Part(length, width, height, new BitArray(bools.ToArray()));
            // Set the product to the canonical rotation of itself.
            Analyze(product);
            product = Canonicalize(product);
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

        // Returns the canonical rotation of the given part.
        public Part Canonicalize(Part part)
        {
            return partRotations[part][0];
        }

        // Returns the canonical rotations of all parts that can be produced by welding the two inputs together that
        // are substructures of the product.
        public List<Part> AllSubproductCombinations(Part first, Part second)
        {
            int xoredHash = first.GetHashCode() ^ second.GetHashCode();
            if (allSubproductCombinationsMemo.ContainsKey(xoredHash))
                return allSubproductCombinationsMemo[xoredHash];

            // TODO: Implement.
            return null;
        }

        // Check if each rotation is a part of the product (or is the entire product). If any are, they all are. Store
        // the result in isSubproductMemo under the canonical rotation.
        public bool IsSubproduct(Part part)
        {
            if (isSubproductMemo.ContainsKey(part))
                return isSubproductMemo[part];

            // Find max X, Y, Z of part within its bounding cube.
            int maxX = 0, maxY = 0, maxZ = 0;
            for (int x = 0; x < part.length; x++)
                for (int y = 0; y < part.width; y++)
                    for (int z = 0; z < part.height; z++)
                        if (part.Get(x, y, z))
                        {
                            maxX = Math.Max(maxX, x);
                            maxY = Math.Max(maxY, y);
                            maxZ = Math.Max(maxZ, z);
                        }

            // Check all positions of each rotation against the product.
            List<Part> rotations = partRotations[part];
            bool subproductFound = false;
            foreach (Part rotation in rotations)
                if (IsSubproductForSingleRotation(rotation, maxX, maxY, maxZ))
                {
                    subproductFound = true;
                    break;
                }
            isSubproductMemo.Add(rotations[0], subproductFound);
            return subproductFound;
        }
        private bool IsSubproductForSingleRotation(Part part, int maxX, int maxY, int maxZ)
        {
            for (int xShift = 0; xShift < product.length - maxX; xShift++)
                for (int yShift = 0; yShift < product.width - maxY; yShift++)
                    for (int zShift = 0; zShift < product.height - maxZ; zShift++)
                        if (IsSubproductForSingleShift(part, maxX, xShift, maxY, yShift, maxZ, zShift))
                            return true;
            return false;
        }
        private bool IsSubproductForSingleShift(Part part, int maxX, int xShift, int maxY, int yShift, int maxZ, int zShift)
        {
            for (int x = 0; x <= maxX; x++)
                for (int y = 0; y <= maxY; y++)
                    for (int z = 0; z <= maxZ; z++)
                        if (part.Get(x, y, z) != product.Get(x + xShift, y + yShift, z + zShift))
                            return false;
            return true;
        }
    }
}
