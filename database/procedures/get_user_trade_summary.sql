CREATE OR REPLACE PROCEDURE GET_USER_TRADE_SUMMARY
(
    p_user_id       IN RAW,
    p_currency      IN NUMBER,
    p_energy_type   IN NUMBER DEFAULT NULL,

    p_trade_count   OUT NUMBER,
    p_total_mwh     OUT NUMBER,
    p_average_price OUT NUMBER,
    p_total_value   OUT NUMBER
)
AS
BEGIN
    SELECT
        COUNT(*),
        NVL(SUM(QUANTITY_MWH), 0),
        NVL(AVG(PRICE_PER_MWH), 0),
        NVL(SUM(QUANTITY_MWH * PRICE_PER_MWH), 0)
    INTO
        p_trade_count,
        p_total_mwh,
        p_average_price,
        p_total_value
    FROM TRADES
    WHERE
        (SELLER_ID = p_user_id OR BUYER_ID = p_user_id)
        AND CURRENCY = p_currency
        AND
        (
            p_energy_type IS NULL
            OR ENERGY_TYPE = p_energy_type
        );
END;
/