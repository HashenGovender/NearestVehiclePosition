using NearestVehiclePosition;
using static System.Net.Mime.MediaTypeNames;

namespace NearestVehiclePositionTests
{
    [TestClass]
    public class NearestVehicleTests
    {
        const string dataFileName = "VehiclePositions.dat";

        [TestMethod]
        public void Test_Solve_With_Brute_Force_Matches_Solve_With_KDTree()
        {

            var nearestNeighboursBruteForce = SolveWithBruteForce.Solve(PointsToQuery.Points, dataFileName);
            var nearestNeighboursKDTree = SolveWithKDTree.Solve(PointsToQuery.Points, dataFileName);

            Assert.AreEqual(nearestNeighboursBruteForce.Length, nearestNeighboursKDTree.Length);

            for (int i=0; i<nearestNeighboursBruteForce.Length; i++)
            {
                Assert.AreEqual(nearestNeighboursBruteForce[i].Value.PositionId, nearestNeighboursKDTree[i].Value.PositionId);
                Assert.AreEqual(nearestNeighboursBruteForce[i], nearestNeighboursKDTree[i]);
            }
            
            
        }
    }
}