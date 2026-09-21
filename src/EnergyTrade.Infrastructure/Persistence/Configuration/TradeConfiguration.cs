using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyTrade.Infrastructure.Persistence.Configurations;

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("TRADES");

        builder.HasKey(trade => trade.Id);

        builder.Property(trade => trade.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(trade => trade.EnergyOfferId)
            .HasColumnName("ENERGY_OFFER_ID")
            .IsRequired();

        builder.Property(trade => trade.SellerId)
            .HasColumnName("SELLER_ID")
            .IsRequired();

        builder.Property(trade => trade.BuyerId)
            .HasColumnName("BUYER_ID")
            .IsRequired();

        builder.Property(trade => trade.EnergyType)
            .HasColumnName("ENERGY_TYPE")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(trade => trade.QuantityMWh)
            .HasColumnName("QUANTITY_MWH")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(trade => trade.PricePerMWh)
            .HasColumnName("PRICE_PER_MWH")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(trade => trade.Currency)
            .HasColumnName("CURRENCY")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(trade => trade.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();
    }
}