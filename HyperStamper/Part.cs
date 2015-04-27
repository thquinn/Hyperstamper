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
        // TODO: LWH don't need to be stored in every instance of Part. Maybe PartsInfo should pass dimensions when making calls like GetRotations().
        public byte length, width, height;
        protected BitArray bitArray;

        // Constructors.
        public Part(byte length, byte width, byte height, BitArray bitArray)
        {
            this.length = length;
            this.width = width;
            this.height = height;
            this.bitArray = bitArray;
        }
        public Part(byte length, byte width, byte height, string s)
        {
            this.length = length;
            this.width = width;
            this.height = height;
            List<bool> bools = new List<bool>();
            foreach (char c in s)
                if (c == 'X')
                    bools.Add(true);
                else if (c == 'O')
                    bools.Add(false);
            bitArray = new BitArray(bools.ToArray());
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
            bool symmetric180 = CompareTo(rotation180) == 0;
            if (!symmetric180)
                rotations.Add(rotation180);
            // Add 90- and 270-degree rotations, if there's room for them in the space provided.
            if (maxX < width && maxY < length)
            {
                Part rotation90 = Rotate90(length, width, height, width - 1 - maxY, this);
                if (CompareTo(rotation90) != 0)
                {
                    rotations.Add(rotation90);
                    if (!symmetric180)
                        rotations.Add(Rotate270(length, width, height, length - 1 - maxX, this));
                }
            }
            rotations.Sort();
            return rotations;
        }
        // Returns a new Part equivalent to bitArray rotation 180 degrees about the Z axis and shifted as close to the
        // X and Y axes as possible. <shiftX> and <shiftY> are the amounts of empty space on the end of each axis in
        // the original Part.
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
        public static Part Rotate90(byte length, byte width, byte height, int shiftY, Part part)
        {
            Part output = new Part(length, width, height);
            int min = Math.Min(length, width);
            int adjustedShiftY = shiftY - (length - min);
            for (int x = 0; x < min; x++)
                for (int y = 0; y < min; y++)
                    for (int z = 0; z < height; z++)
                        if (part.Get(x, y, z))
                            output.Set(min - y - 1 - adjustedShiftY, x, z, true);
            return output;
        }
        public static Part Rotate270(byte length, byte width, byte height, int shiftX, Part part)
        {
            Part output = new Part(length, width, height);
            int min = Math.Min(length, width);
            int adjustedShiftX = shiftX - (width - min);
            for (int x = 0; x < min; x++)
                for (int y = 0; y < min; y++)
                    for (int z = 0; z < height; z++)
                        if (part.Get(x, y, z))
                            output.Set(y, min - x - 1 - adjustedShiftX, z, true);
            return output;
        }

        // Override methods.
        public int CompareTo(Part other)
        {
            // Comparisons are only valid with two parts of identical dimensions.
            if (length != other.length || width != other.width || height != other.height)
                return 0;
            for (int i = bitArray.Length - 1; i >= 0; i--)
                if (bitArray.Get(i) != other.bitArray.Get(i))
                    return bitArray.Get(i) ? 1 : -1;
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
            // Hash each 32-bit chunk of the bit array and XOR them all together.
            int hash = 0;
            uint u = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (i % 32 == 0 && i != 0)
                {
                    hash ^= u.GetHashCode();
                    u = 0;
                }
                u *= 2;
                if (bitArray.Get(bitArray.Length - i - 1))
                    u++;
            }
            hash ^= u.GetHashCode();
            return hash;
        }
        public override string ToString()
        {
            for (int i = 0; i < bitArray.Length; i++)
                stringBuilder.Append(bitArray.Get(i) ? 'X' : 'O');
            string output = stringBuilder.ToString();
            stringBuilder.Clear();
            return output;
        }
    }
}
