using EnergyTrade.Domain.Enums;

namespace EnergyTrade.Domain.Entities
{
    public class EnergyOffer
    {
        public Guid Id { get; private set; }

        public Guid SellerId { get; private set; }

        public Guid PortfolioId { get; private set; }

        public EnergyType EnergyType { get; private set; }

        public decimal QuantityMWh { get; private set; }

        public decimal PricePerMWh { get; private set; }

        public Currency Currency { get; private set; }

        public DateTimeOffset DeliveryStart { get; private set; }

        public DateTimeOffset DeliveryEnd { get; private set; }

        public OfferStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        private EnergyOffer()
        {
            
        }

        public EnergyOffer(
        Guid sellerId,
        Guid portfolioId,
        EnergyType energyType,
        decimal quantityMWh,
        decimal pricePerMWh,
        Currency currency, 
        DateTimeOffset deliveryStart,
        DateTimeOffset deliveryEnd)

        {
            if(sellerId == Guid.Empty)
            {
                throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
            }

            if(portfolioId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Portfolio id cannot be empty.",
                    nameof(portfolioId));
                
            }

            if(quantityMWh <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantityMWh),
                    "QuantityMWh must be greater than zero."
                );
            }

            if (pricePerMWh <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pricePerMWh),
                    "PricePerMWh must be greater than zero."
                );
            }

            if (deliveryEnd <= deliveryStart)
            {
                throw new ArgumentException(
                    "DeliveryEnd must be after DeliveryStart."
                );
            }

            Id = Guid.NewGuid();
            SellerId = sellerId;
            PortfolioId = portfolioId;
            EnergyType = energyType;
            QuantityMWh = quantityMWh;
            PricePerMWh = pricePerMWh;
            Currency = currency;
            DeliveryStart = deliveryStart;
            DeliveryEnd = deliveryEnd;

            Status = OfferStatus.Open;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void Update(
            EnergyType energyType,
            decimal quantityMWh,
            decimal priceperMWh,
            Currency currency,
            DateTimeOffset deliveryStart,
            DateTimeOffset deliveryEnd)
        {
            EnsureOpen();

            if (quantityMWh <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantityMWh),
                    "QuantityMWh must be greater than zero."
                );
            }

            if (priceperMWh <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(priceperMWh),
                    "PricePerMWh must be greater than zero."
                );
            }

            if (deliveryEnd <= deliveryStart)
            {
                throw new ArgumentException(
                    "DeliveryEnd must be after delivery start."
                );
            }

            EnergyType = energyType;
            QuantityMWh = quantityMWh;
            PricePerMWh = priceperMWh;
            Currency = currency;
            DeliveryStart = deliveryStart;
            DeliveryEnd = deliveryEnd;

            UpdatedAt = DateTimeOffset.UtcNow;
        }
    
    
        public void Close()
        {
            EnsureOpen();

            Status = OfferStatus.Closed;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    
        public void Cancel()
        {
            EnsureOpen();

            Status = OfferStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    
        private void EnsureOpen()
        {
            if (Status != OfferStatus.Open)
            {
                throw new InvalidOperationException(
                    "Only open offers can be modified"
                );
            }
        }
    }
}