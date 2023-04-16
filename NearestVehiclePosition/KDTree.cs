using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NearestVehiclePosition.Models;

namespace NearestVehiclePosition
{
    //--------------------------------------------------------------------------------------------------------
    // Provides functionality to build a 2 dimensional KD-Tree and find the nearest neighbour to a given point
    // An explanation of KD-Trees can be found here: https://en.wikipedia.org/wiki/K-d_tree
    //--------------------------------------------------------------------------------------------------------
    public class KDTree
    {
        // To optimise the build time of the tree I am storing the the vehicle position indices, latitudes and longitudes in 
        // flat primitive arrays which are used in the sorting algorithm to build the tree instead of working with the
        // VehiclePosition objects directly which slows things down

        // Store the indices
        public int[] indices;
        // Store the latitudes
        public float[] lats;
        // Store the longitudes
        public float[] lons;


        public KDTree(List<VehiclePosition> items)
        {
            int len = items.Count;
            lats = new float[len];
            lons = new float[len];

            indices = new int[len];
            for (int ind = 0; ind < indices.Length; ind++)
            {
                indices[ind] = ind;
                lats[ind] = items[ind].Latitude;
                lons[ind] = items[ind].Longitude;
            }

            // Build 2 dimensional KD-Tree by sorting the indices, lats and lons arrays in place
            SortKDTree(0, indices.Length - 1, 0);
        }


        // Sort the elements to form a KD-Tree 
        private void SortKDTree(int left, int right, int depth)
        {
            if (right - left <= 0) return;

            int middle = left + right >> 1;

            QuickSelectFloydRivest(middle, left, right, depth % 2);

            SortKDTree(left, middle - 1, depth + 1);
            SortKDTree(middle + 1, right, depth + 1);
        }


        //-------------------------------------------------------------------------------------------------------------------
        // Using Floyd-Rivest Select algorithm since it performs faster in this case than the regular Quick Select algorithm
        // Uses sampling to partition the array into 3 sets to find the median element
        // Floyd-Rivest Select algorithm is documented here: https://en.wikipedia.org/wiki/Floyd%E2%80%93Rivest_algorithm
        //-------------------------------------------------------------------------------------------------------------------
        private void QuickSelectFloydRivest(int middle, int left, int right, int sortingDimension)
        {
            while (right > left)
            {
                // default constants used as prescribed by the algorithm
                if (right - left > 600)
                {
                    var n = right - left + 1;
                    var m = middle - left + 1;
                    var z = Math.Log(n);
                    var s = 0.5 * Math.Exp(2 * z / 3);
                    var sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * (m - n / 2 < 0 ? -1 : 1);
                    var newLeft = (int)Math.Max(left, Math.Floor(middle - m * s / n + sd));
                    var newRight = (int)Math.Min(right, Math.Floor(middle + (n - m) * s / n + sd));
                    QuickSelectFloydRivest(middle, newLeft, newRight, sortingDimension);
                }

                var t = sortingDimension == 0 ? lats[middle] : lons[middle];
                var i = left;
                var j = right;

                SwapItem(left, middle);
                if ((sortingDimension == 0 ? lats[right] : lons[right]) > t) SwapItem(left, right);

                while (i < j)
                {
                    SwapItem(i, j);
                    i++;
                    j--;
                    while ((sortingDimension == 0 ? lats[i] : lons[i]) < t) i++;
                    while ((sortingDimension == 0 ? lats[j] : lons[j]) > t) j--;
                }

                if ((sortingDimension == 0 ? lats[left] : lons[left]) == t) SwapItem(left, j);
                else
                {
                    j++;
                    SwapItem(j, right);
                }

                if (j <= middle) left = j + 1;
                if (middle <= j) right = j - 1;
            }
        }

        // Swap the elements of all 3 arrays to keep them in sync with each other
        private void SwapItem(int i, int j)
        {
            Swap(indices, i, j);
            Swap(lats, i, j);
            Swap(lons, i, j);
        }

        private static void Swap<T>(T[] arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }




        // Keep track of the array index of the nearest point found so far
        private int bestNeighbourInd = -1;
        // Keep track of the distance to nearest point found so far
        private float bestDistance = 0;

        // Return the index of the nearest VehiclePosition
        public int FindNearestPosition(float lat, float lon)
        {
            bestNeighbourInd = -1;
            bestDistance = 0;

            NearestNeighbour(0, indices.Length - 1, lat, lon);

            return indices[bestNeighbourInd];
        }

        //------------------------------------------------------------------------------------------------------
        // Find the nearest position to the target latitude and longitude by traversing the KD-Tree recursively
        // Similar to binary tree traversal, alternating the comparision of lat/lon at each depth to check
        // whether to proceed left or right
        //------------------------------------------------------------------------------------------------------
        private void NearestNeighbour(int left, int right, float targetLat, float targetLon, int axis = 0)
        {
            if (right - left < 0)
                return;

            int middle = left + right >> 1;

            float squareDist = MathF.Pow(lats[middle] - targetLat, 2) + MathF.Pow(lons[middle] - targetLon, 2);

            if (bestNeighbourInd < 0 || squareDist < bestDistance)
            {
                bestDistance = squareDist;
                bestNeighbourInd = middle;
            }

            if (bestDistance == 0)
                return;

            float axisDist = (axis == 0 ? lats[middle] : lons[middle]) - (axis == 0 ? targetLat : targetLon);

            NearestNeighbour(axisDist > 0 ? left : middle + 1, axisDist > 0 ? middle - 1 : right, targetLat, targetLon, (axis + 1) % 2);

            if (MathF.Pow(axisDist, 2) >= bestDistance)
                return;

            NearestNeighbour(axisDist > 0 ? middle + 1 : left, axisDist > 0 ? right : middle - 1, targetLat, targetLon, (axis + 1) % 2);
        }

    }
}
