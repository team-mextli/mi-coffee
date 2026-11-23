-- =============================================================
-- Mi Coffee - triggers, functions and procedures
-- Run order: schema.sql, routines_tests.sql, seed.sql, queries_views.sql
--
-- Triggers only validate as a backup of Business; they never write
-- data, so the application and the database do not run the same
-- operation twice.
--
-- Agreed error for every validation trigger:
--   SIGNAL SQLSTATE '45000' without MYSQL_ERRNO, reported as error 1644
--   (MiCoffee.Data.MySqlErrors.IsRejectedByTrigger).
-- =============================================================

USE mi_coffee;

-- -------------------------------------------------------------
-- Trigger: an order line cannot use a product marked as not available.
-- Event:     BEFORE INSERT on order_item
-- Condition: the product exists and is_available = FALSE
-- Effect:    the insert is rejected with error 1644 and, inside
--            OrderRepository.Insert, the whole order is rolled back.
-- A product that does not exist is left to the foreign key (error 1452).
-- -------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_order_item_product_available;

DELIMITER $$

CREATE TRIGGER trg_order_item_product_available
BEFORE INSERT ON order_item
FOR EACH ROW
BEGIN
    IF EXISTS (
        SELECT 1
        FROM product
        WHERE product_id = NEW.product_id
          AND is_available = FALSE
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Product is not available';
    END IF;
END$$

DELIMITER ;

-- -------------------------------------------------------------
-- Tests (run after seed.sql). They are commented out so the script
-- runs without stopping; select one statement without the leading
-- "-- " and run only the selection.
-- -------------------------------------------------------------
-- Invalid: Frappé de matcha (product 8) is not available (error 1644)
-- INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price) VALUES (12, 8, 1, 60.00);

-- Valid: Té verde (product 10) is available; the row is inserted and then removed
-- INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price) VALUES (12, 10, 1, 32.00);
-- DELETE FROM order_item WHERE customer_order_id = 12 AND product_id = 10;
