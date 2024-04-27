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
using System.Configuration;

namespace SqlLoadRunner
{
    class Program
    {
        static List<Query> _queries = new List<Query>();    
        static List<SqlServer> _sqlServers = new List<SqlServer>();
        static int _minDbConnections;
        static int _maxDbConnections;

        static void Main(string[] args)
        {
            // get min and max settings from settinhgs file
            
            if(int.TryParse( ConfigurationManager.AppSettings["MinDbConnections"], out int minResult)){
                _minDbConnections = minResult;
            }
            if (int.TryParse(ConfigurationManager.AppSettings["MaxDbConnections"], out int maxResult)){
                _maxDbConnections = maxResult;
            }

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
                            string folderNameOnly = Path.GetFileName(folder);
                            string fileNameOnly = Path.GetFileName(f);
                            string text = File.ReadAllText(f);

                            if (string.IsNullOrEmpty(text)) continue; 

                            Query q = new Query() { Sql = text, DatabaseTarget = folderNameOnly, Name = fileNameOnly };
                            _queries.Add(q);
                        }
                    
                }

                var serverXml = XElement.Load((string)binFolder + "\\servers.xml");


                foreach (var server in serverXml.Elements("server"))
                {
                    
                    //_connectionStrings.Add($"server={server.Attribute("name").Value};Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;");
                    _sqlServers.Add(new SqlServer() { ConnectionString = $"server={server.Attribute("name").Value};Database=master;Trusted_Connection=True;Application Name=SqlLoadRunner;TrustServerCertificate=True;"
                        , Name=server.Attribute("name").Value });
                }

                // start a task for each server and then start another for each db localted
                // server
                foreach (var sqlServer in _sqlServers)
                {
                    // at server level

                    Task.Run(() =>
                    {
                        try
                        {
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
                                    var dbQueries = _queries.Where(q => dbName.ToUpper().Contains(q.DatabaseTarget.ToUpper())).ToList();
                                    var db = new Database(dbName, sqlServer.ConnectionString, dbQueries, _minDbConnections, _maxDbConnections);
                                    sqlServer.Databases.Add(db);
                                    Console.WriteLine($"Adding {db.Name}, found {dbQueries.Count} queries"  );
                                }
                            }
                        }catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                        }   
                    });
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
