using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyTrade.Infrastructure.Persistence.Configurations;

public sealed class EnergyOfferConfiguration
    : IEntityTypeConfiguration<EnergyOffer>
{
    public void Configure(EntityTypeBuilder<EnergyOffer> builder)
    {
        builder.ToTable("ENERGY_OFFERS");

        builder.HasKey(offer => offer.Id);

        builder.Property(offer => offer.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(offer => offer.SellerId)
            .HasColumnName("SELLER_ID")
            .IsRequired();

        builder.Property(x => x.PortfolioId)
            .HasColumnName("PORTFOLIO_ID")
            .HasColumnType("RAW(16)")
            .IsRequired();

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(offer => offer.EnergyType)
            .HasColumnName("ENERGY_TYPE")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(offer => offer.QuantityMWh)
            .HasColumnName("QUANTITY_MWH")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(offer => offer.PricePerMWh)
            .HasColumnName("PRICE_PER_MWH")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(offer => offer.Currency)
            .HasColumnName("CURRENCY")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(offer => offer.DeliveryStart)
            .HasColumnName("DELIVERY_START")
            .IsRequired();

        builder.Property(offer => offer.DeliveryEnd)
            .HasColumnName("DELIVERY_END")
            .IsRequired();

        builder.Property(offer => offer.Status)
            .HasColumnName("STATUS")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(offer => offer.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(offer => offer.UpdatedAt)
            .HasColumnName("UPDATED_AT");
    }
}