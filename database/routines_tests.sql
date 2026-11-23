-- =============================================================
-- Mi Coffee - triggers, functions and procedures
-- Run order: schema.sql, routines_tests.sql, seed.sql, queries_views.sql
--
-- Triggers only validate as a backup of Business; they never write
-- data, so the application and the database do not run the same
-- operation twice. The status history is written only by
-- sp_change_order_status.
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
-- Trigger: an order can only move to an allowed status.
-- Event:     BEFORE UPDATE on customer_order
-- Condition: the status changes and the new status is not allowed from
--            the old one. Allowed: Received -> InPreparation -> Ready ->
--            Delivered, and Cancelled from Received, InPreparation or
--            Ready. Delivered and Cancelled are final.
-- Effect:    the update is rejected with error 1644 and, inside
--            sp_change_order_status, the transaction is rolled back.
-- Updates that do not change the status (for example the seed total
-- update) pass. The trigger does not write order_status_history: only
-- sp_change_order_status writes it, in the same transaction as the update.
-- -------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_customer_order_status_transition;

DELIMITER $$

CREATE TRIGGER trg_customer_order_status_transition
BEFORE UPDATE ON customer_order
FOR EACH ROW
BEGIN
    IF NEW.status <> OLD.status AND NOT (
        (OLD.status = 'Received'      AND NEW.status IN ('InPreparation', 'Cancelled')) OR
        (OLD.status = 'InPreparation' AND NEW.status IN ('Ready', 'Cancelled')) OR
        (OLD.status = 'Ready'         AND NEW.status IN ('Delivered', 'Cancelled'))
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Status transition not allowed';
    END IF;
END$$

DELIMITER ;

-- -------------------------------------------------------------
-- Function: total of an order calculated from its items and options.
-- Formula:   sum over the items of (unit_price + sum of the option
--            extra_price) * quantity, the same formula Business uses
--            in OrderService.
-- Returns:   0.00 when the order has no items or does not exist.
-- Use:       checks the stored customer_order.total. The orders whose
--            total does not match their lines are:
--            SELECT customer_order_id FROM customer_order
--            WHERE total <> fn_order_total(customer_order_id);
-- -------------------------------------------------------------
DROP FUNCTION IF EXISTS fn_order_total;

DELIMITER $$

CREATE FUNCTION fn_order_total(p_customer_order_id INT)
RETURNS DECIMAL(10,2)
READS SQL DATA
BEGIN
    DECLARE v_total DECIMAL(10,2);

    SELECT SUM((oi.unit_price + IFNULL(x.extras, 0)) * oi.quantity)
    INTO v_total
    FROM order_item oi
    LEFT JOIN (
        SELECT order_item_id, SUM(extra_price) AS extras
        FROM order_item_option
        GROUP BY order_item_id
    ) x ON x.order_item_id = oi.order_item_id
    WHERE oi.customer_order_id = p_customer_order_id;

    RETURN IFNULL(v_total, 0.00);
END$$

DELIMITER ;

-- -------------------------------------------------------------
-- Procedure: changes the status of an order and records who changed it.
-- Parameters: p_customer_order_id  order to change
--             p_previous_status    status the caller read; the change
--                                  only applies while the order still
--                                  has it, so two simultaneous changes
--                                  cannot overwrite each other
--             p_new_status         status to move to
--             p_user_id            staff user who makes the change
-- Effect:     UPDATE customer_order and INSERT INTO order_status_history
--             in one transaction, both or none. This is the only code
--             that writes order_status_history; the triggers never do.
-- Errors:     SIGNAL SQLSTATE '45000' (error 1644) with one of
--               'Order not found'
--               'Order status changed'
--               'Invalid status'
--               'New status equals current status'
--             'Status transition not allowed' comes from
--             trg_customer_order_status_transition, which fires on the
--             UPDATE: the transition rule lives only in that trigger.
--             Any other error (for example a user that does not exist,
--             error 1452) is raised unchanged. In every error the
--             transaction is rolled back.
-- The procedure starts its own transaction: do not call it inside
-- another one, because START TRANSACTION would commit that one first.
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_change_order_status;

DELIMITER $$

CREATE PROCEDURE sp_change_order_status(
    IN p_customer_order_id INT,
    IN p_previous_status   VARCHAR(20),
    IN p_new_status        VARCHAR(20),
    IN p_user_id           INT
)
BEGIN
    DECLARE v_current_status VARCHAR(20);

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    -- The row stays locked until COMMIT or ROLLBACK.
    SELECT status
    INTO v_current_status
    FROM customer_order
    WHERE customer_order_id = p_customer_order_id
    FOR UPDATE;

    IF v_current_status IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Order not found';
    END IF;

    IF NOT (v_current_status <=> p_previous_status) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Order status changed';
    END IF;

    IF p_new_status IS NULL OR p_new_status NOT IN
        ('Received', 'InPreparation', 'Ready', 'Delivered', 'Cancelled') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid status';
    END IF;

    IF p_new_status = v_current_status THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'New status equals current status';
    END IF;

    UPDATE customer_order
    SET status = p_new_status
    WHERE customer_order_id = p_customer_order_id;

    INSERT INTO order_status_history
        (customer_order_id, user_id, previous_status, new_status)
    VALUES
        (p_customer_order_id, p_user_id, v_current_status, p_new_status);

    COMMIT;
END$$

DELIMITER ;

-- -------------------------------------------------------------
-- Procedure: registers a stock entry (merchandise received).
-- Parameters: p_supply_id  supply that receives the entry
--             p_quantity   amount to add, greater than 0
-- Effect:     current_stock = current_stock + p_quantity, in a single
--             UPDATE, so two simultaneous entries do not overwrite
--             each other.
-- Errors:     SIGNAL SQLSTATE '45000' (error 1644) with one of
--               'Quantity must be greater than 0'
--               'Supply not found'
--               'Stock exceeds the maximum'
--             Nothing is changed when an error is raised.
-- 99999999.99 is the limit of the DECIMAL(10,2) column.
-- -------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_add_stock;

DELIMITER $$

CREATE PROCEDURE sp_add_stock(
    IN p_supply_id INT,
    IN p_quantity  DECIMAL(10,2)
)
BEGIN
    IF p_quantity IS NULL OR p_quantity <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Quantity must be greater than 0';
    END IF;

    UPDATE supply
    SET current_stock = current_stock + p_quantity
    WHERE supply_id = p_supply_id
      AND current_stock + p_quantity <= 99999999.99;

    IF ROW_COUNT() = 0 THEN
        IF EXISTS (SELECT 1 FROM supply WHERE supply_id = p_supply_id) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock exceeds the maximum';
        ELSE
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Supply not found';
        END IF;
    END IF;
END$$

DELIMITER ;

-- -------------------------------------------------------------
-- Tests (run after seed.sql). They are commented out so the script
-- runs without stopping; select one statement without the leading
-- "-- " and run only the selection.
-- -------------------------------------------------------------
-- TRG-03 Invalid: Frappé de matcha (product 8) is not available (error 1644)
-- INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price) VALUES (12, 8, 1, 60.00);

-- TRG-04 Valid: Té verde (product 10) is available; the row is inserted and then removed
-- INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price) VALUES (12, 10, 1, 32.00);
-- DELETE FROM order_item WHERE customer_order_id = 12 AND product_id = 10;

-- TRG-01 Invalid: order 1 is Delivered, a final status (error 1644)
-- UPDATE customer_order SET status = 'Received' WHERE customer_order_id = 1;

-- Invalid: order 11 is Received, it cannot skip InPreparation (error 1644)
-- UPDATE customer_order SET status = 'Ready' WHERE customer_order_id = 11;

-- TRG-02 Valid: order 11 is Received; the change is rolled back so the seed stays intact
-- START TRANSACTION;
-- UPDATE customer_order SET status = 'InPreparation' WHERE customer_order_id = 11;
-- ROLLBACK;

-- -------------------------------------------------------------
-- Function and procedure tests (run after seed.sql, in this order).
-- Same convention as above: select a test without the leading
-- "-- " and run only the selection. The valid procedure tests
-- (SP-01, SP-09) commit their changes, so to repeat them run
-- schema.sql, routines_tests.sql and seed.sql again first.
-- Every procedure error is reported as error 1644 unless another
-- number is given.
-- -------------------------------------------------------------
-- FN-01 Valid: order 8 (2 Latte with Mediano and Jarabe de vainilla, 1 Pay de queso)
-- Expected: stored total and function result are 167.00
-- SELECT customer_order_id, total, fn_order_total(customer_order_id) AS calculated FROM customer_order WHERE customer_order_id = 8;

-- FN-02 Valid: every stored total matches its lines
-- Expected: 0 rows
-- SELECT customer_order_id, total, fn_order_total(customer_order_id) AS calculated FROM customer_order WHERE total <> fn_order_total(customer_order_id);

-- FN-03 Boundary: an order that does not exist has no lines
-- Expected: 0.00
-- SELECT fn_order_total(999) AS calculated;

-- SP-01 Valid: order 10 (InPreparation) moves to Ready by user 3
-- Expected: order 10 is Ready and the history has the new row (InPreparation -> Ready, user 3)
-- CALL sp_change_order_status(10, 'InPreparation', 'Ready', 3);
-- SELECT customer_order_id, status FROM customer_order WHERE customer_order_id = 10;
-- SELECT customer_order_id, user_id, previous_status, new_status FROM order_status_history WHERE customer_order_id = 10 ORDER BY order_status_history_id;

-- SP-02 Invalid: the order does not exist
-- Expected: error 1644 'Order not found'
-- CALL sp_change_order_status(999, 'Received', 'InPreparation', 3);

-- SP-03 Invalid: the caller read an old status (order 11 is Received, not InPreparation)
-- Expected: error 1644 'Order status changed'
-- CALL sp_change_order_status(11, 'InPreparation', 'Ready', 3);

-- SP-04 Invalid: the new status is not one of the five defined
-- Expected: error 1644 'Invalid status'
-- CALL sp_change_order_status(11, 'Received', 'Lost', 3);

-- SP-05 Invalid: the new status equals the current one
-- Expected: error 1644 'New status equals current status'
-- CALL sp_change_order_status(11, 'Received', 'Received', 3);

-- SP-06 Invalid: Received cannot skip InPreparation (rejected by the trigger)
-- Expected: error 1644 'Status transition not allowed'; order 11 stays Received and gets no history row
-- CALL sp_change_order_status(11, 'Received', 'Ready', 3);

-- SP-07 Invalid: user 999 does not exist, so the history insert fails and the UPDATE is undone
-- Expected: error 1452; order 12 stays Received
-- CALL sp_change_order_status(12, 'Received', 'InPreparation', 999);

-- SP-08 Invalid: Delivered is a final status (rejected by the trigger)
-- Expected: error 1644 'Status transition not allowed'
-- CALL sp_change_order_status(1, 'Delivered', 'Received', 3);

-- SP-09 Valid: entry of 1.00 for Té chai en polvo (0.40, minimum 0.50)
-- Expected: stock 1.40 and in_alert = 0
-- CALL sp_add_stock(3, 1.00);
-- SELECT name, current_stock, minimum_stock, current_stock <= minimum_stock AS in_alert FROM supply WHERE supply_id = 3;

-- SP-10 Invalid: quantity 0
-- Expected: error 1644 'Quantity must be greater than 0'
-- CALL sp_add_stock(3, 0);

-- SP-11 Invalid: negative quantity
-- Expected: error 1644 'Quantity must be greater than 0'
-- CALL sp_add_stock(3, -5.00);

-- SP-12 Invalid: the supply does not exist
-- Expected: error 1644 'Supply not found'
-- CALL sp_add_stock(999, 1.00);

-- SP-13 Invalid: the resulting stock would exceed 99,999,999.99
-- Expected: error 1644 'Stock exceeds the maximum'; stock of supply 1 stays 8.50
-- CALL sp_add_stock(1, 99999999.99);
