using System.Diagnostics;
using System.Text;
using NearestVehiclePosition.Models;

namespace NearestVehiclePosition
{
    public static class SolveWithBruteForce
    {
        public static long FileReadParseTime = 0;
        public static long FindNearestPositionsTime = 0;

        //------------------------------------------------------------------------------------------------
        // Solve by using brute force - check distance from each query point to all positions in the file
        //------------------------------------------------------------------------------------------------
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

            // iterate through all positions and query points and keep track of closest distance so far
            float tempDist;
            foreach (var vehPosition in vehPositions)
            {
                for (int i = 0; i < pointsToQuery.Length; i++)
                {
                    if ((tempDist = MathF.Sqrt(MathF.Pow((pointsToQuery[i].Lat - vehPosition.Latitude), 2) + MathF.Pow((pointsToQuery[i].Lon - vehPosition.Longitude), 2))) < minDist[i])
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
