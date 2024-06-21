using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SqlLoadRunner
{
    public class Helper
    {
        private PerformanceCounter _cpuCounter = new PerformanceCounter();
        
        public Helper()
        {
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";
            dynamic firstValue = _cpuCounter.NextValue();
        }

        public long GetCpuPc()
        {
            // now matches task manager reading
            return (long)_cpuCounter.NextValue();
        }
    }
}
