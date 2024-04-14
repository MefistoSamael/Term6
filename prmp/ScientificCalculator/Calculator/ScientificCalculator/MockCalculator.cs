using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificCalculator.Calculator.ScientificCalculator
{
    class MockCalculator : ICalculator
    {
        public async Task<string> Plus(string first, string second)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Minus(string first, string second)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Mult(string first, string second)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Div(string first, string second)
        { 
            throw new NotImplementedException();
        }
        public async Task<string> YsPower(string first, string second)
        { 
            throw new NotImplementedException();
        }
        public async Task<string> NthRoot(string first, string second)
        { 
            throw new NotImplementedException();
        }


        public string Sign(string first)
        { 
            throw new NotImplementedException(); 
        }
        public async Task<string> Percent(string first)
        { 
            throw new NotImplementedException(); 
        }
        public string Comma(string first)
        { 
            throw new NotImplementedException();
        }
        public async Task<string> TenPower(string first)
        { 
            return Double.MaxValue.ToString("0." + new string('#', 339));
        }
        public async Task<string> Cube(string first)
        { 
            throw new NotImplementedException();
        }
        public async Task<string> Square(string first)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Invert(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> SquareRoot(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> CubeRoot(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> NaturalLogarithm(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> DecimalLogarithm(string first) 
        {
            throw new NotImplementedException();
        }
        public async Task<string> Sin(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> Cos(string first) 
        { 
            throw new NotImplementedException();
        }
        public async Task<string> Tan(string first) 
        { 
            throw new NotImplementedException();
        }
        public string EulersNumber() 
        { 
            throw new NotImplementedException();
        }
        public string Pi() 
        { 
            throw new NotImplementedException();
        }

        public string Rand()
        {
            throw new NotImplementedException();
        }

    }
}
