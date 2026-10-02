using EnergyTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnergyTrade.Infrastructure.Persistence.Configuration;

public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(
        EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IDEMPOTENCY_RECORDS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .HasColumnType("RAW(16)")
            .IsRequired();

        builder.Property(x => x.Key)
            .HasColumnName("KEY")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Operation)
            .HasColumnName("OPERATION")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasColumnName("REQUEST_HASH")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Response)
            .HasColumnName("RESPONSE")
            .HasColumnType("CLOB")
            .IsRequired();

        builder.Property(x => x.StatusCode)
            .HasColumnName("STATUS_CODE")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.HasIndex(x => new
            {
                x.Key,
                x.Operation
            })
            .IsUnique();
    }
}