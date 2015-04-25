using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HyperStamper
{
    public class Part : IComparable<Part>
    {
        private static StringBuilder stringBuilder = new StringBuilder();

        // Size of the product in the X, Y, and Z dimensions, as well as information about the absence (0) or
        // presence (1) of a block at each coordinate.
        public byte length, width, height;
        public BitArray bitArray;

        // Constructors.
        public Part(byte length, byte width, byte height, BitArray bitArray)
        {
            this.length = length;
            this.width = width;
            this.height = height;
            this.bitArray = bitArray;
        }
        public Part(byte length, byte width, byte height)
        {
            this.length = length;
            this.width = width;
            this.height = height;
            bitArray = new BitArray(length * width * height);
        }
        public bool Get(int x, int y, int z)
        {
            return bitArray.Get(z * length * width + y * length + x);
        }
        public void Set(int x, int y, int z, bool value)
        {
            bitArray.Set(z * length * width + y * length + x, value);
        }

        // Returns a list of this Part's rotations, itself included, in canonical order.
        public List<Part> GetRotations()
        {
            List<Part> rotations = new List<Part>();
            rotations.Add(this);
            // Find the max X and Y to determine how to shift after rotation.
            int maxX = -1, maxY = -1;
            for (int x = 0; x < length; x++)
                for (int y = 0; y < width; y++)
                    for (int z = 0; z < height; z++)
                        if (Get(x, y, z))
                        {
                            maxX = Math.Max(x, maxX);
                            maxY = Math.Max(y, maxY);
                        }
            // Add 180-degree rotation.
            Part rotation180 = Rotate180(length, width, height, length - 1 - maxX, width - 1 - maxY, this);
            if (CompareTo(rotation180) != 0)
                rotations.Add(rotation180);
            // Add 90- and 270-degree rotations, if there's room for them in the space provided.
            if (maxX < width && maxY < length)
            {
                // TODO: Add rotations.
            }
            rotations.Sort();
            return rotations;
        }
        // Returns a new Part equivalent to bitArray rotation 180 degrees about the Z axis and shifted as close to the X and Y axes as possible.
        public static Part Rotate180(byte length, byte width, byte height, int shiftX, int shiftY, Part part)
        {
            Part output = new Part(length, width, height);
            for (int x = 0; x < length; x++)
                for (int y = 0; y < width; y++)
                    for (int z = 0; z < height; z++)
                        if (part.Get(x, y, z))
                            output.Set(length - x - 1 - shiftX, width - y - 1 - shiftY, z, true);
            return output;
        }

        // Override methods.
        public int CompareTo(Part other)
        {
            int thisIndex = Math.Min(0, bitArray.Length - other.bitArray.Length);
            int otherIndex = Math.Min(0, other.bitArray.Length - bitArray.Length);

            while (thisIndex != bitArray.Length)
            {
                bool thisBit = thisIndex < 0 ? false : bitArray.Get(thisIndex);
                bool otherBit = otherIndex < 0 ? false : other.bitArray.Get(thisIndex);
                if (thisBit != otherBit)
                    return thisBit ? 1 : -1;
                thisIndex++;
                otherIndex++;
            }

            return 0;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Part))
                return false;
            return CompareTo(obj as Part) == 0;
        }
        public override int GetHashCode()
        {
            int result = 29;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray.Get(i))
                    result++;
                result *= 23;
            }
            return result;
        }
        public override string ToString()
        {
            for (int i = 0; i < bitArray.Length; i++)
                stringBuilder.Append(bitArray.Get(i) ? '1' : '0');
            string output = stringBuilder.ToString();
            stringBuilder.Clear();
            return output;
        }
    }
}
