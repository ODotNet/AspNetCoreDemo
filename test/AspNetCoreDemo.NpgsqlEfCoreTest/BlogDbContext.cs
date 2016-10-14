using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace AspNetCoreDemo.NpgsqlEfCoreTest
{
    public class DbSettings
    {
        public string ConnectionString { get; set; }
    }
    public class TestHostingEnvironment : IHostingEnvironment
    {
        public TestHostingEnvironment()
        {
            this.ApplicationName = "UnitTest Application";
            this.EnvironmentName = "UnitTesting";

            var workDirectory = PlatformServices.Default.Application.ApplicationBasePath;
            this.ContentRootPath = workDirectory.IndexOf($@"{Path.DirectorySeparatorChar}bin") > 0 ? workDirectory.Substring(0, workDirectory.IndexOf($@"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)) : workDirectory;
            this.ContentRootFileProvider = new PhysicalFileProvider(this.ContentRootPath);

            this.WebRootPath = null;
            this.WebRootFileProvider = new NullFileProvider();
        }
        public string ApplicationName { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; }

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; }

        public IFileProvider WebRootFileProvider { get; set; }

        public string WebRootPath { get; set; }
    }
    //docs: https://damienbod.com/2016/01/11/asp-net-5-with-postgresql-and-entity-framework-7/
    // >dotnet ef migration add testMigration in AspNet5MultipleProject
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<TypeTestEntity> TypeTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //builder.Entity<DataEventRecord>().HasKey(m => m.DataEventRecordId);
            //builder.Entity<SourceInfo>().HasKey(m => m.SourceInfoId);

            //// shadow properties
            //builder.Entity<DataEventRecord>().Property<DateTime>("UpdatedTimestamp");
            //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");

            //base.OnModelCreating(builder);

            modelBuilder.Entity<TypeTestEntity>().Property(x => x.BizId).HasDefaultValueSql("NEXTVAL('bizid_test_pgsqltype_seq')");

            modelBuilder.HasPostgresExtension("uuid-ossp");
            //modelBuilder.Entity<TypeTestEntity>().ToTable("typetest");
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            //updateUpdatedProperty<SourceInfo>();
            //updateUpdatedProperty<DataEventRecord>();

            return base.SaveChanges();
        }

        private void updateUpdatedProperty<T>() where T : class
        {
            var modifiedSourceInfo =
                ChangeTracker.Entries<T>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in modifiedSourceInfo)
            {
                entry.Property("UpdatedTimestamp").CurrentValue = DateTime.UtcNow;
            }
        }
    }

    [Table("test_pgsqltype")]
    public class TypeTestEntity
    {
        [Key, Column("sysid")]
        public Guid SysId { get; set; }

        [Column("bizid")]
        public int BizId { get; set; }

        [Column("arraytype")]
        public string[] arrayType { get; set; }

        [Column("jsontype", TypeName = "json")]
        public string JsonType { get; set; }
    }
}
