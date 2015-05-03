using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    public class PartCollection
    {
        private static StringBuilder stringBuilder = new StringBuilder();

        public SortedList<Part, int> parts;

        public PartCollection()
        {
            parts = new SortedList<Part, int>();
        }
        // Copies a PartCollection, but removes an instance of <used1> and <used2> and adds an instance of <made>.
        public PartCollection(PartCollection partCollection, Part used1, Part used2, Part made)
        {
            parts = new SortedList<Part, int>();
            foreach (KeyValuePair<Part, int> pair in partCollection.parts)
                parts.Add(pair.Key, pair.Value);
            Remove(used1);
            Remove(used2);
            Add(made);
        }

        public void Add(Part part)
        {
            Add(part, 1);
        }
        public void Add(Part part, int quantity)
        {
            if (parts.ContainsKey(part))
                parts[part] += quantity;
            else
                parts.Add(part, quantity);
        }
        public void Remove(Part part)
        {
            Remove(part, 1);
        }
        public void Remove(Part part, int quantity)
        {
            parts[part] -= quantity;
            if (parts[part] <= 0)
                parts.Remove(part);
        }
        public int Quantity()
        {
            int total = 0;
            foreach (int quantity in parts.Values)
                total += quantity;
            return total;
        }

        public Tuple<Part, Part>[] GetPairs()
        {
            int numPairs = parts.Count * (parts.Count - 1) / 2;
            int numDoubleableParts = 0;
            foreach (KeyValuePair<Part, int> pair in parts)
                if (pair.Value >= 2)
                    numDoubleableParts++;
            Tuple<Part, Part>[] pairs = new Tuple<Part, Part>[numPairs + numDoubleableParts];
            int i = 0;
            // All possible combinations of different parts.
            for (int first = 0; first < parts.Count - 1; first++)
                for (int second = first + 1; second < parts.Count; second++)
                {
                    pairs[i] = new Tuple<Part, Part>(parts.Keys[first], parts.Keys[second]);
                    i++;
                }
            // All pairs of the same part that has at least two copies in this collection.
            foreach (KeyValuePair<Part, int> pair in parts)
                if (pair.Value >= 2)
                {
                    pairs[i] = new Tuple<Part, Part>(pair.Key, pair.Key);
                    i++;
                }
            return pairs;
        }

        public int GetTotalCubeCount()
        {
            int cubeCount = 0;
            foreach (KeyValuePair<Part, int> pair in parts)
                for (int i = 0; i < pair.Key.bitArray.Length; i++)
                    if (pair.Key.bitArray[i])
                        cubeCount += pair.Value;
            return cubeCount;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(PartCollection))
                return false;
            PartCollection other = (PartCollection)obj;
            if (parts.Count != other.parts.Count)
                return false;
            for (int i = 0; i < parts.Count; i++)
                if (parts.Keys[i] != other.parts.Keys[i] || parts.Values[i] != other.parts.Values[i])
                    return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (KeyValuePair<Part, int> pair in parts)
                hash ^= pair.Key.GetHashCode() * pair.Value;
            return hash;
        }
        public override string ToString()
        {
            foreach (KeyValuePair<Part, int> pair in parts)
            {
                stringBuilder.Append(pair.Value);
                stringBuilder.Append("x ");
                stringBuilder.AppendLine(pair.Key.ToString());
            }
            string output = stringBuilder.ToString();
            stringBuilder.Clear();
            return output;
        }
    }
}
