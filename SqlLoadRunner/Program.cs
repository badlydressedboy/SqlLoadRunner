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

            try
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var queriesFolder = binFolder + "\\Queries";    
                var dbfolders = Directory.GetDirectories(queriesFolder);

                foreach (var folder in dbfolders)
                {
                   
                        var fileEntries = Directory.GetFiles(folder);
                        foreach (var f in fileEntries)
                        {
                            string text = File.ReadAllText(f);
                            Query q = new Query() { Sql = text, DatabaseTarget = folder };
                            _queries.Add(q);
                        }
                    
                }

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
                        comm.CommandTimeout = 10000;
                        comm.CommandText = "SELECT name FROM sys.databases where name not in ('master','tempdb','model','msdb')";
                        comm.CommandType = System.Data.CommandType.Text;
                        SqlDataReader reader = comm.ExecuteReader();
                        while (reader.Read())
                        {
                            string dbName = reader.GetString(0);
                            var dbQueries = _queries.Where(q => dbName.Contains(q.DatabaseTarget)).ToList();   
                            var db = new Database(dbName, sqlServer.ConnectionString, dbQueries);
                            sqlServer.Databases.Add(db);
                        }
                    }
                }   

                // start a thread for each DB found



                
                
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            do
            {
                while (!Console.KeyAvailable)
                {
                  
                    Thread.Sleep(1000);
                  
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);                            

        }

       
    }
}
