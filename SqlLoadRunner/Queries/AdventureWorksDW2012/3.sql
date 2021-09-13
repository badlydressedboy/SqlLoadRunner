SELECT *  
    FROM [AdventureWorksDW2012].[dbo].[FactResellerSales] frs
    INNER JOIN AdventureWorksDW2012.dbo.DimCurrency dc
	    ON frs.CurrencyKey = dc.CurrencyKey
    WHERE dc.CurrencyKey = 100` 
    ORDER BY ShipDateKey