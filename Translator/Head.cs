using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Antlr4.Runtime;

namespace Translator
{
    class Head
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

            try
            {
                CompilerRun(args[0]);
                Console.WriteLine(new string('-', 120));
                Console.WriteLine(new string('-', 120));
                InterpreterRun();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void AntlrRun(string path)
        {
            /*using (StreamReader reader = new StreamReader(path))
            {
                AntlrInputStream inputStream = new AntlrInputStream(reader);

                PL17Lexer lexer = new PL17Lexer(inputStream);
                CommonTokenStream cts = new CommonTokenStream(lexer);
                PL17Parser parser = new PL17Parser(cts);

                parser.BuildParseTree = true;
                PL17BaseListener listener = new PL17BaseListener();
            }*/


        }

        static void LexerRun(string sourcePath)
        {
            Lexer lexer = new Lexer(sourcePath);

            lexer.Lex();
        }

        static void ParserRun(string lexerPath)
        {
            Parser parser = new Parser(lexerPath);
            parser.Parse();
        }

        static void CompilerRun(string sourcePath)
        {
            try
            {
                Console.WriteLine($"Компіляція...\n");
                LexerRun(sourcePath);
                //Console.WriteLine(new string('-', 120));
                ParserRun(@"data\lexer.dat");
                Console.WriteLine($"\nCompiler: Компіляція завершена успішно");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception($"\nCompiler: Компіляція аварійно завершена");
            }
        }

        static void InterpreterRun()
        {
            try
            {
                Console.WriteLine($"Інтерпретація...\n");
                Interpreter interpreter = new Interpreter(@"data\parser.dat");
                interpreter.Interpret();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception($"\nІнтерпретація аварійно завершена");
            }
        }
    }
}
