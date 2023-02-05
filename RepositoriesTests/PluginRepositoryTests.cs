using Contracts.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Repositories;

namespace RepositoriesTests
{
    [TestClass]
    public class PluginRepositoryTests
    {
        [TestMethod]
        public void SaveData_Nothing_CallsSaveChanges()
        {
            var lease = new Lease("Test", "MoreTest")
            { Id = 1, Active = true, LastLease = DateTime.Now - TimeSpan.FromDays(1), LeaseTime = 432000, Subscribed = true };

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

            //create the service and inject the mock context
            var service = new PluginRepository(mockContext.Object);
            service.SaveData(lease);

            //assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
            var testLease = data.First();
            Assert.AreEqual(testLease.DisplayName, lease.DisplayName);
            Assert.AreEqual(testLease.TopicUrl, lease.TopicUrl);
        }
    }
}
