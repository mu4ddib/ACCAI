using System;
using System.Linq;
using ACCAI.Domain.Entities;
using ACCAI.Infrastructure.DataSource.ModelConfig;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Test.Api.ACCAI.Infrastructure.ModelConfig
{
    [TestFixture]
    public class ContratoConfigTests
    {
        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options) { }

            public DbSet<Contrato> Contratos { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Aplica la configuración real
                new ContratoConfig().Configure(modelBuilder.Entity<Contrato>());
            }
        }

        [Test]
        public void Should_Configure_Table_And_Key_Correctly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new TestDbContext(options);
            context.Database.EnsureCreated();
            var entity = context.Model.FindEntityType(typeof(Contrato));

            // Assert
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity!.GetTableName(), Is.EqualTo("Contrato"));

            var key = entity.FindPrimaryKey();
            Assert.That(key, Is.Not.Null);
            Assert.That(key!.Properties.Single().Name, Is.EqualTo("Id"));
        }

        [Test]
        public void Should_Seed_Data_Correctly()
        {
            // Arrange
            var builder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());
            var contratoBuilder = builder.Entity<Contrato>();

            // Act
            new ContratoConfig().Configure(contratoBuilder);

            // Assert
            var metadata = contratoBuilder.Metadata;
            var seeds = metadata.GetSeedData().ToList();
            Assert.That(seeds, Has.Count.EqualTo(2));
        }


        [Test]
        public void Should_Call_Private_Seed_Method_Via_Configure()
        {
            // Arrange
            var builder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());
            var entityBuilder = builder.Entity<Contrato>();

            // Act
            var config = new ContratoConfig();
            config.Configure(entityBuilder);

            // Assert
            var data = entityBuilder.Metadata.GetSeedData().ToList();
            Assert.That(data.Count, Is.EqualTo(2));
            Assert.That(data.Any(d => d["Id"]?.Equals(1) == true), Is.True);
        }
    }
}
