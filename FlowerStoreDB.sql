CREATE DATABASE FlowerStoreDB;
GO

USE FlowerStoreDB;
GO

-- 1. Customers
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- 2. Flowers
CREATE TABLE Flowers (
    Id INT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price FLOAT NOT NULL,
    Quantity INT NOT NULL
);

-- 3. Orders
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT,
    OrderDate DATETIME,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- 4. OrderDetails
CREATE TABLE OrderDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT,
    FlowerId INT,
    Quantity INT,
    Price FLOAT,

    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (FlowerId) REFERENCES Flowers(Id)
);
INSERT INTO Flowers (Id, Name, Price, Quantity) VALUES
(1, 'Rose', 10, 100),
(2, 'Tulip', 15, 50),
(3, 'Lily', 20, 30),
(4, 'Orchid', 25, 20);
INSERT INTO Customers (Name) VALUES
(N'Nguyen Viet Anh'),
(N'Tran Thi B'),
(N'Le Van C'),
(N'Pham Thi D');
INSERT INTO Orders (CustomerId, OrderDate) VALUES
(1, '2026-03-20'),
(2, '2026-03-21'),
(3, '2026-03-22'),
(1, '2026-03-23');
INSERT INTO OrderDetails (OrderId, FlowerId, Quantity, Price) VALUES
(1, 1, 5, 10),   -- Order 1 mua Rose
(1, 2, 2, 15),   -- Order 1 mua Tulip

(2, 3, 3, 20),   -- Order 2 mua Lily

(3, 1, 1, 10),   -- Order 3 mua Rose
(3, 4, 2, 25),   -- Order 3 mua Orchid

(4, 2, 4, 15);   -- Order 4 mua Tulip