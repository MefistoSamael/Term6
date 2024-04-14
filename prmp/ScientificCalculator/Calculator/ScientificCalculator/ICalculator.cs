using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificCalculator.Calculator.ScientificCalculator
{
    internal interface ICalculator
    {
        Task<string> Plus(string first, string second);
        Task<string> Minus(string first, string second);
        Task<string> Mult(string first, string second);
        Task<string> Div(string first, string second);
        Task<string> YsPower(string first, string second);
        Task<string> NthRoot(string first, string second);
        
        
        string Sign(string first);
        Task<string> Percent(string first);
        string Comma(string first);
        Task<string> TenPower(string first);
        Task<string> Cube(string first);
        Task<string> Square(string first);
        Task<string> Invert(string first);
        Task<string> SquareRoot(string first);
        Task<string> CubeRoot(string first);
        Task<string> NaturalLogarithm(string first);
        Task<string> DecimalLogarithm(string first);
        Task<string> Sin(string first);
        Task<string> Cos(string first);
        Task<string> Tan(string first);
        string EulersNumber();
        string Pi();
        string Rand();
    }
}
