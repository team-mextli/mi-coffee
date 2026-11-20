-- =============================================================
-- Mi Coffee - queries and views
-- Run after schema.sql and seed.sql.
-- Part 1: ten documented queries (results described for seed.sql).
-- Part 2: CRUD operations with validations.
-- Part 3: views (Frontend).
-- =============================================================

USE mi_coffee;

-- =============================================================
-- Part 1 - Queries
-- =============================================================

-- -------------------------------------------------------------
-- Q1 (JOIN) Receipt of an order: each product with its chosen
-- options and line subtotal. Four tables joined.
-- Expected for order 8: 2 Latte (Mediano, Jarabe de vainilla) = 122.00
-- and 1 Pay de queso = 45.00.
-- -------------------------------------------------------------
SELECT
    co.customer_order_id                                  AS order_number,
    co.created_at,
    p.name                                                AS product,
    oi.quantity,
    oi.unit_price,
    GROUP_CONCAT(po.name ORDER BY po.option_group SEPARATOR ', ') AS options,
    (oi.unit_price + IFNULL(SUM(oio.extra_price), 0)) * oi.quantity AS subtotal
FROM customer_order co
JOIN order_item oi             ON oi.customer_order_id = co.customer_order_id
JOIN product p                 ON p.product_id = oi.product_id
LEFT JOIN order_item_option oio ON oio.order_item_id = oi.order_item_id
LEFT JOIN product_option po     ON po.product_option_id = oio.product_option_id
WHERE co.customer_order_id = 8
GROUP BY co.customer_order_id, co.created_at, oi.order_item_id, p.name, oi.quantity, oi.unit_price
ORDER BY oi.order_item_id;

-- -------------------------------------------------------------
-- Q2 (JOIN) Inventory alerts: supplies at or below their minimum,
-- with category and supplier (LEFT JOIN because the supplier is
-- optional). Sorted by the largest shortage first.
-- Expected: 4 rows; Té chai en polvo shows no supplier.
-- -------------------------------------------------------------
SELECT
    s.name                              AS supply,
    sc.name                             AS category,
    s.current_stock,
    s.minimum_stock,
    s.unit,
    s.minimum_stock - s.current_stock   AS shortage,
    IFNULL(sp.name, 'Sin proveedor')    AS supplier,
    sp.contact
FROM supply s
JOIN supply_category sc ON sc.supply_category_id = s.supply_category_id
LEFT JOIN supplier sp   ON sp.supplier_id = s.supplier_id
WHERE s.current_stock <= s.minimum_stock
ORDER BY shortage DESC;

-- -------------------------------------------------------------
-- Q3 (JOIN) Status history filtered by date range and new status,
-- with the staff member who made each change.
-- Expected for 2026-11-16 to 2026-11-18 and 'Delivered': 6 rows.
-- -------------------------------------------------------------
SELECT
    h.customer_order_id  AS order_number,
    h.previous_status,
    h.new_status,
    h.changed_at,
    u.full_name          AS changed_by,
    u.role
FROM order_status_history h
JOIN user u ON u.user_id = h.user_id
WHERE h.changed_at >= '2026-11-16'
  AND h.changed_at <  '2026-11-19'
  AND h.new_status = 'Delivered'
ORDER BY h.changed_at;

-- -------------------------------------------------------------
-- Q4 (subquery, NOT EXISTS) Products that have never been ordered,
-- useful to review the menu.
-- Expected: 2 rows, Frappé de matcha (not available) and Té verde.
-- -------------------------------------------------------------
SELECT
    p.product_id,
    p.name,
    pc.name       AS category,
    p.is_available
FROM product p
JOIN product_category pc ON pc.product_category_id = p.product_category_id
WHERE NOT EXISTS (
    SELECT 1
    FROM order_item oi
    WHERE oi.product_id = p.product_id
)
ORDER BY pc.name, p.name;

-- -------------------------------------------------------------
-- Q5 (subquery, EXISTS) Suppliers to call today: those that supply
-- at least one item in alert.
-- Expected: 3 suppliers.
-- -------------------------------------------------------------
SELECT
    sp.supplier_id,
    sp.name,
    sp.contact,
    sp.note
FROM supplier sp
WHERE EXISTS (
    SELECT 1
    FROM supply s
    WHERE s.supplier_id = sp.supplier_id
      AND s.current_stock <= s.minimum_stock
)
ORDER BY sp.name;

-- -------------------------------------------------------------
-- Q6 (aggregate, GROUP BY) Daily sales: delivered orders, revenue and
-- average ticket per day. Cancelled and pending orders are excluded.
-- Expected: 3 days, 662.00 in total.
-- -------------------------------------------------------------
SELECT
    DATE(co.created_at)      AS sale_date,
    COUNT(*)                 AS delivered_orders,
    SUM(co.total)            AS revenue,
    ROUND(AVG(co.total), 2)  AS average_ticket
FROM customer_order co
WHERE co.status = 'Delivered'
GROUP BY DATE(co.created_at)
ORDER BY sale_date;

-- -------------------------------------------------------------
-- Q7 (aggregate, GROUP BY + HAVING) Best-selling products: units sold
-- in non-cancelled orders, only products with 2 or more units.
-- Expected: Concha 5, Latte 4, Americano 3, Café de olla 2.
-- -------------------------------------------------------------
SELECT
    p.name               AS product,
    SUM(oi.quantity)     AS units_sold,
    COUNT(DISTINCT oi.customer_order_id) AS orders
FROM order_item oi
JOIN product p         ON p.product_id = oi.product_id
JOIN customer_order co ON co.customer_order_id = oi.customer_order_id
WHERE co.status <> 'Cancelled'
GROUP BY p.product_id, p.name
HAVING SUM(oi.quantity) >= 2
ORDER BY units_sold DESC, product;

-- -------------------------------------------------------------
-- Q8 (aggregate, GROUP BY + HAVING) Most chosen options per group:
-- how many times each option was selected and the extra revenue it
-- produced, only options chosen at least twice.
-- Expected: 5 options; Grande and Mediano were chosen 5 times each.
-- -------------------------------------------------------------
SELECT
    po.option_group,
    po.name                   AS option_name,
    COUNT(*)                  AS times_chosen,
    SUM(oio.extra_price * oi.quantity) AS extra_revenue
FROM order_item_option oio
JOIN product_option po ON po.product_option_id = oio.product_option_id
JOIN order_item oi     ON oi.order_item_id = oio.order_item_id
GROUP BY po.product_option_id, po.option_group, po.name
HAVING COUNT(*) >= 2
ORDER BY po.option_group, times_chosen DESC;

-- -------------------------------------------------------------
-- Q9 (free) Revenue by product category for delivered orders,
-- including the extra price of options. Combines a derived table
-- with four joins and aggregation.
-- Expected: Bebidas calientes first with 360.00; 662.00 in total.
-- -------------------------------------------------------------
SELECT
    pc.name                   AS category,
    SUM(oi.quantity)          AS units,
    SUM((oi.unit_price + IFNULL(x.extras, 0)) * oi.quantity) AS revenue
FROM order_item oi
JOIN customer_order co   ON co.customer_order_id = oi.customer_order_id
JOIN product p           ON p.product_id = oi.product_id
JOIN product_category pc ON pc.product_category_id = p.product_category_id
LEFT JOIN (
    SELECT order_item_id, SUM(extra_price) AS extras
    FROM order_item_option
    GROUP BY order_item_id
) x ON x.order_item_id = oi.order_item_id
WHERE co.status = 'Delivered'
GROUP BY pc.product_category_id, pc.name
ORDER BY revenue DESC;

-- -------------------------------------------------------------
-- Q10 (free) Service time: minutes from confirmation to delivery for
-- each delivered order, and whether it was above the average service
-- time (scalar subquery over the same history).
-- Expected: 6 delivered orders, average 10.5 minutes.
-- -------------------------------------------------------------
SELECT
    co.customer_order_id                                AS order_number,
    co.created_at,
    h.changed_at                                        AS delivered_at,
    TIMESTAMPDIFF(MINUTE, co.created_at, h.changed_at)  AS service_minutes,
    CASE
        WHEN TIMESTAMPDIFF(MINUTE, co.created_at, h.changed_at) > (
            SELECT AVG(TIMESTAMPDIFF(MINUTE, co2.created_at, h2.changed_at))
            FROM customer_order co2
            JOIN order_status_history h2 ON h2.customer_order_id = co2.customer_order_id
            WHERE h2.new_status = 'Delivered'
        ) THEN 'Arriba del promedio'
        ELSE 'En o debajo del promedio'
    END                                                 AS evaluation
FROM customer_order co
JOIN order_status_history h ON h.customer_order_id = co.customer_order_id
WHERE h.new_status = 'Delivered'
ORDER BY service_minutes DESC;

-- =============================================================
-- Part 2 - CRUD with validations
-- Each block works on its own records and leaves the test data as
-- seed.sql created it.
-- =============================================================

-- -------------------------------------------------------------
-- C1 Supplier: INSERT, SELECT, UPDATE and DELETE.
-- -------------------------------------------------------------
INSERT INTO supplier (name, contact, note)
VALUES ('Café Orgánico Valle de Guadalupe', '646 100 0007', 'Proveedor de prueba');

SET @new_supplier_id = LAST_INSERT_ID();

SELECT supplier_id, name, contact, note
FROM supplier
WHERE supplier_id = @new_supplier_id;

UPDATE supplier
SET contact = 'pedidos@cafeorganico.example',
    note    = 'Entrega los lunes'
WHERE supplier_id = @new_supplier_id;

SELECT supplier_id, name, contact, note
FROM supplier
WHERE supplier_id = @new_supplier_id;

-- Validated delete: only removes the supplier when no supply uses it.
DELETE FROM supplier
WHERE supplier_id = @new_supplier_id
  AND NOT EXISTS (SELECT 1 FROM supply s WHERE s.supplier_id = @new_supplier_id);

SELECT ROW_COUNT() AS suppliers_deleted;  -- Expected: 1

-- Same validated delete on a supplier that supplies items: nothing is deleted.
DELETE FROM supplier
WHERE supplier_id = 2
  AND NOT EXISTS (SELECT 1 FROM supply s WHERE s.supplier_id = 2);

SELECT ROW_COUNT() AS suppliers_deleted;  -- Expected: 0

-- -------------------------------------------------------------
-- C2 Product category: the delete only succeeds when the category
-- has no products.
-- -------------------------------------------------------------
INSERT INTO product_category (name) VALUES ('Temporada');
SET @new_category_id = LAST_INSERT_ID();

DELETE FROM product_category
WHERE product_category_id = 1
  AND NOT EXISTS (SELECT 1 FROM product p WHERE p.product_category_id = 1);
SELECT ROW_COUNT() AS categories_deleted; -- Expected: 0 (Bebidas calientes has products)

DELETE FROM product_category
WHERE product_category_id = @new_category_id
  AND NOT EXISTS (SELECT 1 FROM product p WHERE p.product_category_id = @new_category_id);
SELECT ROW_COUNT() AS categories_deleted; -- Expected: 1

-- -------------------------------------------------------------
-- C3 Allowed options: assign an option only if it is not assigned
-- yet, list the result, then remove it.
-- -------------------------------------------------------------
INSERT INTO product_allowed_option (product_id, product_option_id)
SELECT 10, 7
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM product_allowed_option
    WHERE product_id = 10 AND product_option_id = 7
);
SELECT ROW_COUNT() AS options_assigned;   -- Expected: 1 (Té verde + Leche de avena)

-- Running the same insert again assigns nothing.
INSERT INTO product_allowed_option (product_id, product_option_id)
SELECT 10, 7
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM product_allowed_option
    WHERE product_id = 10 AND product_option_id = 7
);
SELECT ROW_COUNT() AS options_assigned;   -- Expected: 0

SELECT p.name AS product, po.option_group, po.name AS option_name, po.extra_price
FROM product_allowed_option pao
JOIN product p         ON p.product_id = pao.product_id
JOIN product_option po ON po.product_option_id = pao.product_option_id
WHERE pao.product_id = 10
ORDER BY po.option_group, po.name;

DELETE FROM product_allowed_option
WHERE product_id = 10 AND product_option_id = 7;

-- -------------------------------------------------------------
-- C4 Stock: a merchandise entry and an adjustment that would leave
-- negative stock. The validated update affects no rows.
-- -------------------------------------------------------------
UPDATE supply
SET current_stock = current_stock + 12.00
WHERE supply_id = 6;                      -- Leche deslactosada: 6.00 -> 18.00, leaves the alert

SELECT name, current_stock, minimum_stock,
       current_stock <= minimum_stock AS in_alert
FROM supply WHERE supply_id = 6;

UPDATE supply
SET current_stock = current_stock - 20.00
WHERE supply_id = 6
  AND current_stock - 20.00 >= 0;
SELECT ROW_COUNT() AS supplies_updated;   -- Expected: 0 (would be negative)

UPDATE supply
SET current_stock = current_stock - 12.00
WHERE supply_id = 6
  AND current_stock - 12.00 >= 0;         -- Back to 6.00, in alert again

-- -------------------------------------------------------------
-- C5 Constraint violations enforced by the database.
-- These statements fail on purpose, so they are commented out.
-- To demonstrate one, select the statement without the leading
-- "-- " and run only the selection.
-- -------------------------------------------------------------
-- CHECK: quantity must be at least 1 (error 3819)
-- INSERT INTO order_item (customer_order_id, product_id, quantity, unit_price) VALUES (12, 1, 0, 35.00);

-- CHECK: stock cannot be negative (error 3819)
-- UPDATE supply SET current_stock = -1 WHERE supply_id = 1;

-- UNIQUE: username already exists (error 1062)
-- INSERT INTO user (full_name, username, password_hash, role) VALUES ('Duplicado', 'cruiz', 'x', 'Employee');

-- UNIQUE: the same option cannot be allowed twice for a product (error 1062)
-- INSERT INTO product_allowed_option (product_id, product_option_id) VALUES (3, 1);

-- UNIQUE: the same option cannot be chosen twice for one order line (error 1062)
-- INSERT INTO order_item_option (order_item_id, product_option_id, extra_price) VALUES (1, 3, 15.00);

-- FOREIGN KEY: a category with products cannot be deleted (error 1451)
-- DELETE FROM product_category WHERE product_category_id = 1;

-- FOREIGN KEY: a product must belong to an existing category (error 1452)
-- INSERT INTO product (product_category_id, name, base_price) VALUES (99, 'Sin categoría', 20.00);

-- NOT NULL: a supplier needs a contact (error 1048)
-- INSERT INTO supplier (name, contact) VALUES ('Sin contacto', NULL);

-- ENUM: the status must be one of the five defined values (error 1265)
-- UPDATE customer_order SET status = 'Lost' WHERE customer_order_id = 12;

-- =============================================================
-- Part 3 - Views (Frontend)
-- =============================================================
