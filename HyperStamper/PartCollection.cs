using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    public class PartCollection
    {
        protected SortedList<Part, int> parts;

        public PartCollection()
        {
            parts = new SortedList<Part, int>();
        }
        public PartCollection(PartCollection partCollection)
        {
            parts = new SortedList<Part, int>();
            foreach (KeyValuePair<Part, int> pair in partCollection.parts)
                parts.Add(pair.Key, pair.Value);
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
            {
                hash ^= pair.Key.GetHashCode();
                hash ^= pair.Value.GetHashCode();
            }
            return hash;
        }
    }
}
