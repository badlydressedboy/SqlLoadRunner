using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLoadRunner.Models
{
    public class SqlServer
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public List<Database> Databases { get; set; }   = new List<Database>();
    }
}
