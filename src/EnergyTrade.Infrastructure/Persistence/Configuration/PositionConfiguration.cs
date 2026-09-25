using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyTrade.Infrastructure.Persistence.Configuration;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("POSITIONS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .HasColumnType("RAW(16)");

        builder.Property(x => x.PortfolioId)
            .HasColumnName("PORTFOLIO_ID")
            .HasColumnType("RAW(16)")
            .IsRequired();

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.EnergyType)
            .HasColumnName("ENERGY_TYPE")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.QuantityMWh)
            .HasColumnName("QUANTITY_MWH")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UPDATED_AT")
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.EnergyType
        })
        .IsUnique();
    }
}