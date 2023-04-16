using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NearestVehiclePosition.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NearestVehiclePosition
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //----------------------------------------------------------------
            // SETUP DATA FILE PATH AND POINTS TO QUERY FROM appSettings.json 
            //----------------------------------------------------------------
            IConfiguration Config = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json")
                    .Build();

            string dataFileName = Config.GetSection("DataFilePath").Value;
            var pointsToQuery = Config.GetSection("PointsToQuery").Get<Coordinate[]>();
            
            
            //------------------------------
            // SOLVING BY USING BRUTE FORCE 
            //------------------------------
            Stopwatch timer = Stopwatch.StartNew();
            Console.WriteLine("#########################################################");
            Console.WriteLine("################# Solve With Brute Force ################");
            Console.WriteLine("#########################################################");
            Console.WriteLine();

            var nearestPositionsBF = SolveWithBruteForce.Solve(pointsToQuery, dataFileName);
            DisplayNearestNeighbours(nearestPositionsBF, pointsToQuery);

            Console.WriteLine();
            Console.WriteLine("Data File Read / Parse Time (ms): " + SolveWithBruteForce.FileReadParseTime);
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

            var nearestPositionsKD = SolveWithKDTree.Solve(pointsToQuery, dataFileName);
            DisplayNearestNeighbours(nearestPositionsKD, pointsToQuery);

            Console.WriteLine();
            Console.WriteLine("Data File Read / Parse Time (ms): " + SolveWithKDTree.FileReadParseTime);
            Console.WriteLine("KD-Tree Build Time (ms): " + SolveWithKDTree.TreeBuildTime);
            Console.WriteLine("Find Nearest Positions Time (ms): " + SolveWithKDTree.FindNearestPositionsTime);
            Console.WriteLine();
            Console.WriteLine();

            Console.ReadLine();
        }


        //-----------------------------------
        // Display nearest neighbour results
        //-----------------------------------
        static void DisplayNearestNeighbours(VehiclePosition[] nearestVehiclePositions, Coordinate[] pointsToQuery)
        {
            for (int i = 0; i < pointsToQuery.Length; i++)
            {
               Console.WriteLine($"Nearest position to ({pointsToQuery[i].Lat}, {pointsToQuery[i].Lon}) is: \n[PositionId:{nearestVehiclePositions[i].PositionId}, Latitude:{nearestVehiclePositions[i].Latitude}, Longitude:{nearestVehiclePositions[i].Longitude}, Registration:{nearestVehiclePositions[i].VehicleRegistration}, DateTime:{ DateTimeOffset.FromUnixTimeSeconds((long)nearestVehiclePositions[i].RecordedTimeUTC).LocalDateTime}]\n");
            }
        }
    }
}