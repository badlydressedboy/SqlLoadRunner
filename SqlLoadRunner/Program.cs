using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;

namespace SqlLoadRunner
{
    class Program
    {
        static string _connString = "server=localhost\\dev2019;Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;";
        
        // populate what databases are installed
        static List<string> _databases = new List<string>() { "StackOverflow2010" };
        
        static int ThreadCount = 0;
        static void Main(string[] args)
        {
            int minConnections = 5;
            int maxConnections = 20;
            

            Console.WriteLine("Starting... ESC to stop");

            List<string> queries = new List<string>();

            try
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Queries";
                var dbfolders = Directory.GetDirectories(binFolder);

                foreach(var folder in dbfolders)
                {
                    if (_databases.Contains(folder.Replace(Path.GetDirectoryName(folder) + Path.DirectorySeparatorChar, "")))
                    {
                        var fileEntries = Directory.GetFiles(folder);
                        foreach(var f in fileEntries)
                        {
                            string text = System.IO.File.ReadAllText(f);
                            queries.Add(text);
                        }
                    }
                }
                



                
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

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
