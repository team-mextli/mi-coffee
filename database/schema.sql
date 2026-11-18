-- =============================================================
-- Mi Coffee - database schema
-- Based on "Análisis del problema del proyecto final Mi Coffee"
-- 12 tables, 9 relationships (2 N:M resolved with junction tables)
-- Target: MySQL 8.0.16+ (CHECK constraints are enforced)
-- =============================================================

DROP DATABASE IF EXISTS mi_coffee;

CREATE DATABASE mi_coffee
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE mi_coffee;

-- -------------------------------------------------------------
-- Staff access
-- -------------------------------------------------------------

-- Staff account (employee or admin). Users are deactivated, never
-- deleted, so the order status history keeps its references.
CREATE TABLE user (
    user_id        INT AUTO_INCREMENT PRIMARY KEY,
    full_name      VARCHAR(100) NOT NULL,
    username       VARCHAR(50)  NOT NULL,
    password_hash  VARCHAR(255) NOT NULL,
    role           ENUM('Employee', 'Admin') NOT NULL,
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_user_username UNIQUE (username)
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Catalog
-- -------------------------------------------------------------

CREATE TABLE product_category (
    product_category_id  INT AUTO_INCREMENT PRIMARY KEY,
    name                 VARCHAR(50) NOT NULL,
    CONSTRAINT uq_product_category_name UNIQUE (name)
) ENGINE = InnoDB;

CREATE TABLE product (
    product_id           INT AUTO_INCREMENT PRIMARY KEY,
    product_category_id  INT NOT NULL,
    name                 VARCHAR(100) NOT NULL,
    base_price           DECIMAL(10,2) NOT NULL,
    is_available         BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_product_product_category
        FOREIGN KEY (product_category_id)
        REFERENCES product_category (product_category_id),
    CONSTRAINT ck_product_base_price CHECK (base_price >= 0)
) ENGINE = InnoDB;

-- Customization option (size, milk, sweetener, extra).
-- "option_group" avoids the reserved word GROUP.
CREATE TABLE product_option (
    product_option_id  INT AUTO_INCREMENT PRIMARY KEY,
    option_group       VARCHAR(50) NOT NULL,
    name               VARCHAR(50) NOT NULL,
    extra_price        DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT ck_product_option_extra_price CHECK (extra_price >= 0)
) ENGINE = InnoDB;

-- N:M product <-> product_option: which options each product allows.
CREATE TABLE product_allowed_option (
    product_allowed_option_id  INT AUTO_INCREMENT PRIMARY KEY,
    product_id                 INT NOT NULL,
    product_option_id          INT NOT NULL,
    CONSTRAINT uq_product_allowed_option UNIQUE (product_id, product_option_id),
    CONSTRAINT fk_product_allowed_option_product
        FOREIGN KEY (product_id)
        REFERENCES product (product_id),
    CONSTRAINT fk_product_allowed_option_product_option
        FOREIGN KEY (product_option_id)
        REFERENCES product_option (product_option_id)
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Customer orders
-- -------------------------------------------------------------

-- Confirmed order. customer_order_id is also the order number shown
-- to the customer. Orders are never deleted. No relation with user:
-- customers order without logging in.
CREATE TABLE customer_order (
    customer_order_id  INT AUTO_INCREMENT PRIMARY KEY,
    created_at         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    customer_name      VARCHAR(50) NULL,
    total              DECIMAL(10,2) NOT NULL,
    status             ENUM('Received', 'InPreparation', 'Ready', 'Delivered', 'Cancelled')
                       NOT NULL DEFAULT 'Received',
    CONSTRAINT ck_customer_order_total CHECK (total >= 0),
    INDEX ix_customer_order_created_at (created_at),
    INDEX ix_customer_order_status (status)
) ENGINE = InnoDB;

-- One line of an order. unit_price is copied from product.base_price
-- when the order is confirmed so later price changes do not affect it.
CREATE TABLE order_item (
    order_item_id      INT AUTO_INCREMENT PRIMARY KEY,
    customer_order_id  INT NOT NULL,
    product_id         INT NOT NULL,
    quantity           INT NOT NULL,
    unit_price         DECIMAL(10,2) NOT NULL,
    note               VARCHAR(255) NULL,
    CONSTRAINT fk_order_item_customer_order
        FOREIGN KEY (customer_order_id)
        REFERENCES customer_order (customer_order_id),
    CONSTRAINT fk_order_item_product
        FOREIGN KEY (product_id)
        REFERENCES product (product_id),
    CONSTRAINT ck_order_item_quantity CHECK (quantity >= 1),
    CONSTRAINT ck_order_item_unit_price CHECK (unit_price >= 0)
) ENGINE = InnoDB;

-- N:M order_item <-> product_option: options chosen for one line.
-- extra_price is copied from product_option at confirmation time.
CREATE TABLE order_item_option (
    order_item_option_id  INT AUTO_INCREMENT PRIMARY KEY,
    order_item_id         INT NOT NULL,
    product_option_id     INT NOT NULL,
    extra_price           DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_order_item_option_order_item
        FOREIGN KEY (order_item_id)
        REFERENCES order_item (order_item_id),
    CONSTRAINT fk_order_item_option_product_option
        FOREIGN KEY (product_option_id)
        REFERENCES product_option (product_option_id),
    CONSTRAINT ck_order_item_option_extra_price CHECK (extra_price >= 0)
) ENGINE = InnoDB;

-- Traceability: every status change records who did it and when.
CREATE TABLE order_status_history (
    order_status_history_id  INT AUTO_INCREMENT PRIMARY KEY,
    customer_order_id        INT NOT NULL,
    user_id                  INT NOT NULL,
    previous_status          ENUM('Received', 'InPreparation', 'Ready', 'Delivered', 'Cancelled') NOT NULL,
    new_status               ENUM('Received', 'InPreparation', 'Ready', 'Delivered', 'Cancelled') NOT NULL,
    changed_at               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_order_status_history_customer_order
        FOREIGN KEY (customer_order_id)
        REFERENCES customer_order (customer_order_id),
    CONSTRAINT fk_order_status_history_user
        FOREIGN KEY (user_id)
        REFERENCES user (user_id)
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Inventory and suppliers
-- -------------------------------------------------------------

CREATE TABLE supplier (
    supplier_id  INT AUTO_INCREMENT PRIMARY KEY,
    name         VARCHAR(100) NOT NULL,
    contact      VARCHAR(100) NOT NULL,
    note         VARCHAR(255) NULL
) ENGINE = InnoDB;

CREATE TABLE supply_category (
    supply_category_id  INT AUTO_INCREMENT PRIMARY KEY,
    name                VARCHAR(50) NOT NULL,
    CONSTRAINT uq_supply_category_name UNIQUE (name)
) ENGINE = InnoDB;

-- Alerts are not stored: a supply is in alert when
-- current_stock <= minimum_stock (computed at query time).
CREATE TABLE supply (
    supply_id           INT AUTO_INCREMENT PRIMARY KEY,
    supply_category_id  INT NOT NULL,
    supplier_id         INT NULL,
    name                VARCHAR(100) NOT NULL,
    unit                VARCHAR(20) NOT NULL,
    current_stock       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    minimum_stock       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT fk_supply_supply_category
        FOREIGN KEY (supply_category_id)
        REFERENCES supply_category (supply_category_id),
    CONSTRAINT fk_supply_supplier
        FOREIGN KEY (supplier_id)
        REFERENCES supplier (supplier_id),
    CONSTRAINT ck_supply_current_stock CHECK (current_stock >= 0),
    CONSTRAINT ck_supply_minimum_stock CHECK (minimum_stock >= 0)
) ENGINE = InnoDB;
