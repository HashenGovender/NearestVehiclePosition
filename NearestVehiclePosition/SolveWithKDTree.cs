using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NearestVehiclePosition.Models;

namespace NearestVehiclePosition
{
    public static class SolveWithKDTree
    {
        public static long FileReadParseTime = 0;
        public static long TreeBuildTime = 0;
        public static long FindNearestPositionsTime = 0;

        //--------------------------------------
        // Solve by using 2 dimensional KD-Tree
        //--------------------------------------
        public static VehiclePosition[] Solve(Coordinate[] pointsToQuery, string dataFileName)
        {
            // setup timer to measure execution time
            Stopwatch timer = Stopwatch.StartNew();

            int PositionId;
            string VehicleRegistration;
            float Latitude;
            float Longitude;
            ulong RecordedTimeUTC;

            float[] minDist = new float[pointsToQuery.Length];
            for (int i = 0; i < pointsToQuery.Length; i++)
            {
                minDist[i] = float.MaxValue;
            }

            List<VehiclePosition> vehPositions = new List<VehiclePosition>();
            VehiclePosition[] nearestVehiclePositions = new VehiclePosition[pointsToQuery.Length];

            // Read all file bytes into byte array - this is faster than using BinaryReader class to read values sequentially
            var fileBytes = File.ReadAllBytes(dataFileName);
            int curByteInd = 0;
            int curVehPosInd = 0;
            int byteInd;

            // iterate through the file bytes and read / parse the values according to field data type sizes
            while (curByteInd < fileBytes.Length)
            {
                PositionId = BitConverter.ToInt32(fileBytes, curByteInd);
                curByteInd += 4;

                // read null terminated string representing the Vehicle Registration
                byteInd = curByteInd;
                while (fileBytes[curByteInd] != 0)
                {
                    curByteInd++;
                }
                VehicleRegistration = Encoding.ASCII.GetString(fileBytes, byteInd, curByteInd - byteInd);
                curByteInd++;

                Latitude = BitConverter.ToSingle(fileBytes, curByteInd);
                curByteInd += 4;

                Longitude = BitConverter.ToSingle(fileBytes, curByteInd);
                curByteInd += 4;

                RecordedTimeUTC = BitConverter.ToUInt64(fileBytes, curByteInd);
                curByteInd += 8;

                vehPositions.Add(new VehiclePosition() { PositionId = PositionId, VehicleRegistration = VehicleRegistration, Latitude = Latitude, Longitude = Longitude, RecordedTimeUTC = RecordedTimeUTC });
                curVehPosInd++;
            }

            FileReadParseTime = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();

            // instantiate an object of KDTree class - pass all the vehicle positions read from file to build the tree
            var kdTree = new KDTree(vehPositions);

            TreeBuildTime = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();

            for (int i = 0; i < pointsToQuery.Length; i++)
            {
                // call FindNearestPosition method to get the nearest neighbour for each query point
                nearestVehiclePositions[i] = vehPositions[kdTree.FindNearestPosition(pointsToQuery[i].Lat, pointsToQuery[i].Lon)];
            }

            FindNearestPositionsTime = timer.ElapsedMilliseconds;
            timer.Reset();

            return nearestVehiclePositions;
        }
    }
}
