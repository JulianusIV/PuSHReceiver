using Contracts.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Repositories;

namespace RepositoriesTests
{
    [TestClass]
    public class LeaseRepositoryTests
    {
        [TestMethod]
        public void FindLease_Id_ReturnsMockData()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(CreateMockContext());
            var result = service.FindLease(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("AAA", result.DisplayName);
            Assert.AreEqual("A1", result.TopicUrl);
        }

        [TestMethod]
        public void GetActiveExpiredLeases_Nothing_ReturnsLeases()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(CreateMockContext());
            var result = service.GetActiveExpiredLeases();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(x => x.Id == 3));
            Assert.IsTrue(result.Any(x => x.Id == 4));
            Assert.IsTrue(result.Any(x => x.Id == 5));
        }

        [TestMethod]
        public void GetRunningLeases_Nothing_ReturnsLeases()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(CreateMockContext());
            var result = service.GetRunningLeases();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(x => x.Id == 1));
            Assert.IsTrue(result.Any(x => x.Id == 6));
        }

        [TestMethod]
        public void GetSubscribedLeases_Nothing_ReturnsLeases()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(CreateMockContext());
            var result = service.GetSubscribedLeases();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());
            Assert.IsTrue(result.Any(x => x.Id == 1));
            Assert.IsTrue(result.Any(x => x.Id == 3));
            Assert.IsTrue(result.Any(x => x.Id == 5));
            Assert.IsTrue(result.Any(x => x.Id == 6));
        }

        [TestMethod]
        public void CountSubscribedLeases_Nothing_ReturnsInt()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(CreateMockContext());
            var result = service.CountSubscribedLeases();

            Assert.AreEqual(4, result);
        }

        private static IDbContext CreateMockContext()
        {
            //arrange data
            var data = new List<Lease>
            {
                new Lease("AAA", "A1") { Id= 1, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true },
                new Lease("BBB", "B2") { Id= 2, Active = false, LastLease = DateTime.MinValue, LeaseTime = 0, Subscribed = false },
                new Lease("CCC", "C3") { Id= 3, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(7), LeaseTime = 432000, Subscribed = true },
                new Lease("DDD", "D4") { Id= 4, Active = true, LastLease = DateTime.MinValue, LeaseTime = 0, Subscribed = false },
                new Lease("EEE", "E5") { Id= 5, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(7), LeaseTime = 432000, Subscribed = true },
                new Lease("FFF", "F6") { Id= 6, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true },
            }.AsQueryable();

            //create a mock dbset
            var mockSet = new Mock<DbSet<Lease>>();
            mockSet.As<IQueryable<Lease>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            //create mock context
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(c => c.Leases).Returns(mockSet.Object);
            return mockContext.Object;
        }
    }
}