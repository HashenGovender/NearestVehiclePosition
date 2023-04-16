using System.Diagnostics;

namespace NearestVehiclePosition
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string dataFileName = "VehiclePositions.dat";

            //------------------------------
            // SOLVING BY USING BRUTE FORCE 
            //-------------------------------

            Stopwatch timer = Stopwatch.StartNew();
            Console.WriteLine("#########################################################");
            Console.WriteLine("################# Solve With Brute Force ################");
            Console.WriteLine("#########################################################");
            Console.WriteLine();

            var nearestPositions = SolveWithBruteForce.Solve(PointsToQuery.Points, dataFileName);
            DisplayNearestNeighbours(nearestPositions, PointsToQuery.Points);

            Console.WriteLine();
            Console.WriteLine("File Read / Parse Time (ms): " + SolveWithBruteForce.FileReadParseTime);
            Console.WriteLine("Find Nearest Positions Time (ms): " + SolveWithBruteForce.FindNearestPositionsTime);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();


            //--------------------------
            // SOLVING BY USING KD-TREE 
            //--------------------------

            Console.WriteLine("#########################################################");
            Console.WriteLine("################## Solve With KD-Tree ###################");
            Console.WriteLine("#########################################################");
            Console.WriteLine();

            nearestPositions = SolveWithKDTree.Solve(PointsToQuery.Points, dataFileName);
            DisplayNearestNeighbours(nearestPositions, PointsToQuery.Points);

            Console.WriteLine();
            Console.WriteLine("File Read / Parse Time (ms): " + SolveWithKDTree.FileReadParseTime);
            Console.WriteLine("KD-Tree Build Time (ms): " + SolveWithKDTree.TreeBuildTime);
            Console.WriteLine("Find Nearest Positions Time (ms): " + SolveWithKDTree.FindNearestPositionsTime);
            Console.WriteLine();
            Console.WriteLine();

        }


        static void DisplayNearestNeighbours(VehiclePosition?[] nearestVehiclePositions, float[,] pointsToQuery)
        {
            for (int i = 0; i < pointsToQuery.GetLength(0); i++)
            {
                if (nearestVehiclePositions[i].HasValue)
                {
                    Console.WriteLine($"Nearest position to ({pointsToQuery[i, 0]}, {pointsToQuery[i, 1]}) is: \n[PositionId:{nearestVehiclePositions[i].Value.PositionId}, Latitude:{nearestVehiclePositions[i].Value.Latitude}, Longitude:{nearestVehiclePositions[i].Value.Longitude}, Registration:{nearestVehiclePositions[i].Value.VehicleRegistration}, DateTime:{ DateTimeOffset.FromUnixTimeSeconds((long)nearestVehiclePositions[i].Value.RecordedTimeUTC).LocalDateTime}]\n");
                }          
            }
        }
    }
}