using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Collections.Generic;
using Newtonsoft.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AspNetCoreDemo.NpgsqlEfCoreTest
{
    public class DbContextTest
    {
        public IServiceCollection Services { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ITestLogger OutputHelper { get; private set; }

        public DbContextTest(ITestOutputHelper outputHelper)
        {
            this.OutputHelper = new ConsoleTestLogger();

            this.OutputHelper.WriteLine($"AppContext.BaseDirectory:{AppContext.BaseDirectory}");
            Services = new ServiceCollection();
            Services.AddSingleton<IHostingEnvironment, TestHostingEnvironment>();
            Services.AddSingleton<ITestLogger, ConsoleTestLogger>();

            IHostingEnvironment env = Services.BuildServiceProvider().GetService<IHostingEnvironment>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            // Added - uses IOptions<T> for your settings.
            Services.AddOptions();

            // Added - Confirms that we have a home for our DemoSettings
            Services.Configure<DbSettings>(Configuration.GetSection("DbSettings"));

            Services.AddDbContext<TestDbContext>((serviceProvider, dbContextBuilder) =>
            {
                var dbSettings = serviceProvider.GetService<IOptions<DbSettings>>().Value;
                dbContextBuilder.UseNpgsql(dbSettings.ConnectionString);
            });

            Services.AddSingleton<DbContextOptions<TestDbContext>>(provider =>
            {
                var dbSettings = provider.GetService<IOptions<DbSettings>>().Value;
                return new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(dbSettings.ConnectionString).Options;
            });

            //Services.AddDependencyRegister();
            this.ServiceProvider = this.Services.BuildServiceProvider();
        }

        [Fact]
        public void DbConnectionSettingTest()
        {
            var dbOption = ServiceProvider.GetService<IOptions<DbSettings>>().Value;
            Assert.NotNull(dbOption);
            this.OutputHelper.WriteLine($"Connection String: {dbOption.ConnectionString}");
        }

        [Fact]
        public void ServiceCollectionTest()
        {
            Assert.NotNull(this.Services);
        }

        //[Fact]
        //public void FailureTest()
        //{
        //    Assert.Equal(0, 1);
        //}

        [Fact]
        public void DapperReadTest()
        {
            using (var dbContext = ServiceProvider.GetService<TestDbContext>())
            {
                var connection = dbContext.Database.GetDbConnection();
                var typeRecords = connection.Query<TypeTestEntity>("select * from test_pgsqltype");

                Assert.NotNull(typeRecords);

                var now = DateTime.Now;
                var stringArray = new List<string>() { "A", "B", "C" };

                var entity = new TypeTestEntity()
                {
                    SysId = Guid.NewGuid(),
                    arrayType = stringArray.ToArray()
                };

                entity.JsonType = JsonConvert.SerializeObject(entity);

                connection.Execute(@"INSERT INTO test_pgsqltype(SysId,ArrayType,JsonType) VALUES(@sysid,@arraytype,@jsontype::json)", new { entity.SysId, arrayType = entity.arrayType, entity.JsonType });

                typeRecords = connection.Query<TypeTestEntity>("select * from test_pgsqltype where SysId = @SysId", new { entity.SysId });

                Assert.Equal(1, typeRecords.Count());
            }
        }

        [Fact]
        public void NpgsqlEfCoreDataTypeTest()
        {
            this.OutputHelper.WriteLine($"GuestUserAddTest");
            TypeTestEntity newEntity = CreateNewEntity();

            using (var dbContext = ServiceProvider.GetService<TestDbContext>())
            {
                dbContext.Add(newEntity);
                dbContext.SaveChanges();

                var addedEntity = dbContext.TypeTests.SingleOrDefault(entity => entity.SysId == newEntity.SysId);

                Assert.NotNull(addedEntity);
                Assert.True(ReferenceEquals(newEntity, addedEntity));
                Assert.Equal(newEntity.JsonType, addedEntity.JsonType);
                Assert.Equal(newEntity.arrayType, addedEntity.arrayType);
            }

            var dbContextOption = ServiceProvider.GetService<DbContextOptions<TestDbContext>>();

            Assert.NotNull(dbContextOption);

            var updatedArrayValue = new List<string>() { "AAA", "BBB", "CCCC" };
            using (var dbContext = new TestDbContext(dbContextOption))
            {
                var addedEntity = dbContext.TypeTests.SingleOrDefault(entity => entity.SysId == newEntity.SysId);

                Assert.NotNull(addedEntity);
                Assert.False(ReferenceEquals(newEntity, addedEntity));
                Assert.Equal(newEntity.JsonType, addedEntity.JsonType);
                Assert.Equal(newEntity.arrayType, addedEntity.arrayType);

                addedEntity.arrayType = updatedArrayValue.ToArray();

                dbContext.Update(addedEntity);
                dbContext.SaveChanges();
            }


            using (var dbContext = new TestDbContext(dbContextOption))
            {
                var updatedEntity = dbContext.TypeTests.SingleOrDefault(entity => entity.SysId == newEntity.SysId);

                Assert.NotNull(updatedEntity);
                Assert.False(ReferenceEquals(newEntity, updatedEntity));
                Assert.Equal(newEntity.JsonType, updatedEntity.JsonType);
                Assert.Equal(updatedArrayValue, updatedEntity.arrayType);
            }
        }

        private static TypeTestEntity CreateNewEntity()
        {
            var stringArray = new List<string>() { "A", "B", "C" };

            var newEntity = new TypeTestEntity()
            {
                SysId = Guid.NewGuid(),
                arrayType = stringArray.ToArray()
            };
            newEntity.JsonType = JsonConvert.SerializeObject(newEntity);
            return newEntity;
        }
    }

    public interface ITestLogger
    {
        void WriteLine(string msg);
    }

    public class ConsoleTestLogger : ITestLogger
    {
        public void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
