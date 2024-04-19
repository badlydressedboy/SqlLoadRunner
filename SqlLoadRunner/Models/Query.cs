using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLoadRunner.Models
{
    public class Query
    {
        public string DatabaseTarget { get; set; }  
        public string Name { get; set; }    
        public string Sql { get; set; }
    }
}
