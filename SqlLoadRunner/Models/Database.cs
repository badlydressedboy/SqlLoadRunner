namespace SqlLoadRunner.Models
{
    public class Database
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }

        public Database(string name, string connectionString)
        {
            Name = name;
            ConnectionString = connectionString + $"; database={name}";
        }   
    }
}