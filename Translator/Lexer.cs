using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Translator
{
    class Lexer
    {
        readonly string program;
        readonly int length;
        int position;
        int lineNumber;
        int state;
        string buff;

        public List<Lexeme> Lexemes { get; }
        public List<Lexeme> Idents { get; }
        public List<Lexeme> Literals { get; }

        readonly Dictionary<string, Token> LEXEMES_TABLE = new Dictionary<string, Token>
        {
            ["main"] = Token.KEYWORD,
            ["for"] = Token.KEYWORD, ["by"] = Token.KEYWORD, ["while"] = Token.KEYWORD, ["do"] = Token.KEYWORD,
            ["if"] = Token.KEYWORD, ["then"] = Token.KEYWORD, ["fi"] = Token.KEYWORD, ["else"] = Token.KEYWORD,
            ["int"] = Token.KEYWORD, ["float"] = Token.KEYWORD,  ["text"] = Token.KEYWORD, ["bool"] = Token.KEYWORD,
            ["read"] = Token.KEYWORD, ["write"] = Token.KEYWORD,
            ["true"] = Token.BOOL, ["false"] = Token.BOOL,
            ["="] = Token.ASSIGN_OP,
            ["+"] = Token.ADD_OP, ["-"] = Token.ADD_OP,
            ["*"] = Token.MULT_OP,["/"] = Token.MULT_OP, ["%"] = Token.MULT_OP,
            ["^"] = Token.POWER_OP,  ["("] = Token.BRACKETS_OP, [")"] = Token.BRACKETS_OP,
            [";"] = Token.PUNCT, [","] = Token.PUNCT, 
            ["{"] = Token.CURLY_OP, ["}"] = Token.CURLY_OP,
            [">"] = Token.LOGIC_OP, ["<"] = Token.LOGIC_OP, ["!="] = Token.LOGIC_OP, ["=="] = Token.LOGIC_OP,  [">="] = Token.LOGIC_OP, ["<="] = Token.LOGIC_OP
        };

        readonly Dictionary<(int, string), int> STF = new Dictionary<(int, string), int>
        {
            [(0, "ws")] = 0, [(0, "eof")] = 0,
            [(0, "eol")] = 1,
            [(0, ";")] = 2, [(0, ",")] = 2, [(0, "{")] = 2, [(0, "}")] = 2,

            [(0, "letter")] = 10, [(10, "letter")] = 10, [(10, "digit")] = 10, [(10, "_")] = 10, [(10, "other")] = 11,

            [(0, "digit")] = 20, [(20, "digit")] = 20, [(20, ".")] = 21, [(20, "other")] = 22, [(21, "digit")] = 21, [(21, "other")] = 23,

            [(0, "\'")] = 30, [(30, "other")] = 30, [(30, "\'")] = 31, [(30, "eof")] = 102,

            [(0, "=")] = 40, [(0, ">")] = 41, [(0, "<")] = 41,
            [(0, "!")] = 42, [(42, "other")] = 103, [(42, "=")] = 44,
            [(40, "=")] = 44, [(41, "=")] = 44,
            [(40, "other")] = 43, [(41, "other")] = 43,

            [(0, "op")] = 50,

            [(0, "other")] = 101
        };


        readonly int[] FINAL_STATES = new int[] {1, 2, 11, 22, 23, 31, 43, 44, 50, 101, 102, 103 }; 
        readonly int[] SPECIAL_STATES = new int[] { 2, 31, 44, 50 };
        readonly int[] ERROR_STATES = new int[] { 101, 102, 103};

        public Lexer(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                program = reader.ReadToEnd() + "\0";
            }
            length = program.Length;
            position = 0;
            lineNumber = 1;
            state = 0;
            buff = string.Empty;

            Lexemes = new List<Lexeme>();
            Idents = new List<Lexeme>();
            Literals = new List<Lexeme>();
        }

        public void Lex()
        {
            try
            {
                Console.WriteLine("Таблиця розбору:");
                Console.WriteLine($"{"№ рядка",15}|{"Лексема",15}|{"Токен",15}|\n" + new string('-', 48));
                while (position < length)
                {
                    char current = Peek(0);
                    string classOfChar = ClassOfChar(current);
                    state = NextState(classOfChar);

                    if (IsFinal())
                    {

                        FinalStateHandling();

                        if (state != 1)
                            Console.WriteLine($"{lineNumber,15}|{Lexemes.Last().Text,15}|{Lexemes.Last().Type,15}|");

                        state = 0;
                        buff = string.Empty;
                    }
                    else if (state == 0)
                    {
                        buff = string.Empty;
                        MoveNext();
                    }
                    else
                    {
                        buff += current;
                        MoveNext();
                    }
                }
                Console.WriteLine();
                Misc.Display(this);
                Console.WriteLine("\nLexer: Лексичний аналіз успішно завершився");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Misc.Display(this);
                Console.WriteLine();
                Console.WriteLine(e.Message);

                throw new Exception($"\nLexer: Аварійне завершення програми з кодом {state}");


            }
            finally
            {

                Misc.Serialize(new List<List<Lexeme>> { Lexemes, Idents, Literals }, @"data\lexer.dat");
            }
        }

        void FinalStateHandling()
        {
            if (ERROR_STATES.Contains(state))
            {
                Throw();
                return;
            }

            if (state == 1)
            {
                lineNumber++;
                MoveNext();
                return;
            }

            if (SPECIAL_STATES.Contains(state))
            {
                buff += Peek(0);
                MoveNext();
            }
   
            Lexemes.Add(Lexemize());
        }

        Lexeme Lexemize()
        {
            Token type = GetToken();
            if ((int)type >= 0 && (int)type <= 4)
            {
                return AddToList(type);
            }
            else
            {
                return new Lexeme(type, buff, lineNumber);
            }
        }

        Lexeme AddToList(Token type)
        {
            if ((int)type == 0)
            {
                Ident id = new Ident(type, buff);
                int idx = Misc.IndexOf(id, Idents);
                if (idx != -1)
                {
                    Idents[idx].LineNumber = lineNumber;
                    return Idents[idx];
                }
                else
                {
                    id.Index = Idents.Count;
                    id.LineNumber = lineNumber;
                    Idents.Add(id);
                    return id;
                }
            }
            else
            {
                Literal lit = new Literal(type, buff);
                int idx = Misc.IndexOf(lit, Literals);
                if (idx != -1)
                {
                    Literals[idx].LineNumber = lineNumber;
                    return Literals[idx];
                }
                else
                {
                    lit.Index = Literals.Count;
                    lit.LineNumber = lineNumber;
                    Literals.Add(lit);
                    return lit;
                }
            }
        }
        Token GetToken()
        {
            try
            {
                return LEXEMES_TABLE[buff];
            }
            catch (Exception)
            {
                switch (state)
                {
                    case 11:
                        return Token.ID;
                    case 22:
                        return Token.INT;
                    case 23:
                        return Token.FLOAT;
                    case 31:
                        return Token.TEXT;
                    default:
                        throw;
                }
            }
        }

        void Throw()
        {
            string current = Peek(0).ToString();
            if (current == '\r'.ToString() || current == '\n'.ToString())
            {
                current = "\\n";
            }
            switch (state)
            {
                case 101:
                    throw new Exception($"у рядку {lineNumber} неочікуваний символ \'{current}\'");

                case 102:
                    throw new Exception($"Незакриті лапки рядка символів, що почався в {lineNumber} рядку");

                case 103:
                    throw new Exception($"у рядку {lineNumber} очікувався символ \'=\', а не \'{current}\'");
            }
        }

        bool IsFinal() => FINAL_STATES.Contains(state);

        string ClassOfChar(char symbol)
        {
            if (char.IsDigit(symbol))
                return "digit";
            else if (@"+-*/%^()".Contains(symbol))
                return "op";
            else if (Regex.IsMatch(symbol.ToString(), @"^[a-zA-z]+$"))
                return "letter";
            else if (@".';,{}!=<>".Contains(symbol))
                return symbol.ToString();
            else if (" \t\r".Contains(symbol))
                return "ws";
            else if (symbol == '\n')
                return "eol";
            else if (symbol == '\0')
                return "eof";
            return "other";
        }

        int NextState(string classOfChar)
        {
            try
            {
                return STF[(state, classOfChar)];
            }
            catch (Exception)
            {
                return STF[(state, "other")];
            }
        }

        char Peek(int relativePosition)
        {
            int pos = position + relativePosition;
            if (pos >= length)
                return '\0';
            return program[pos];
        }

        char MoveNext()
        {
            position++;
            return Peek(0);
        }
    }
}
