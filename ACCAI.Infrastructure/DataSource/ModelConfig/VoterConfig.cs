using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ACCAI.Domain.Entities;
namespace ACCAI.Infrastructure.DataSource.ModelConfig;
public sealed class VoterConfig : IEntityTypeConfiguration<Voter>
{
    public void Configure(EntityTypeBuilder<Voter> b)
    {
        b.ToTable("Voters");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nid).HasMaxLength(50).IsRequired();
        b.Property(x => x.Origin).HasMaxLength(80).IsRequired();
        b.Property(x => x.DateOfBirth).IsRequired();
        b.Property<DateTime>("CreatedOn");
        b.Property<DateTime>("LastModifiedOn");
        b.HasIndex(x => x.Nid).IsUnique();
    }
}
