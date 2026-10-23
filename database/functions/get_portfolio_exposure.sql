CREATE OR REPLACE FUNCTION GET_PORTFOLIO_EXPOSURE
(
    p_portfolio_id IN RAW
)
RETURN NUMBER
AS
    v_exposure NUMBER;
BEGIN
    SELECT
        NVL(SUM(QUANTITY_MWH), 0)
    INTO
        v_exposure
    FROM POSITIONS
    WHERE PORTFOLIO_ID = p_portfolio_id;

    RETURN v_exposure;
END;
/