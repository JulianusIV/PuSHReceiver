using Contracts.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Repositories;
using System.Security.Claims;

namespace RepositoriesTests
{
    [TestClass]
    public class UserRepositoryTests
    {
        Mock<IDbContext>? _mockContext;
        List<User>? _data;

        [TestInitialize]
        public void TestInitialize()
        {
            _data = new List<User>
            {
                new User("AAA", "5F23E4F71C3C727AB02B49793EF10A9F2FBD98B62562D658AB585CB399BE23DB:87513C55EB8463A4FFC056E06F612013:100000:SHA256") { Id= 1 },
                new User("BBB", "") { Id= 2 },
                new User("CCC", "") { Id= 3 },
                new User("DDD", "") { Id= 4 },
                new User("EEE", "") { Id= 5 },
                new User("FFF", "") { Id= 6 },
            };

            var roles = new List<Role>
            {
                new Role("Admin") { Id = 1 },
                new Role("Test") { Id = 2 }
            };

            var userQueryable = _data.AsQueryable();
            //create a mock dbset
            var userMockSet = new Mock<DbSet<User>>();
            userMockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userQueryable.Provider);
            userMockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userQueryable.Expression);
            userMockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userQueryable.ElementType);
            userMockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userQueryable.GetEnumerator);
            userMockSet.Setup(d => d.Add(It.IsAny<User>())).Callback<User>(_data.Add);
            userMockSet.Setup(d => d.Remove(It.IsAny<User>())).Callback<User>((s) => _data.Remove(s));

            var roleQueryable = roles.AsQueryable();
            var roleMockSet = new Mock<DbSet<Role>>();
            roleMockSet.As<IQueryable<Role>>().Setup(m => m.Provider).Returns(roleQueryable.Provider);
            roleMockSet.As<IQueryable<Role>>().Setup(m => m.Expression).Returns(roleQueryable.Expression);
            roleMockSet.As<IQueryable<Role>>().Setup(m => m.ElementType).Returns(roleQueryable.ElementType);
            roleMockSet.As<IQueryable<Role>>().Setup(m => m.GetEnumerator()).Returns(roleQueryable.GetEnumerator);

            //create mock context
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(c => c.Users).Returns(userMockSet.Object);
            mockContext.Setup(c => c.Roles).Returns(roleMockSet.Object);
            _mockContext = mockContext;
        }

        [TestMethod]
        public void GetUser_Int_ReturnsUser()
        {
            var service = new UserRepository(_mockContext!.Object);
            var result = service.GetUser(1);

            Assert.AreEqual(result.Id, _data!.First().Id);
            Assert.AreEqual(result.UserName, _data!.First().UserName);
            Assert.AreEqual(result.PasswordHash, _data!.First().PasswordHash);
        }

        [TestMethod]
        [DataRow("AAA", "test", true)]
        [DataRow("BBB", "test", false)]
        [DataRow("AAA", "testing", false)]
        public void GetUserClaims_LoginData_ReturnsClaimsPrincipal(string name, string password, bool expected)
        {
            var service = new UserRepository(_mockContext!.Object);
            var claims = service.GetUserClaims(name, password);

            if (expected)
            {
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims!.HasClaim(ClaimTypes.Name, name));
            }
            else
            {
                Assert.IsNull(claims);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CreateUser_UserData_CreatesUser(bool addRoles)
        {
            var service = new UserRepository(_mockContext!.Object);
            service.CreateUser("Testuser", "test", addRoles ? new List<Role>() { new("Admin") { Id = 1 } } : null);

            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
            Assert.IsTrue(_data!.First(x => x.UserName == "Testuser").PasswordHash.EndsWith(":100000:SHA256"));
            if (addRoles)
                Assert.IsTrue(_data!.First(x => x.UserName == "Testuser").Roles.Any());
        }
    }
}
