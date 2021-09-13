SELECT SUM([UnitPrice])
	, SUM(TotalProductCost)
	, SUM(SalesAmount)
  FROM [AdventureWorksDW2012].[dbo].[FactInternetSales]
WHERE PromotionKey < 10