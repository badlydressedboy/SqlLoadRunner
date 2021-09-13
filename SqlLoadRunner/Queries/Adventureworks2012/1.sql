BEGIN TRAN
DELETE TOP (1) FROM [Adventureworks2012].[Purchasing].[Vendor]
ROLLBACK