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
        Mock<IDbContext>? _mockContext;
        List<Lease>? _data;

        [TestInitialize]
        public void TestInitialize()
        {
            var user = new User("Testuser", "") { Id = 1 };

            _data = new List<Lease>
            {
                new Lease("AAA", "A1") { Id= 1, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true },
                new Lease("BBB", "B2") { Id= 2, Active = false, LastLease = DateTime.MinValue, LeaseTime = 0, Subscribed = false, Owner = user },
                new Lease("CCC", "C3") { Id= 3, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(7), LeaseTime = 432000, Subscribed = true },
                new Lease("DDD", "D4") { Id= 4, Active = true, LastLease = DateTime.MinValue, LeaseTime = 0, Subscribed = false, Owner = user },
                new Lease("EEE", "E5") { Id= 5, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(7), LeaseTime = 432000, Subscribed = true },
                new Lease("FFF", "F6") { Id= 6, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true },
            };
            var queryable = _data.AsQueryable();
            //create a mock dbset
            var mockSet = new Mock<DbSet<Lease>>();
            mockSet.As<IQueryable<Lease>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Lease>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator);
            mockSet.Setup(d => d.Add(It.IsAny<Lease>())).Callback<Lease>(_data.Add);
            mockSet.Setup(d => d.Remove(It.IsAny<Lease>())).Callback<Lease>((s) => _data.Remove(s));

            //create mock context
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(c => c.Leases).Returns(mockSet.Object);
            _mockContext = mockContext;
        }

        [TestMethod]
        public void FindLease_Id_ReturnsMockData()
        {
            //create service and inject mocked context
            var service = new LeaseRepository(_mockContext!.Object);
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
            var service = new LeaseRepository(_mockContext!.Object);
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
            var service = new LeaseRepository(_mockContext!.Object);
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
            var service = new LeaseRepository(_mockContext!.Object);
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
            var service = new LeaseRepository(_mockContext!.Object);
            var result = service.CountSubscribedLeases();

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void GetLeasesByUser_User_ReturnsLeases()
        {
            var owner = new User("Testuser", "") { Id = 1 };

            var service = new LeaseRepository(_mockContext!.Object);
            var result = service.GetLeasesByUser(owner);

            Assert.IsTrue(result.Count() == 2);
            Assert.IsTrue(result.All(x => x.Id == 2 || x.Id == 4));
        }

        [TestMethod]
        public void CreateLease_Lease_CreatesLease()
        {
            var lease = new Lease("TestLease", "someUrl") { Id = 10 };

            var service = new LeaseRepository(_mockContext!.Object);
            service.CreateLease(lease);

            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
            var testLease = _data!.First(x => x.Id == 10);
            Assert.AreEqual(testLease.DisplayName, lease.DisplayName);
            Assert.AreEqual(testLease.TopicUrl, lease.TopicUrl);
        }

        [TestMethod]
        public void UpdateLease_Lease_UpdatesLease()
        {
            var lease = new Lease("TestLease", "someUrl")
            { Id = 1, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true };

            var service = new LeaseRepository(_mockContext!.Object);
            service.UpdateLease(lease);

            //assert
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
            var testLease = _data!.First();
            Assert.AreEqual(testLease.DisplayName, lease.DisplayName);
            Assert.AreEqual(testLease.TopicUrl, lease.TopicUrl);
        }

        [TestMethod]
        public void DeleteLease_Int_DeletesLease()
        {
            var service = new LeaseRepository(_mockContext!.Object);
            service.DeleteLease(1);

            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
            Assert.IsFalse(_data!.Any(x => x.Id == 1));
        }
    }
}