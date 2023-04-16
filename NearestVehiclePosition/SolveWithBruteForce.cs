using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NearestVehiclePosition
{
    public static class SolveWithBruteForce
    {
        public static long FileReadParseTime = 0;
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

            float tempDist;
            foreach (var vehPosition in vehPositions)
            {
                for (int i = 0; i < pointsToQueryLength; i++)
                {
                    if ((tempDist = MathF.Sqrt(MathF.Pow((pointsToQuery[i, 0] - vehPosition.Latitude), 2) + MathF.Pow((pointsToQuery[i, 1] - vehPosition.Longitude), 2))) < minDist[i])
                    {
                        minDist[i] = tempDist;
                        nearestVehiclePositions[i] = vehPosition;
                    }
                }
            }

            FindNearestPositionsTime = timer.ElapsedMilliseconds;
            timer.Reset();
            
            return nearestVehiclePositions;
        }
    }
}
