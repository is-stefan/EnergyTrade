using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyTrade.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("ORDERS");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(order => order.BuyerId)
            .HasColumnName("BUYER_ID")
            .IsRequired();

        builder.Property(x => x.PortfolioId)
            .HasColumnName("PORTFOLIO_ID")
            .HasColumnType("RAW(16)")
            .IsRequired();

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(order => order.EnergyType)
            .HasColumnName("ENERGY_TYPE")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.QuantityMWh)
            .HasColumnName("QUANTITY_MWH")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(order => order.MaxPricePerMWh)
            .HasColumnName("MAX_PRICE_PER_MWH")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.Currency)
            .HasColumnName("CURRENCY")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.DeliveryStart)
            .HasColumnName("DELIVERY_START")
            .IsRequired();

        builder.Property(order => order.DeliveryEnd)
            .HasColumnName("DELIVERY_END")
            .IsRequired();

        builder.Property(order => order.Status)
            .HasColumnName("STATUS")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(order => order.UpdatedAt)
            .HasColumnName("UPDATED_AT");
    }
}