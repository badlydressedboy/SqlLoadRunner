using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Data.SqlClient;

namespace SqlLoadRunner.Models
{
    public class Database
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        List<Query> _referenceQueries; // only ones appropriated for this db 
        
        List<QueryInstance> _queryInstances = new List<QueryInstance>(); 

        
        public Database(string name, string connectionString, List<Query> queries)
        {
            Name = name;
            ConnectionString = connectionString + $"; database={name}";
            _referenceQueries = queries;

            SetupQueries();

            var timer = new System.Timers.Timer(3000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();  
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // fires every 2 seconds regardless of the time it takes to run the query
            SetupQueries();
        }

        private void SetupQueries()
        {
            // generate random number of queries to run

            if (_referenceQueries.Count == 0) return; // no queries to run 

            var random = new Random();
            var targetQueryCount = random.Next(1, 5);

            _queryInstances.RemoveAll(q => !q.IsRunning);

            int runningCount = _queryInstances.FindAll(q => q.IsRunning).Count;

            if (runningCount < targetQueryCount)
            {
                for (int i = 0; i < targetQueryCount - runningCount; i++)
                {
                    // get random query
                    var query = _referenceQueries[random.Next(0, _referenceQueries.Count)];

                    var newQuery = new QueryInstance() { DatabaseTarget = Name, Name = query.Name, Sql = query.Sql, IsRunning = false };
                    _queryInstances.Add(newQuery);

                    // start the query
                    Task.Run(() => StartQuery(newQuery));
                }
            }
        }

        private void StartQuery(QueryInstance query)
        {
            // start the query
            if (query == null || query.IsRunning) return;
            
            query.IsRunning = true;
            Console.WriteLine($"Starting query {query.Name}");
            try
            {
                using(var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using(var cmd = new SqlCommand(query.Sql, conn))
                    {
                        cmd.CommandTimeout = 600;
                        cmd.ExecuteReader();
                    }
                }
                Console.WriteLine($"Complete query {query.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                query.IsRunning = false;
            }
        }   
    }
}