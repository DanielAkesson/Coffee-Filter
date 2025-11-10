using System;
using System.Collections.Generic;
using System.IO;

namespace CoffeeFilter
{
    class CoffeeFilter
    {
        private static bool PrintTokenizer = false;
        private static Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;
        public static void RunFile(string path)
        {
            Run(File.ReadAllText(path));
            // Indicate an error in the exit code.
            if (hadError)
            {
                Console.ReadLine();
                System.Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                Console.ReadLine();
                System.Environment.Exit(70);
            }
        }
        public static void RunPrompt()
        {
            Console.WriteLine("Welcome to Coffee Filter prompt!");
            Console.WriteLine("Enter any valid Coffee Filter Line or:");
            Console.WriteLine("-h for help");
            Console.WriteLine("-r {path} to run a file");
            Console.WriteLine("-e to exit the prompt");
            while (true) {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null)
                    break;

                //Commands
                if(line.StartsWith("-"))
                {
                    if (PromptCommand(line.Split(' ')))
                        break;
                    continue;
                }

                //Run actual prompt
                try
                {
                    Run(line);
                }
                catch(Exception error)
                {
                    hadError = true;
                }
                
                hadError = false;
            }
            Console.WriteLine("Leaving prompt!");
        }
        private static bool PromptCommand(params string[] args)
        {
            if (args.Length == 1)
            {
                //Unary commands
                switch (args[0].ToLower())
                {
                    case "-h":
                    case "-help":
                        Console.WriteLine("Coffee Filter is a soft typed language that uses ; as a line-ender");
                        Console.WriteLine("To get more information try one of the following commands:");
                        Console.WriteLine("-h v: Variables");
                        Console.WriteLine("-h t: Types");
                        Console.WriteLine("-h l: List");
                        Console.WriteLine("-h f: Functions");
                        Console.WriteLine("-h b: Branch logic");
                        Console.WriteLine("-h s: Standard Functions");
                        Console.WriteLine("-h g: Get the full Grammar");
                        Console.WriteLine("Coffee Filter prompt");
                        Console.WriteLine("-r {PATH}: Run a file of Coffee Filter code");
                        Console.WriteLine("-h: Help");
                        Console.WriteLine("-e: Exit the prompt");
                        return false;
                    case "-r":
                    case "-run":
                        Console.WriteLine("run requires a path like: -r test.ai");
                        return false;
                    case "-e":
                    case "-exit":
                        return true;
                }
            }

            //Commands with at least two arguments
            if (args.Length > 1)
            {
                switch (args[0].ToLower())
                {
                    case "-h":
                    case "-help":
                        switch (args[1].ToLower())
                        {
                            case "v":
                            case "variables":
                                Console.WriteLine("variables are declared with 'var' {IDENTIFIDER}");
                                Console.WriteLine("variables can be assigned with '=' {VALUE}");
                                Console.WriteLine("and accessed with {IDENTIFIDER}");
                                return false;
                            case "t":
                            case "types":
                                Console.WriteLine("Coffee Script is soft typed and will assign types for you");
                                Console.WriteLine("Supported types are: Int, Bool, Functions, String, List, Object, Null");
                                Console.WriteLine("Basic functions like arithmetic and string concatenations are supported");
                                return false;
                            case "l":
                            case "list":
                                Console.WriteLine("Lists are declared with [{VALUE},*]");
                                Console.WriteLine("Lists have some standard dot accesses: size, add(v), add_range(v), insert(v), remove(v)");
                                Console.WriteLine("Lists can be accessed with {IDENTIFIDER}[] syntax");
                                return false;
                            case "f":
                            case "functions":
                                Console.WriteLine("Functions are declared as 'fun {IDENTIFIDER}({PARAMS}){ return value; }'");
                                Console.WriteLine("Functions are views as a type and can be assigned to variables as expected");
                                return false;
                            case "b":
                            case "branch":
                                Console.WriteLine("Supported branch logics are if, while, for");
                                Console.WriteLine("Functions are views as a type and can be assigned to variables as expected");
                                return false;
                            case "s":
                            case "std":
                            case "standard":
                                Console.WriteLine("Standard functions include:");
                                Console.WriteLine("print(var value): Prints a value to the console");
                                Console.WriteLine("clock(): returns the current runtime");
                                return false;
                            case "g":
                            case "grammar":
                                Console.WriteLine("" +
                                    "program \t\t\t-> line* EOF\r\n" +
                                    "line \t\t\t\t-> declaration | statement\r\n" +
                                    "declaration \t\t\t-> variable_declaration_statement | function_declaration_statement\r\n" +
                                    "variable_declaration_statement \t-> \"var\" IDENTIFIER(\"=\" expression)?\";\"\r\n" +
                                    "function_declaration_statement \t-> \"fun\" function\r\n" +
                                    "function \t\t\t-> IDENTIFIER \"(\"parameters?\")\" block_statement\r\n" +
                                    "parameters \t\t\t-> IDENTIFIER ( \",\" IDENTIFIER )*\r\n" +
                                    "statement \t\t\t-> expression_statement | for_statement | if_statement | while_statement | block_statement | return_statement\r\n" +
                                    "for_statement \t\t\t-> \"for\" \"(\" ( variable_declaration_statement | expression_statement | \";\" ) expression? \";\" expression? \")\" statement\r\n" +
                                    "while_statement \t\t-> \"while\" \"(\" expression \")\" statement\r\n" +
                                    "if_statement \t\t\t-> \"if\" \"(\" expression \")\" statement ( \"else\" statement )?\r\n" +
                                    "block_statement \t\t-> \"{\" line* \"}\"\r\n" +
                                    "expression_statement \t\t-> expression \";\"\r\n" +
                                    "return_statement \t\t-> \"return\" expression?\";\"\r\n" +
                                    "expression \t\t\t-> assignment\r\n" +
                                    "assignment \t\t\t-> (IDENTIFIER \"=\" assignment) | logic_or\r\n" +
                                    "logic_or \t\t\t-> logic_and (\"or\" logic_and)*\r\n" +
                                    "logic_and \t\t\t-> equality (\"and\" equality)*\r\n" +
                                    "equality \t\t\t-> comparison ( ( \"!=\" | \"==\" ) comparison )*\r\n" +
                                    "comparison \t\t\t-> term ( ( \">\" | \">=\" | \"<\" | \"<=\" ) term )*\r\n" +
                                    "term \t\t\t\t-> factor ( ( \"-\" | \"+\" ) factor )*\r\n" +
                                    "factor \t\t\t\t-> unary ( ( \"/\" | \"*\" ) unary )*\r\n" +
                                    "unary \t\t\t\t-> ( \"!\" | \"-\" ) unary | function_call\r\n" +
                                    "function_call \t\t\t-> object_expression | (\"(\" arguments?\")\")*\r\n " +
                                    "arguments \t\t\t-> expression (\",\" expression)*\r\n" +
                                    "object_expression \t\t-> \"{\"declaration*\"}\" | primary\r\n" +
                                    "primary \t\t\t-> NUMBER | STRING | \"true\" | \"false\" | \"nil\" | \"(\" expression \")\" | IDENTIFIER | \"[\" arguments? \"]\"");
                                return false;
                        }
                        return false;

                    case "-r":
                    case "-run":
                        try { RunFile(args[1]); }
                        catch (Exception ex) { Console.WriteLine(ex); }
                        return false;
                }
                
            }
            return false;
        }
        private static void Run(string source)
        {
            //Tokenizer
            Tokenizer scanner = new Tokenizer();
            List<Token> tokens = scanner.Tokenize(source);

            if (PrintTokenizer)
                foreach (Token t in tokens)
                    Console.WriteLine($"{t.token} : {t.lexeme}");
          
            //Parsing
            Parser<Variable> parser = new Parser<Variable>(tokens);
            List<Statement<Variable>> statements = parser.Parse();

            // Stop if there was a compile syntax error.
            if (hadError) return;

            //Runtime Interpret
            interpreter.Interpret(statements);

        }
        private static void Report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        } 
        public static void Error(Token token, string message)
        {
            if (token.token == TokenEnum.EOF)
                Report(token.line, " at end", message);
            else
                Report(token.line, " at '" + token.lexeme + "'", message);
        }
        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }
    }
}
