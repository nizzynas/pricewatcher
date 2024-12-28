using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pricewatchingni
{
    public class TokenBalance
    {
        public string Mint { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Decimals { get; set; }
    }
}
