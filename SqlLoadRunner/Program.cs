using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using SqlLoadRunner.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace SqlLoadRunner
{
    class Program
    {
        static string _connString = "server=localhost\\dev2019;Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;";
        
        static List<Query> _queries = new List<Query>();    

        static List<string> _connectionStrings = new List<string>();
        static List<SqlServer> _sqlServers = new List<SqlServer>();


        // populate what databases are installed
        static List<string> _databases = new List<string>() { "StackOverflow2010" };
        
        static int ThreadCount = 0;
        static void Main(string[] args)
        {
            int minConnections = 5;
            int maxConnections = 20;

            // need to setup config file for this so local pc and vm can be radically different 
            

            Console.WriteLine("Starting... ESC to stop");

            List<string> queries = new List<string>();

            try
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var queriesFolder = binFolder + "\\Queries";    
                var dbfolders = Directory.GetDirectories(binFolder);

                var serverXml = XElement.Load((string)binFolder + "\\servers.xml");


                foreach (var server in serverXml.Elements("server"))
                {
                    Console.WriteLine("Adding " + server.Attribute("name").Value);
                    //_connectionStrings.Add($"server={server.Attribute("name").Value};Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;");
                    _sqlServers.Add(new SqlServer() { ConnectionString = $"server={server.Attribute("name").Value};Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;", Name=server.Attribute("name").Value });
                }

                // start a task for each server and then start another for each db localted
                // server
                foreach (var sqlServer in _sqlServers)
                {
                    // at server level
                   
                    // get databases
                    using (SqlConnection conn1 = new SqlConnection(sqlServer.ConnectionString))
                    {
                        conn1.Open();
                        SqlCommand comm = conn1.CreateCommand();
                        comm.CommandTimeout = 30000;
                        comm.CommandText = "SELECT name FROM sys.databases";
                        comm.CommandType = System.Data.CommandType.Text;
                        SqlDataReader reader = comm.ExecuteReader();
                        while (reader.Read())
                        {
                            var db = new Database(reader.GetString(0), sqlServer.ConnectionString);
                            sqlServer.Databases.Add(db);
                        }
                    }
                }   

                // start a thread for each DB found



                foreach (var folder in dbfolders)
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
