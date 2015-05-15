using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockUpdate
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public double Temp { get; set; }
        public double Pressure { get; set; }
    }
}
