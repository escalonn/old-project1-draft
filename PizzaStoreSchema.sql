USE PizzaStoreDB;
GO

ALTER TABLE PizzaStore.PSOrderPart DROP CONSTRAINT FK_PSOrderPart_PSOrder;
GO
ALTER TABLE PizzaStore.PSUser DROP CONSTRAINT FK_PSUser_PSLocation;
GO
ALTER TABLE PizzaStore.PSOrder DROP CONSTRAINT FK_PSOrder_PSLocation, CONSTRAINT FK_PSOrder_PSUser;
GO
DROP TABLE PizzaStore.PSOrderPart;
GO
DROP TABLE PizzaStore.PSLocation;
GO
DROP TABLE PizzaStore.PSUser;
GO
DROP TABLE PizzaStore.PSOrder;
GO
DROP SCHEMA PizzaStore;
GO

--SELECT * FROM PizzaStore.PSOrder;
--SELECT * FROM PizzaStore.PSUser;
--SELECT * FROM PizzaStore.PSLocation;
--SELECT * FROM PizzaStore.PSOrderPart;
--GO

-- Scaffold-DbContext "Server=tcp:nicholassqlweek.database.windows.net,1433;Initial Catalog=PizzaStoreDB;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Project PizzaStore.Data -Force

-- SCHEMATA

--DROP SCHEMA PizzaStore
CREATE SCHEMA PizzaStore;
GO

-- TABLES

CREATE TABLE PizzaStore.PSOrder
(
	OrderID int IDENTITY NOT NULL,
	LocationID int NOT NULL,
	UserID int NOT NULL,
	OrderTime datetime2 NULL
)
GO

CREATE TABLE PizzaStore.PSUser
(
	UserID int IDENTITY NOT NULL,
	FirstName nvarchar(128) NOT NULL,
	LastName nvarchar(128) NOT NULL,
	DefaultLocationID int NOT NULL
)
GO

CREATE TABLE PizzaStore.PSLocation
(
	LocationID int IDENTITY NOT NULL,
	Inventory int NOT NULL
)
GO

CREATE TABLE PizzaStore.PSOrderPart
(
	OrderID int NOT NULL,
	Price money NOT NULL,
	Qty int NOT NULL
)
GO

-- PRIMARY KEYS

ALTER TABLE PizzaStore.PSOrder ADD
	CONSTRAINT PK_PSOrder_OrderID PRIMARY KEY (OrderID);
GO

ALTER TABLE PizzaStore.PSUser ADD
	CONSTRAINT PK_PSUser_UserID PRIMARY KEY (UserID);
GO

ALTER TABLE PizzaStore.PSLocation ADD
	CONSTRAINT PK_PSLocation_LocationID PRIMARY KEY (LocationID);
GO

ALTER TABLE PizzaStore.PSOrderPart ADD
	CONSTRAINT PK_PSOrderPart_OrderID_Price PRIMARY KEY (OrderID, Price);
GO

-- FOREIGN KEYS

ALTER TABLE PizzaStore.PSOrder ADD
	CONSTRAINT FK_PSOrder_PSLocation
		FOREIGN KEY (LocationID) REFERENCES PizzaStore.PSLocation (LocationID),
	CONSTRAINT FK_PSOrder_PSUser
		FOREIGN KEY (UserID) REFERENCES PizzaStore.PSUser (UserID);
GO

ALTER TABLE PizzaStore.PSUser ADD
	CONSTRAINT FK_PSUser_PSLocation
		FOREIGN KEY (DefaultLocationID) REFERENCES PizzaStore.PSLocation (LocationID);
GO

ALTER TABLE PizzaStore.PSOrderPart ADD
	CONSTRAINT FK_PSOrderPart_PSOrder
		FOREIGN KEY (OrderID) REFERENCES PizzaStore.PSOrder (OrderID);
GO
