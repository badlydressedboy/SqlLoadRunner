using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;

namespace SqlLoadRunner
{
    class Program
    {
        static string _connString = "server=localhost;Database=master;Trusted_Connection=True;Application Name=Load Test;";

        static int ThreadCount = 0;
        static void Main(string[] args)
        {
            int minConnections = 5;
            int maxConnections = 20;
            

            Console.WriteLine("Starting... ESC to stop");

            List<string> queries = new List<string>();
            //string query = 
            queries.Add(@"BEGIN TRAN
DELETE TOP (1) FROM [Adventureworks2012].[Purchasing].[Vendor]
ROLLBACK");
            queries.Add(@"DECLARE @RC int
EXECUTE @RC = [Adventureworks2012].[dbo].[uspGetBillOfMaterials] 316, '2002-06-01 00:00:00.000'");
            queries.Add("BEGIN TRAN COMMIT TRAN");
            queries.Add("SELECT * FROM [AdventureWorksDW2012].[dbo].[DimCustomer] BEGIN TRAN COMMIT TRAN");            
            queries.Add("SELECT *  FROM [AdventureWorksDW2012].[dbo].[FactResellerSales] BEGIN TRAN COMMIT TRAN");            
            queries.Add("SELECT *  FROM [AdventureWorksDW2012].[dbo].[FactResellerSales] WHERE CurrencyKey = 100");            
            queries.Add(@"SELECT *  
    FROM [AdventureWorksDW2012].[dbo].[FactResellerSales] frs
    INNER JOIN AdventureWorksDW2012.dbo.DimCurrency dc
	    ON frs.CurrencyKey = dc.CurrencyKey
    WHERE dc.CurrencyKey = 100
    ORDER BY ShipDateKey
    ");            
            queries.Add(@"SELECT SUM([UnitPrice])
	, SUM(TotalProductCost)
	, SUM(SalesAmount)
  FROM [AdventureWorksDW2012].[dbo].[FactInternetSales]
WHERE PromotionKey < 10");
            queries.Add(@"UPDATE [AdventureWorksDW2012].[dbo].[FactSalesQuota]
SET [SalesAmountQuota] = [SalesAmountQuota] + 1

UPDATE [AdventureWorksDW2012].[dbo].[FactSalesQuota]
SET [SalesAmountQuota] = [SalesAmountQuota] - 1");

            do
            {
                while (!Console.KeyAvailable)
                {
                    // random amount of connection in bounds per iteration
                    Random rnd = new Random();
                    int connections = rnd.Next(minConnections, maxConnections);

                    for (int i = connections; i > 0; i--)
                    {
                        string iterationQuery = queries[i % queries.Count];
                        Thread myThread = new Thread(() => DoQuery(iterationQuery));
                        myThread.Start();
                    }

                    while (ThreadCount > 0)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    Console.WriteLine("Finished iteration");
                    System.Threading.Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);                            

        }

        private static void DoQuery(string query)
        {
            ThreadCount++;
            Console.WriteLine(query);

            try
            {
                using (SqlConnection conn = new SqlConnection(_connString))
                {                    
                    conn.Open();
                    SqlCommand comm = conn.CreateCommand();
                    comm.CommandTimeout = 5000;
                    
                    comm.CommandText = query;
                    comm.CommandType = System.Data.CommandType.Text;
                    comm.ExecuteNonQuery();
                }
                
            }
            catch (SqlException ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

            ThreadCount--;
        }
    }
}
