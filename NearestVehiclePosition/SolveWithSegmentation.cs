using System.Diagnostics;
using System.Text;
using NearestVehiclePosition.Models;

namespace NearestVehiclePosition
{
    public static class SolveWithSegmentation
    {
        public static long FileReadParseTime = 0;
        public static long FindNearestPositionsTime = 0;

        //------------------------------------------------------------------------------------------------
        // Solve by using segmentation - divide the points into a grid of segments
        //------------------------------------------------------------------------------------------------
        public static VehiclePosition[] Solve(Coordinate[] pointsToQuery, string dataFileName)
        {
            // setup timer to measure execution time
            Stopwatch timer = Stopwatch.StartNew();


            // setup a 2d array to store the segments indexed by latitude and longitude with stripped off decimals
            // each segment contains a list of VehiclePosition objects
            List<VehiclePosition>[,] segments = new List<VehiclePosition>[180,360];


            //declare vars to hold parsed data from binary file
            int PositionId;
            string VehicleRegistration;
            float Latitude;
            float Longitude;
            ulong RecordedTimeUTC;


            //declare nearestVehiclePositions which will be populated and returned from the function
            VehiclePosition[] nearestVehiclePositions = new VehiclePosition[pointsToQuery.Length];


            // Read all file bytes into byte array - this is faster than using BinaryReader class to read values sequentially
            var fileBytes = File.ReadAllBytes(dataFileName);
            int curByteInd = 0;
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

                // generate the segment indices used to index into the segments array, strip off decimals and offset to make positive
                int segmentIndLat = (int)(Latitude+90);
                int segmentIndLon = (int)(Longitude+180);

                if (segments[segmentIndLat,segmentIndLon] == null)
                {
                    segments[segmentIndLat, segmentIndLon] = new List<VehiclePosition>();
                }
                segments[segmentIndLat, segmentIndLon].Add(new VehiclePosition() { PositionId = PositionId, VehicleRegistration = VehicleRegistration, Latitude = Latitude, Longitude = Longitude, RecordedTimeUTC = RecordedTimeUTC });

            }

            FileReadParseTime = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();


            // iterate through each query point and find its nearest vehicle position
            for (int i = 0; i < pointsToQuery.Length; i++)
            {
                float minDistSoFar = float.MaxValue;
                VehiclePosition nearestVehPosSoFar = new VehiclePosition();
                float sqDist;

                bool terminate = false;
                int iteration = 0;

                // check each segment of positions in clockwise order starting with the query point and spiraling out
                // with each iteration until the minimum distance is found and finalised 
                while (!terminate)
                {
                    if (minDistSoFar < float.MaxValue)
                        terminate = true;

                    // with each iteration extend the bounds of the outer "ring" of segments to check
                    for (int x = -iteration; x <= iteration; x++)
                    {
                        for (int y = -iteration; y <= iteration; y++)
                        {
                            // generate the segment indices of the current segment to check and index into the segments array
                            int segmentIndLat = (int)(pointsToQuery[i].Lat + 90 + x);
                            int segmentIndLon = (int)(pointsToQuery[i].Lon + 180 + y);


                            // iterate through all the positions in the current segment and check the square distance to the query point 
                            if (segmentIndLat >= 0 && segmentIndLon >= 0 && segments[segmentIndLat, segmentIndLon] != null)
                            {
                                foreach (var pos in segments[segmentIndLat, segmentIndLon])
                                {
                                    // if square distance is less than minDistSoFar update it and keep track of the nearest vehicle position object
                                    if ((sqDist = MathF.Pow((pointsToQuery[i].Lat - pos.Latitude), 2) + MathF.Pow((pointsToQuery[i].Lon - pos.Longitude), 2)) < minDistSoFar)
                                    {
                                        minDistSoFar = sqDist;
                                        nearestVehPosSoFar = pos;
                                    }
                                }
                            }
                        }
                    }

                    iteration++;
                }

                nearestVehiclePositions[i] = nearestVehPosSoFar;
            }

            FindNearestPositionsTime = timer.ElapsedMilliseconds;
            timer.Reset();
            
            return nearestVehiclePositions;
        }
    }
}
