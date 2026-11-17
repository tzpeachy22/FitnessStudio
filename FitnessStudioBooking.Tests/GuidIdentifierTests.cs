using api.Data;
using api.Models;
using api.Migrations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace FitnessStudioBooking.Tests;

public sealed class GuidIdentifierTests
{
    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql("Server=localhost;Database=gym_class_booking;User=test;Password=test;",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options);

    [Fact]
    public void Keys_and_seed_relationships_should_use_valid_guids()
    {
        using var db = CreateContext();
        foreach (var entity in db.GetService<IDesignTimeModel>().Model.GetEntityTypes())
        {
            entity.FindPrimaryKey()!.Properties.Single().ClrType.Should().Be(typeof(Guid));
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var property = foreignKey.Properties.Single();
                property.ClrType.Should().Be(typeof(Guid));
                var principalIds = foreignKey.PrincipalEntityType.GetSeedData()
                    .Select(row => row[foreignKey.PrincipalKey.Properties.Single().Name]).ToHashSet();
                foreach (var seed in entity.GetSeedData())
                {
                    principalIds.Should().Contain(seed[property.Name]);
                }
            }
        }
    }

    [Fact]
    public void Adding_new_entities_should_generate_distinct_nonempty_guids()
    {
        using var db = CreateContext();
        var first = new Business { Name = "First" };
        var second = new Business { Name = "Second" };
        db.Businesses.AddRange(first, second);
        first.Id.Should().NotBeEmpty();
        second.Id.Should().NotBeEmpty().And.NotBe(first.Id);
    }

    [Fact]
    public void Guid_migration_should_preserve_rows_and_restore_relationships()
    {
        var migration = new ConvertIdsToGuid { ActiveProvider = "Pomelo.EntityFrameworkCore.MySql" };
        var operations = migration.UpOperations;
        operations.Should().NotContain(operation => operation is DeleteDataOperation || operation is DropTableOperation);
        operations.OfType<DropForeignKeyOperation>().Should().HaveCount(9);
        operations.OfType<AddForeignKeyOperation>().Should().HaveCount(9);
        operations.OfType<SqlOperation>().Count(operation => operation.Sql.StartsWith("UPDATE ")).Should().Be(15);
    }
}
