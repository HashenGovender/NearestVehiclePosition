using Microsoft.Extensions.Configuration;
using NearestVehiclePosition;
using NearestVehiclePosition.Models;

namespace NearestVehiclePositionTests
{
    [TestClass]
    public class NearestVehicleTests
    {
        [TestMethod]
        public void Test_Solve_With_Brute_Force_Matches_Solve_With_KDTree()
        {
            IConfiguration Config = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json")
                    .Build();

            string dataFileName = Config.GetSection("DataFilePath").Value;
            var pointsToQuery = Config.GetSection("PointsToQuery").Get<Coordinate[]>();

            var nearestNeighboursBruteForce = SolveWithBruteForce.Solve(pointsToQuery, dataFileName);
            var nearestNeighboursKDTree = SolveWithKDTree.Solve(pointsToQuery, dataFileName);

            Assert.AreEqual(nearestNeighboursBruteForce.Length, nearestNeighboursKDTree.Length);

            for (int i = 0; i < nearestNeighboursBruteForce.Length; i++)
            {
                Assert.AreEqual(nearestNeighboursBruteForce[i], nearestNeighboursKDTree[i]);
            }
        }


        [TestMethod]
        public void Test_Solve_With_Brute_Force_Matches_Solve_With_Segmentation()
        {
            IConfiguration Config = new ConfigurationBuilder()
                    .AddJsonFile("appSettings.json")
                    .Build();

            string dataFileName = Config.GetSection("DataFilePath").Value;
            var pointsToQuery = Config.GetSection("PointsToQuery").Get<Coordinate[]>();

            var nearestNeighboursBruteForce = SolveWithBruteForce.Solve(pointsToQuery, dataFileName);
            var nearestNeighboursSegmentation = SolveWithSegmentation.Solve(pointsToQuery, dataFileName);

            Assert.AreEqual(nearestNeighboursBruteForce.Length, nearestNeighboursSegmentation.Length);

            for (int i = 0; i < nearestNeighboursBruteForce.Length; i++)
            {
                Assert.AreEqual(nearestNeighboursBruteForce[i], nearestNeighboursSegmentation[i]);
            }
        }
    }
}