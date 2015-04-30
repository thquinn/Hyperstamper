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
        private Dictionary<int, HashSet<Part>> allSubproductCombinationsMemo;
        // The product we're trying to create by combining parts.
        public Part product;

        // Optimization variables.
        public int memoedAnalyzeCalls = 0, totalAnalyzeCalls = 0, memoedAllSubproductCalls = 0, totalAllSubproductCalls = 0;

        public PartsInfo(String productString)
        {
            // Initialize dictionaries.
            partRotations = new Dictionary<Part, List<Part>>();
            isSubproductMemo = new Dictionary<Part, bool>();
            allSubproductCombinationsMemo = new Dictionary<int, HashSet<Part>>();

            // TODO: Check that the product completely fills the area given.
            // TODO: Enforce equal length and width.

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
            totalAnalyzeCalls++;
            if (partRotations.ContainsKey(part))
            {
                memoedAnalyzeCalls++;
                return;
            }

            // Find all rotations of part and insert into partRotations with each rotation as key.
            List<Part> rotations = part.GetRotations();
            foreach (Part rotation in rotations)
                partRotations.Add(rotation, rotations);
        }
        public void Analyze(PartCollection partCollection)
        {
            foreach (Part part in partCollection.parts.Keys)
                Analyze(part);
        }

        // Returns the canonical rotation of the given part.
        public Part Canonicalize(Part part)
        {
            Analyze(part);
            return partRotations[part][0];
        }

        // Returns the canonical rotations of all parts that can be produced by welding the two inputs together that
        // are substructures of the product. <first> and <second> must be sorted.
        public HashSet<Part> AllSubproductCombinations(Part first, Part second)
        {
            // If both parts are the same, just use that part's hash as the memo dictionary key. Otherwise, XOR them.
            int firstHash = first.GetHashCode();
            int secondHash = second.GetHashCode();
            int xoredHash = first.Equals(second) ? first.GetHashCode() : first.GetHashCode() ^ second.GetHashCode();
            totalAllSubproductCalls++;
            if (allSubproductCombinationsMemo.ContainsKey(xoredHash))
            {
                memoedAllSubproductCalls++;
                return allSubproductCombinationsMemo[xoredHash];
            }

            int[] firstMaxCoordinates = first.MaxCoordinates();
            // Find each block on the first part that has a face.
            List<Tuple<byte, byte, byte>> firstLeftFaces = new List<Tuple<byte,byte,byte>>();
            List<Tuple<byte, byte, byte>> firstRightFaces = new List<Tuple<byte,byte,byte>>();
            List<Tuple<byte, byte, byte>> firstBackFaces = new List<Tuple<byte,byte,byte>>();
            List<Tuple<byte, byte, byte>> firstFrontFaces = new List<Tuple<byte,byte,byte>>();
            List<Tuple<byte, byte, byte>> firstBottomFaces = new List<Tuple<byte,byte,byte>>();
            List<Tuple<byte, byte, byte>> firstTopFaces = new List<Tuple<byte,byte,byte>>();
            for (byte x = 0; x < first.length; x++)
                for (byte y = 0; y < first.width; y++)
                    for (byte z = 0; z < first.height; z++)
                    {
                        if (first.Get(x, y, z) && (x == 0 || !first.Get(x - 1, y, z)))
                            firstLeftFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        if (first.Get(x, y, z) && (x == first.length - 1 || !first.Get(x + 1, y, z)))
                            firstRightFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        if (first.Get(x, y, z) && (y == 0 || !first.Get(x, y - 1, z)))
                            firstBackFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        if (first.Get(x, y, z) && (y == first.width - 1 || !first.Get(x, y + 1, z)))
                            firstFrontFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        if (first.Get(x, y, z) && (z == 0 || !first.Get(x, y, z - 1)))
                            firstBottomFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        if (first.Get(x, y, z) && (z == first.height - 1 || !first.Get(x, y, z + 1)))
                            firstTopFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                    }

            // TODO: Find every combination, Analyze() each, and return the ones that are subproducts.
            HashSet<Part> subproducts = new HashSet<Part>();
            foreach (Part secondRotation in partRotations[second])
            {
                int[] secondMaxCoordinates = secondRotation.MaxCoordinates();
                // Find each block on this rotation of the second part that has a face.
                List<Tuple<byte, byte, byte>> secondLeftFaces = new List<Tuple<byte, byte, byte>>();
                List<Tuple<byte, byte, byte>> secondRightFaces = new List<Tuple<byte, byte, byte>>();
                List<Tuple<byte, byte, byte>> secondBackFaces = new List<Tuple<byte, byte, byte>>();
                List<Tuple<byte, byte, byte>> secondFrontFaces = new List<Tuple<byte, byte, byte>>();
                List<Tuple<byte, byte, byte>> secondBottomFaces = new List<Tuple<byte, byte, byte>>();
                List<Tuple<byte, byte, byte>> secondTopFaces = new List<Tuple<byte, byte, byte>>();
                for (byte x = 0; x < secondRotation.length; x++)
                    for (byte y = 0; y < secondRotation.width; y++)
                        for (byte z = 0; z < secondRotation.height; z++)
                        {
                            if (secondRotation.Get(x, y, z) && (x == 0 || !secondRotation.Get(x - 1, y, z)))
                                secondLeftFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                            if (secondRotation.Get(x, y, z) && (x == secondRotation.length - 1 || !secondRotation.Get(x + 1, y, z)))
                                secondRightFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                            if (secondRotation.Get(x, y, z) && (y == 0 || !secondRotation.Get(x, y - 1, z)))
                                secondBackFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                            if (secondRotation.Get(x, y, z) && (y == secondRotation.width - 1 || !secondRotation.Get(x, y + 1, z)))
                                secondFrontFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                            if (secondRotation.Get(x, y, z) && (z == 0 || !secondRotation.Get(x, y, z - 1)))
                                secondBottomFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                            if (secondRotation.Get(x, y, z) && (z == secondRotation.height - 1 || !secondRotation.Get(x, y, z + 1)))
                                secondTopFaces.Add(new Tuple<byte, byte, byte>(x, y, z));
                        }

                // Create all combinations of left/right, back/front, top/bottom faces.
                Part tempSubproduct;
                foreach (Tuple<byte, byte, byte> firstFace in firstLeftFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondRightFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, -1, 0, 0, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                foreach (Tuple<byte, byte, byte> firstFace in firstRightFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondLeftFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, 1, 0, 0, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                foreach (Tuple<byte, byte, byte> firstFace in firstBackFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondFrontFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, 0, -1, 0, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                foreach (Tuple<byte, byte, byte> firstFace in firstFrontFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondBackFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, 0, 1, 0, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                foreach (Tuple<byte, byte, byte> firstFace in firstBottomFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondTopFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, 0, 0, -1, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                foreach (Tuple<byte, byte, byte> firstFace in firstTopFaces)
                    foreach (Tuple<byte, byte, byte> secondFace in secondBottomFaces)
                        if (CombineSubproduct(first, secondRotation, firstFace, secondFace, 0, 0, 1, firstMaxCoordinates, secondMaxCoordinates, out tempSubproduct))
                            subproducts.Add(tempSubproduct);
                // TODO: Prevent duplicated combinations (i.e. different block pairs that nevertheless result in the
                // same adjacency between parts).    
            }
            // Memoize.
            allSubproductCombinationsMemo.Add(xoredHash, subproducts);

            return subproducts;
        }
        // Returns true if the two parts can be combined in the provided configuration without intersection, fit within
        // the product bounding cube, and result in a subproduct.
        private bool CombineSubproduct(Part first, Part second, Tuple<byte, byte, byte> firstBlock, Tuple<byte, byte, byte> secondBlock, int dx, int dy, int dz, int[] firstMaxCoordinates, int[] secondMaxCoordinates, out Part subproduct)
        {
            subproduct = null;

            /* Reject combinations that don't fit in the product bounding cube.
             * PROBLEM: This doesn't actually cover all cases. A dz of -1 can put a wide piece on top of another piece
             * with an offset that causes it to go out of bounds in X or Y.
             */
            if (dx == -1 && (firstMaxCoordinates[0] - firstBlock.Item1) + secondBlock.Item1 + 2 > product.length) // left-right
                return false;
            if (dx == 1 && (secondMaxCoordinates[0] - secondBlock.Item1) + firstBlock.Item1 + 2 > product.length) // right-left
                return false;
            if (dy == -1 && (firstMaxCoordinates[1] - firstBlock.Item2) + secondBlock.Item2 + 2 > product.width) // back-front
                return false;
            if (dy == 1 && (secondMaxCoordinates[1] - secondBlock.Item2) + firstBlock.Item2 + 2 > product.width) // front-back
                return false;
            if (dz == -1 && (firstMaxCoordinates[2] - firstBlock.Item3) + secondBlock.Item3 + 2 > product.height) // bottom-top
                return false;
            if (dz == 1 && (secondMaxCoordinates[2] - secondBlock.Item3) + firstBlock.Item3 + 2 > product.height) // top-bottom
                return false;

            // Create the combination.
            Part combination = new Part(product.length, product.width, product.height);
            for (int x = 0; x < first.length; x++)
                for (int y = 0; y < first.width; y++)
                    for (int z = 0; z < first.height; z++)
                        if (first.Get(x, y, z))
                        {
                            int newX = Math.Max(x, secondBlock.Item1 - dx + (x - firstBlock.Item1));
                            int newY = Math.Max(y, secondBlock.Item2 - dy + (y - firstBlock.Item2));
                            int newZ = Math.Max(z, secondBlock.Item3 - dz + (z - firstBlock.Item3));
                            // TEMPORARY FIX: If the combination would go outside bounds, reject it.
                            if (newX < 0 || newX >= product.length || newY < 0 || newY >= product.width || newZ < 0 || newZ >= product.height)
                                return false;
                            combination.Set(newX, newY, newZ, true);
                        }
            for (int x = 0; x < second.length; x++)
                for (int y = 0; y < second.width; y++)
                    for (int z = 0; z < second.height; z++)
                    {
                        if (!second.Get(x, y, z))
                            continue;
                        int newX = Math.Max(x, firstBlock.Item1 + dx + (x - secondBlock.Item1));
                        int newY = Math.Max(y, firstBlock.Item2 + dy + (y - secondBlock.Item2));
                        int newZ = Math.Max(z, firstBlock.Item3 + dz + (z - secondBlock.Item3));
                        // TEMPORARY FIX: If the combination would go outside bounds, reject it.
                        if (newX < 0 || newX >= product.length || newY < 0 || newY >= product.width || newZ < 0 || newZ >= product.height)
                            return false;
                        // If there's any intersection between the parts, reject the whole thing.
                        if (combination.Get(newX, newY, newZ))
                            return false;

                        combination.Set(newX, newY, newZ, true);
                    }
            Analyze(combination);
            if (!IsSubproduct(combination))
                return false;
            subproduct = Canonicalize(combination);
            return true;
        }


        // Check if each rotation is a part of the product (or is the entire product). If any are, they all are. Store
        // the result in isSubproductMemo under the canonical rotation.
        public bool IsSubproduct(Part part)
        {
            // If we've seen seen this part before, return the same result.
            if (isSubproductMemo.ContainsKey(Canonicalize(part)))
                return isSubproductMemo[Canonicalize(part)];

            // Check all positions of each rotation against the product.
            List<Part> rotations = partRotations[part];
            bool subproductFound = false;
            foreach (Part rotation in rotations)
                if (IsSubproductForSingleRotation(rotation))
                {
                    subproductFound = true;
                    break;
                }
            isSubproductMemo.Add(rotations[0], subproductFound);
            return subproductFound;
        }
        private bool IsSubproductForSingleRotation(Part part)
        {
            int[] maxCoordinates = part.MaxCoordinates();
            for (int xShift = 0; xShift < product.length - maxCoordinates[0]; xShift++)
                for (int yShift = 0; yShift < product.width - maxCoordinates[1]; yShift++)
                    for (int zShift = 0; zShift < product.height - maxCoordinates[2]; zShift++)
                        if (IsSubproductForSingleShift(part, maxCoordinates[0], xShift, maxCoordinates[1], yShift, maxCoordinates[2], zShift))
                            return true;
            return false;
        }
        private bool IsSubproductForSingleShift(Part part, int maxX, int xShift, int maxY, int yShift, int maxZ, int zShift)
        {
            for (int x = 0; x <= maxX; x++)
                for (int y = 0; y <= maxY; y++)
                    for (int z = 0; z <= maxZ; z++)
                        if (part.Get(x, y, z) && !product.Get(x + xShift, y + yShift, z + zShift))
                            return false;
            return true;
        }
    }
}
