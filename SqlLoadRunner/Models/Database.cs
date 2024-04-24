using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace SqlLoadRunner.Models
{
    public class Database
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        List<Query> _queries; // only ones appropriated for this db 
        
        List<QueryInstance> _queryInstances = new List<QueryInstance>(); 

        public Database(string name, string connectionString, List<Query> queries)
        {
            Name = name;
            ConnectionString = connectionString + $"; database={name}";
            _queries = queries; 

            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // fires every 2 seconds regardless of the time it takes to run the query
            // generate random number of queries to run
            var random = new Random();  
            var queryCount = random.Next(1, 5);

            int runningCount = _queryInstances.FindAll(q => q.IsRunning).Count;

            if (runningCount < queryCount)
            {
                for (int i = 0; i < queryCount - runningCount; i++)
                {
                    var query = _queries[random.Next(0, _queries.Count)];
                    _queryInstances.Add(new QueryInstance() { DatabaseTarget = Name, Name = query.Name, Sql = query.Sql, IsRunning = false }); 
                }
            }   
        }
    }
}