using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Translator
{
    static class Misc
    {
        public static void Display(Lexer lexer)
        {
            Console.WriteLine("Список ідентифікаторів:");
            Display(lexer.Idents, $"{"Індекс",15}|{"Лексема",15}|{"Токен",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 80));
            Console.WriteLine("Список літералів:");
            Display(lexer.Literals, $"{"Індекс",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 48));

        }
       public static void Display(Parser parser)
        {
            Console.WriteLine("Крок компіляції");
            Console.WriteLine($"Лексема: ({parser.PostfixCode[parser.PostfixCode.Count-1].Type}, {parser.PostfixCode[parser.PostfixCode.Count - 1].Text})");
            DisplayPostfix(parser.PostfixCode);
            Console.WriteLine();
        }
        public static void Display(Interpreter interpreter)
        {
            DisplayPostfix(interpreter.PostfixCode);
            Console.WriteLine();
            Console.WriteLine($"Кількість кроків: {interpreter.CommandTrack.Count}");
            DisplayCommandTrack(interpreter.CommandTrack);
            Console.WriteLine();
            Console.WriteLine("Список ідентифікаторів:");
            Display(interpreter.Idents, $"{"Індекс",15}|{"Лексема",15}|{"Токен",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 80));
            Console.WriteLine("Список літералів:");
            Display(interpreter.Literals, $"{"Індекс",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 48));
            Console.WriteLine("Список міток:");
            Display(interpreter.Labels, $"{"Мітка",15}|{"Значення",15}|\n" + new string('-', 32));
        }

        public static void DisplayTranslator(Parser parser)
        {
            Console.WriteLine("Список ідентифікаторів:");
            Display(parser.Idents, $"{"Індекс",15}|{"Лексема",15}|{"Токен",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 80));
            Console.WriteLine("Список літералів:");
            Display(parser.Literals, $"{"Індекс",15}|{"Тип даних",15}|{"Значення",15}|\n" + new string('-', 48));
            Console.WriteLine("Список міток:");
            Display(parser.Labels, $"{"Мітка",15}|{"Значення",15}|\n" + new string('-', 32));


        }
        public static void Display(IEnumerable list, string header = null)
        {
            Console.WriteLine(header);
            foreach (var i in list)
            {
                Console.WriteLine(i.ToString());
            }
            Console.WriteLine();
        }
        public static void DisplayCommandTrack(List<Lexeme> commandTrack)
        {
            Console.Write("Трек команд: [");
            string buff = string.Empty;
            foreach (Lexeme l in commandTrack)
            {
                buff += $"({l.Type}, {l.Text}), ";
            }
            Console.WriteLine(buff.Trim(' ').Trim(',') + "]");
        }
        public static void DisplayPostfix(List<Lexeme> postfixCode)
        {
            Console.Write("Постфіксний код: [");
            string buff = string.Empty;
            foreach(Lexeme l in postfixCode)
            {
                buff += $"({l.Type}, {l.Text}), ";
            }
            Console.WriteLine(buff.Trim(' ').Trim(',') + "]");
        }

        public static void Display(Stack<Operand> stack)
        {
            Console.Write("Стек: [");
            string buff = string.Empty;
            foreach (Operand o in stack.Reverse())
            {
                buff += $"({o.Type}, {o.Text}), ";
            }
            Console.WriteLine(buff.Trim(' ').Trim(',') + "]");
        }

        public static void Serialize(List<List<Lexeme>> list, string file)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(file, FileMode.Truncate))
            {
                formatter.Serialize(fs, list);
            }
        }

        public static List<List<Lexeme>> Deserialize(List<List<Lexeme>> list, string file)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
            {
                list = (List<List<Lexeme>>)formatter.Deserialize(fs);
            }

            return list;
        }

        public static int IndexOf(Lexeme lexeme, List<Lexeme> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (lexeme.Text == list[i].Text)
                    return i;
                if (lexeme.Type == Token.BOOL)
                {
                    if (lexeme.Text.ToLower() == list[i].Text.ToLower())
                        return i;
                }
            }
            return -1;
        }
    }
}
