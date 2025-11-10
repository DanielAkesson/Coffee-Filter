using System;

namespace CoffeeFilter
{
    class Program
    {
        static string codeFilePath = "../../../../CoffeeFilter/test.al";
        static void Main(string[] args)
        {
            CoffeeFilter.RunPrompt();
            CoffeeFilter.RunFile(codeFilePath);
            Console.ReadLine();
        }
    }
}
