using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NearestVehiclePosition
{
    public static class SolveWithKDTree
    {
        public static long FileReadParseTime = 0;
        public static long TreeBuildTime = 0;
        public static long FindNearestPositionsTime = 0;

        public static VehiclePosition?[] Solve(float[,] pointsToQuery, string dataFileName)
        {
            Stopwatch timer = Stopwatch.StartNew();

            int pointsToQueryLength = pointsToQuery.GetLength(0);
            int PositionId;
            string VehicleRegistration;
            float Latitude;
            float Longitude;
            ulong RecordedTimeUTC;

            float[] minDist = new float[pointsToQueryLength];
            for (int i = 0; i < pointsToQueryLength; i++)
            {
                minDist[i] = float.MaxValue;
            }

            List<VehiclePosition> vehPositions = new List<VehiclePosition>();
            VehiclePosition?[] nearestVehiclePositions = new VehiclePosition?[pointsToQueryLength];

            var fileBytes = File.ReadAllBytes(dataFileName);
            int curByteInd = 0;
            int curVehPosInd = 0;
            int byteInd;

            while (curByteInd < fileBytes.Length)
            {
                PositionId = BitConverter.ToInt32(fileBytes, curByteInd);
                curByteInd += 4;

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

            var kdTree = new KDTree(vehPositions.ToArray());

            TreeBuildTime = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();

            for (int i = 0; i < pointsToQueryLength; i++)
            {
                nearestVehiclePositions[i] = kdTree.FindNearestPosition(pointsToQuery[i, 0], pointsToQuery[i, 1]);
            }

            FindNearestPositionsTime = timer.ElapsedMilliseconds;
            timer.Reset();

            return nearestVehiclePositions;
        }
    }
}
