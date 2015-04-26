using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HyperStamper
{
    public class PartsInfo
    {
        // The product
        public Part product;
        // Maps from any part to a list of its rotations in canonical order -- rotations whose bit arrays evaluate to
        // lower numbers are more canonical than those whose bit arrays evaluate to larger numbers.
        public Dictionary<Part, List<Part>> partRotations;

        public PartsInfo(String productString)
        {
            // TODO: Check that the product completely fills the area given.

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

            partRotations = new Dictionary<Part, List<Part>>();
        }

        public void Analyze(Part part)
        {
            // TODO: Check that the part is shifted as close to the axes as possible.

            // Find all rotations of part and insert into partRotations with each rotation as key.
            List<Part> rotations = part.GetRotations();
            foreach (Part rotation in rotations)
                partRotations.Add(rotation, rotations);
            
            // TODO: Check if each rotation is a part of the product (or the entire product). If any are, they all are -- but you might want to canonicalize differently in that case.
            // In fact, we might only want to store rotations that are subparts(?)
        }
    }
}
