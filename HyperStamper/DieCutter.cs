using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperStamper
{
    /// <summary>
    /// Turns a die cut pattern into a PartCollection.
    /// </summary>
    class DieCutter
    {
        public static PartCollection GetPartCollection(bool[][] pattern, PartsInfo partsInfo)
        {
            Dictionary<int, List<Tuple<int, int>>> groupToIndices = new Dictionary<int, List<Tuple<int, int>>>();
            int[][] indexToGroup = new int[pattern.Length][];
            for (int i = 0; i < pattern.Length; i++)
            {
                indexToGroup[i] = new int[pattern[0].Length];
            }
            

            int groupCounter = 0;
            for (int x = 0; x < pattern.Length; x++)
            {
                for (int y = 0; y < pattern[0].Length; y++)
                {
                    if (!pattern[x][y])
                        continue;
                    Tuple<int, int> current = new Tuple<int, int>(x, y);
                    int leftGroup = 0;
                    int upGroup = 0;
                    if (x > 0 && indexToGroup[x - 1][y] > 0)
                        leftGroup = indexToGroup[x - 1][y];
                    if (y > 0 && indexToGroup[x][y - 1] > 0)
                        upGroup = indexToGroup[x][y -1];
                    if (leftGroup > 0 ^ upGroup > 0)
                    {
                        indexToGroup[x][y] = leftGroup + upGroup;
                        groupToIndices[leftGroup + upGroup].Add(current);
                    }
                    else if (leftGroup == 0 && upGroup == 0)
                    {
                        groupCounter++;
                        indexToGroup[x][y] = groupCounter;
                        groupToIndices[groupCounter] = new List<Tuple<int, int>>();
                        groupToIndices[groupCounter].Add(current);
                    }
                    else if (leftGroup == upGroup)
                    {
                        indexToGroup[x][y] = leftGroup;
                        groupToIndices[leftGroup].Add(current);
                    }
                    else
                    {
                        foreach (Tuple<int, int> coordinate in groupToIndices[leftGroup])
                        {
                            groupToIndices[upGroup].Add(coordinate);
                            indexToGroup[coordinate.Item1][coordinate.Item2] = upGroup;
                        }
                        groupToIndices.Remove(leftGroup);
                        groupToIndices[upGroup].Add(current);
                        indexToGroup[x][y] = upGroup;
                    }
                }
            }

            PartCollection currentPartCollection = new PartCollection();
            foreach (List<Tuple<int, int>> coors in groupToIndices.Values)
            {
                Part part = CoorsToPart(coors, partsInfo.product.length, partsInfo.product.height);
                // A part collection with bad parts is no good.
                if (part == null)
                    return null;
                currentPartCollection.Add(partsInfo.Canonicalize(part));
            }
            return currentPartCollection;
        }

        private static Part CoorsToPart(List<Tuple<int, int>> coors, byte productLength, byte productHeight)
        {
            int minX = int.MaxValue;
            int maxY = int.MinValue;
            foreach(Tuple<int, int> coor in coors)
            {
                if (coor.Item1 < minX)
                    minX = coor.Item1;
                if (coor.Item2 > maxY)
                    maxY = coor.Item2;
            }
            Part part = new Part(productLength, productLength, productHeight);
            foreach (Tuple<int, int> coor in coors)
            {
                if (coor.Item1 - minX >= productLength)
                    return null;
                if (maxY - coor.Item2 >= productHeight)
                    return null;
                part.Set(coor.Item1 - minX, 0, maxY - coor.Item2, true);
            }
            return part;
        }

        public static PartCollection PartCollectionFromBoolArray(bool[] pattern, int patternLength, PartsInfo partsInfo)
        {
            bool[][] data = new bool[patternLength][];
            for (int i = 0; i < patternLength; i++)
                data[i] = new bool[pattern.Length / patternLength];
            for (int i = 0; i < pattern.Length; i++)
                data[i % data.Length][i / data.Length] = pattern[i];
            return GetPartCollection(data, partsInfo);
        }

        public static bool[][] PatternStringsToBoolArray(string[] patternStrings)
        {
            bool[][] boolArray = new bool[patternStrings[0].Length][];
            for (int x = 0; x < patternStrings[0].Length; x++)
                boolArray[x] = new bool[patternStrings.Length];
            for (int x = 0; x < boolArray.Length; x++)
                for (int y = 0; y < boolArray[0].Length; y++)
                    boolArray[x][y] = patternStrings[y][x] == 'X';
            return boolArray;
        }
    }
}
