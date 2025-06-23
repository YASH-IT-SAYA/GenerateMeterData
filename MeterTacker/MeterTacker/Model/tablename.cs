using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterTacker.Model
{
    public class tablename
    {
        public string DisplayName { get; set; }
        public string ActualName { get; set; }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
