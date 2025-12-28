/* =========================================================
   DATABASE
========================================================= */
CREATE DATABASE RealEstateDB
GO
USE RealEstateDB
GO

/* =========================================================
   TABLES
========================================================= */

CREATE TABLE dbo.Parties (
    PartyId INT IDENTITY PRIMARY KEY,
    Type NVARCHAR(50),
    Name NVARCHAR(100),
    CNIC NVARCHAR(20),
    ContactPhone NVARCHAR(20),
    ContactEmail NVARCHAR(100),
    Address NVARCHAR(500),
    Status NVARCHAR(50) DEFAULT 'Active',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.Projects (
    ProjectId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(200),
    Location NVARCHAR(200),
    Status NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.Plots (
    PlotId INT IDENTITY PRIMARY KEY,
    ProjectId INT,
    PlotNo NVARCHAR(50),
    SizeMarla DECIMAL(10,2),
    Price DECIMAL(18,2),
    Status NVARCHAR(50) DEFAULT 'Available',
    BuyerId INT,
    OwnerId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.Sales (
    SaleId INT IDENTITY PRIMARY KEY,
    ProjectId INT,
    PlotId INT,
    BuyerId INT,
    SellerId INT,
    SalePrice DECIMAL(18,2),
    SaleDate DATE,
    DownPayment DECIMAL(18,2),
    Status NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.PaymentPlans (
    PaymentPlanId INT IDENTITY PRIMARY KEY,
    SaleId INT UNIQUE,
    TotalAmount DECIMAL(18,2),
    DownPayment DECIMAL(18,2),
    NumberOfInstallments INT,
    InstallmentAmount DECIMAL(18,2),
    StartDate DATE,
    PlanType NVARCHAR(50) DEFAULT 'Monthly',
    Status NVARCHAR(50) DEFAULT 'Active',
    OverdueReminder BIT DEFAULT 1,
    UpcomingReminder BIT DEFAULT 1
)
GO

CREATE TABLE dbo.Installments (
    InstallmentId INT IDENTITY PRIMARY KEY,
    PaymentPlanId INT,
    InstallmentNo INT,
    DueDate DATE,
    Amount DECIMAL(18,2),
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Due',
    TransactionId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.Transactions (
    TransactionId INT IDENTITY PRIMARY KEY,
    SaleId INT,
    InstallmentId INT,
    Date DATE,
    Amount DECIMAL(18,2),
    Type NVARCHAR(50),
    Description NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.Users (
    UserId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    Role NVARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)
GO

CREATE TABLE dbo.__EFMigrationsHistory (
    MigrationId NVARCHAR(150) PRIMARY KEY,
    ProductVersion NVARCHAR(32) NOT NULL
)
GO

/* =========================================================
   FOREIGN KEYS (BASE)
========================================================= */

ALTER TABLE dbo.Plots
ADD FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(ProjectId)
GO
ALTER TABLE dbo.Plots
ADD FOREIGN KEY (BuyerId) REFERENCES dbo.Parties(PartyId)
GO
ALTER TABLE dbo.Plots
ADD CONSTRAINT FK_Plots_Owner FOREIGN KEY (OwnerId)
REFERENCES dbo.Parties(PartyId)
GO

ALTER TABLE dbo.Sales
ADD FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(ProjectId)
GO
ALTER TABLE dbo.Sales
ADD FOREIGN KEY (PlotId) REFERENCES dbo.Plots(PlotId)
GO
ALTER TABLE dbo.Sales
ADD FOREIGN KEY (BuyerId) REFERENCES dbo.Parties(PartyId)
GO
ALTER TABLE dbo.Sales
ADD FOREIGN KEY (SellerId) REFERENCES dbo.Parties(PartyId)
GO

ALTER TABLE dbo.PaymentPlans
ADD FOREIGN KEY (SaleId) REFERENCES dbo.Sales(SaleId)
GO

ALTER TABLE dbo.Installments
ADD FOREIGN KEY (PaymentPlanId) REFERENCES dbo.PaymentPlans(PaymentPlanId)
GO

ALTER TABLE dbo.Transactions
ADD FOREIGN KEY (SaleId) REFERENCES dbo.Sales(SaleId)
GO
ALTER TABLE dbo.Transactions
ADD FOREIGN KEY (InstallmentId) REFERENCES dbo.Installments(InstallmentId)
GO

/* =========================================================
   CASCADE DELETE
========================================================= */

ALTER TABLE dbo.Plots
DROP CONSTRAINT IF EXISTS FK_Plots_ProjectId
GO
ALTER TABLE dbo.Plots
ADD CONSTRAINT FK_Plots_ProjectId_Cascade
FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(ProjectId)
ON DELETE CASCADE
GO

ALTER TABLE dbo.Sales
DROP CONSTRAINT IF EXISTS FK_Sales_PlotId
GO
ALTER TABLE dbo.Sales
ADD CONSTRAINT FK_Sales_PlotId_Cascade
FOREIGN KEY (PlotId) REFERENCES dbo.Plots(PlotId)
ON DELETE CASCADE
GO

ALTER TABLE dbo.PaymentPlans
DROP CONSTRAINT IF EXISTS FK_PaymentPlans_SaleId
GO
ALTER TABLE dbo.PaymentPlans
ADD CONSTRAINT FK_PaymentPlans_SaleId_Cascade
FOREIGN KEY (SaleId) REFERENCES dbo.Sales(SaleId)
ON DELETE CASCADE
GO

ALTER TABLE dbo.Installments
DROP CONSTRAINT IF EXISTS FK_Installments_PaymentPlanId
GO
ALTER TABLE dbo.Installments
ADD CONSTRAINT FK_Installments_PaymentPlanId_Cascade
FOREIGN KEY (PaymentPlanId) REFERENCES dbo.PaymentPlans(PaymentPlanId)
ON DELETE CASCADE
GO

ALTER TABLE dbo.Transactions
DROP CONSTRAINT IF EXISTS FK_Transactions_InstallmentId
GO
ALTER TABLE dbo.Transactions
ADD CONSTRAINT FK_Transactions_InstallmentId_Cascade
FOREIGN KEY (InstallmentId) REFERENCES dbo.Installments(InstallmentId)
ON DELETE CASCADE
GO

/* =========================================================
   STORED PROCEDURE
========================================================= */

CREATE PROCEDURE dbo.sp_RecordPayment
    @InstallmentId INT,
    @Amount DECIMAL(18,2),
    @PaymentDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TransactionId INT;

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO dbo.Transactions (SaleId, InstallmentId, Date, Amount, Type)
        SELECT pp.SaleId, @InstallmentId, @PaymentDate, @Amount, 'Payment'
        FROM dbo.Installments i
        JOIN dbo.PaymentPlans pp ON i.PaymentPlanId = pp.PaymentPlanId
        WHERE i.InstallmentId = @InstallmentId;

        SET @TransactionId = SCOPE_IDENTITY();

        UPDATE dbo.Installments
        SET PaidAmount = PaidAmount + @Amount,
            Status = CASE WHEN PaidAmount + @Amount >= Amount THEN 'Paid' ELSE 'Partial' END,
            TransactionId = @TransactionId,
            UpdatedAt = GETDATE()
        WHERE InstallmentId = @InstallmentId;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO

/* =========================================================
   LEDGER VIEWS
========================================================= */

CREATE VIEW dbo.vw_BuyerLedger AS
SELECT
    p.PartyId,
    p.Name AS BuyerName,
    t.TransactionId,
    t.Date,
    t.Amount,
    t.Type,
    s.SaleId,
    pl.PlotNo,
    pr.Name AS ProjectName
FROM Parties p
JOIN Sales s ON s.BuyerId = p.PartyId
JOIN Transactions t ON t.SaleId = s.SaleId
JOIN Plots pl ON s.PlotId = pl.PlotId
JOIN Projects pr ON pl.ProjectId = pr.ProjectId
WHERE p.Type = 'Buyer'
GO

CREATE VIEW dbo.vw_SellerLedger AS
SELECT
    p.PartyId,
    p.Name AS SellerName,
    t.TransactionId,
    t.Date,
    t.Amount,
    t.Type,
    s.SaleId,
    pl.PlotNo,
    pr.Name AS ProjectName
FROM Parties p
JOIN Sales s ON s.SellerId = p.PartyId
JOIN Transactions t ON t.SaleId = s.SaleId
JOIN Plots pl ON s.PlotId = pl.PlotId
JOIN Projects pr ON pl.ProjectId = pr.ProjectId
WHERE p.Type = 'Seller'
GO
