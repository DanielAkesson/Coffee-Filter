using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeFilterASTGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ASTGenerator AST_Gen = new ASTGenerator();
            AST_Gen.CreateAST();
        }
    }
}
