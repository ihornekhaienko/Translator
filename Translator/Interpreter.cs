using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Translator
{
    class Interpreter
    {
        public List<Lexeme> Lexemes { get; }
        public List<Lexeme> Idents { get; }
        public List<Lexeme> Literals { get; }
        public List<Lexeme> PostfixCode { get; }
        public List<Lexeme> Labels { get; }
        public List<Lexeme> CommandTrack { get; }
        Stack<Operand> Operands { get; }

        readonly int length;
        Lexeme current;
        int position;
        readonly List<Token> ARIPH_OPS = new List<Token> { Token.MULT_OP, Token.POWER_OP, Token.ADD_OP };
        int state;
        string output = string.Empty;
        public Interpreter(string path)
        {
            List<List<Lexeme>> list = Misc.Deserialize(new List<List<Lexeme>> { Lexemes, Idents, Literals, PostfixCode, Labels }, path);
            Lexemes = list[0];
            Idents = list[1];
            Literals = list[2];
            PostfixCode = list[3];
            Labels = list[4];
            
            Operands = new Stack<Operand>();
            CommandTrack = new List<Lexeme>();
            length = PostfixCode.Count;
            position = 0;
        }

        public void Interpret()
        {
            try
            {
                Console.WriteLine("Вивід у консоль:");
                //Misc.Display(this);
                for (position = 0; position < length; position++)
                {
                    current = PostfixCode[position];

                    if (current.Text == "read" && current.Type == Token.KEYWORD)
                    {
                        current = new Literal(Token.KEYWORD, current.Text, current.LineNumber);
                    }
                    if (current is Operand oper)
                    {
                        if (oper is Ident id && id.DataType == null)
                        {
                            throw GenerateException(301, id.Text);
                        }
                        Operands.Push(oper);
                    }
                    else
                    {
                        InterpretOp();
                    }

                    //Display(position);
                }
                //Console.WriteLine("Вивід у консоль:");
                //Console.WriteLine(output);
                Console.WriteLine();
                Misc.Display(this);
                Console.WriteLine(new string('-', 120));
                Console.WriteLine($"\nInterpreter: Успішне завершення програми");

            }
            catch (Exception e)
            {
                //Console.WriteLine(output);
                Console.WriteLine();
                Misc.Display(this);
                Console.WriteLine(new string('-', 120));
                Console.WriteLine(e.Message);
                throw new Exception($"\nInterpreter: Аварійне завершення програми з кодом {state}");
            }
        }

        void InterpretOp()
        {
            if (current.Type == Token.ASSIGN_OP)
            {
                InterpretAssign();
            }
            else if (current.Type == Token.LOGIC_OP)
            {
                InterpretLogic();
            }
            else if (current.Type == Token.SIGN_OP)
            {
                InterpretUnary();
            }
            else if (ARIPH_OPS.Contains(current.Type))
            {
                InterpretArith();
            }
            else if (current.Text == "write")
            {
                InterpretOutput();
            }
            else if (current.Type == Token.PUNCT)
            {
                Operands.Clear();
            }
            else if (current.Type == Token.JF)
            {
                InterpretJF();
            }
            else if (current.Type == Token.JUMP)
            {
                CommandTrack.Add(PostfixCode[position - 1]);
                CommandTrack.Add(current);
                InterpretJump(PostfixCode[position - 1]);
            }  
        }

        void InterpretJF()
        {
            Operand condition = Operands.Pop();

            if (condition.Value == false)
            {
                CommandTrack.Add(PostfixCode[position - 1]);
                CommandTrack.Add(current);

                InterpretJump(PostfixCode[position - 1]);
            }
        }

        void InterpretJump(Lexeme label)
        {
            try
            {
                for (int i = 0; i < PostfixCode.Count; i++)
                {
                    if (PostfixCode[i] == label && PostfixCode[i + 1].Type == Token.COLON_OP)
                    {
                        CommandTrack.Add(PostfixCode[i]);
                        CommandTrack.Add(PostfixCode[i + 1]);

                        position = i + 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void InterpretUnary()
        {
            Operand right = Operands.Pop();

            if (right.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                right = new Literal(GetType(read), read, right.LineNumber);
            }

            if (right.DataType != Token.INT && right.DataType != Token.FLOAT)
            {
                throw GenerateException(303, $"({right.Type} {right.Text}),  очікувався INT або FLOAT, а не {right.DataType}");
            }

            CommandTrack.Add(right);
            CommandTrack.Add(current);

            dynamic value = -right.Value;
            Operands.Push(new Literal(GetType(Convert.ToString(value)), Convert.ToString(value)));
            AddToTable(Literals, Operands.Peek());
        }
        void InterpretArith()
        {
            Operand right = Operands.Pop();
            if (right.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                right = new Literal(GetType(read), read, right.LineNumber);
            }
            if (right.Value == null)
            {
                throw GenerateException(302, right.Text);
            }

            if (right.DataType != Token.INT && right.DataType != Token.FLOAT)
            {
                throw GenerateException(303, $"({right.Type} {right.Text}),  очікувався INT або FLOAT, а не {right.DataType}");
            }
            Operand left = Operands.Pop();
            if (left.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                left = new Literal(GetType(read), read, left.LineNumber);
            }
            if (left.Value == null)
            {
                throw GenerateException(302, left.Text);
            }
            if (left.DataType != Token.INT && left.DataType != Token.FLOAT)
            {
                throw GenerateException(303, $"({left.Type} {left.Text}),  очікувався INT або FLOAT, а не {left.DataType}");
            }
            Calculate(right, left);


        }
        void CalculateLogicBool(Operand right, Operand left)
        {
            dynamic value = null;

            switch (current.Text)
            {
                case "==":
                    if (left.Value == right.Value)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    break;
                case "!=":
                    if (left.Value != right.Value)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    break;
                case ">":
                    if (left.Value == right.Value)
                    {
                        value = false;
                    }
                    else if (left.Value == true)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    break;
                case "<":
                    if (left.Value == right.Value)
                    {
                        value = false;
                    }
                    else if(left.Value == true)
                    {
                        value = false;
                    }
                    else
                    {
                        value = true;
                    }
                    break;
                case ">=":
                    if (left.Value == right.Value)
                    {
                        value = true;
                    }
                    else if(left.Value == true)
                    {
                        value = true;
                    }
                    else
                    {
                        value = false;
                    }
                    break;
                case "<=":
                    if (left.Value == right.Value)
                    {
                        value = true;
                    }
                    else if(left.Value == true)
                    {
                        value = false;
                    }
                    else
                    {
                        value = true;
                    }
                    break;
            }

            if (Convert.ToString(value) == "NaN")
                throw GenerateException(306);

            CommandTrack.Add(left);
            CommandTrack.Add(right);
            CommandTrack.Add(current);

            Operands.Push(new Literal(GetType(Convert.ToString(value)), Convert.ToString(value)));
            AddToTable(Literals, Operands.Peek());
        }
        void CalculateLogicArith(Operand right, Operand left)
        {
            dynamic value = null;
            switch (current.Text)
            {
                case "==":
                    if (left.Value == right.Value)
                        value = true;
                    else
                        value = false;
                    break;
                case "!=":
                    if (left.Value != right.Value)
                        value = true;
                    else
                        value = false;
                    break;
                case ">":
                    if (left.Value > right.Value)
                        value = true;
                    else
                        value = false;
                    break;
                case "<":
                    if (left.Value < right.Value)
                        value = true;
                    else
                        value = false;
                    break;
                case ">=":
                    if (left.Value >= right.Value)
                        value = true;
                    else
                        value = false;
                    break;
                case "<=":
                    if (left.Value <= right.Value)
                        value = true;
                    else
                        value = false;
                    break;
            }

            if (Convert.ToString(value) == "NaN")
                throw GenerateException(306);

            CommandTrack.Add(left);
            CommandTrack.Add(right);
            CommandTrack.Add(current);

            Operands.Push(new Literal(GetType(Convert.ToString(value)), Convert.ToString(value)));
            AddToTable(Literals, Operands.Peek());
        }
        void InterpretOutput()
        {
            Operand op = Operands.Pop();
            if (op.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                op = new Literal(GetType(read), read, op.LineNumber);
            }

            if (op.Value == null)
                throw GenerateException(302, op.Text);
            if (op.Type == Token.TEXT)
            {
                //output += op.Value.Replace(@"\n", "\n");
                Console.Write(op.Value.Replace(@"\n", "\n").Replace(@"\t", "\t"));
            }
            else
            {
                //output += op.Value;
                Console.Write(op.Value);
            }

            CommandTrack.Add(op);
            CommandTrack.Add(current);
        }

        void Calculate(Operand right, Operand left)
        {
            dynamic value = null;
            switch (current.Text)
            {
                case "+":
                    value = left.Value + right.Value;
                    break;
                case "-":
                    value = left.Value - right.Value;
                    break;
                case "*":
                    value = Convert.ToDouble(left.Value) * Convert.ToDouble(right.Value);
                    break;
                case "/":
                    if (right.Value == 0)
                    {
                        throw GenerateException(305, $"({left.Type} {left.Text}) / ({right.Type} {right.Text})");
                    }
                    value = Convert.ToDouble(left.Value) / Convert.ToDouble(right.Value);
                    break;
                case "%":
                    if (left.DataType == Token.FLOAT)
                    {
                        throw GenerateException(303, $"({left.Type} {left.Text}),  очікувався INT, а не {left.DataType}");
                    }
                    if (right.DataType == Token.FLOAT)
                    {
                        throw GenerateException(303, $"({right.Type} {right.Text}),  очікувався INT, а не {right.DataType}");
                    }
                    value = left.Value % right.Value;
                    break;
                case "^":
                    if (left.Value == 0 && right.Value < 0)
                    {
                        throw GenerateException(305, $"1 / {0}^{right.Value * -1}");
                    }
                    value = Math.Pow(Convert.ToDouble(left.Value), Convert.ToDouble(right.Value));
                    break;
                case "==":
                    value = left.Value == right.Value;
                    break;
                case "!=":
                    value = left.Value != right.Value;
                    break;
                case ">":
                    value = left.Value > right.Value;
                    break;
                case "<":
                    value = left.Value < right.Value;
                    break;
                case ">=":
                    value = left.Value >= right.Value;
                    break;
                case "<=":
                    value = left.Value <= right.Value;
                    break;
            }

            if (Convert.ToString(value) == "NaN")
                throw GenerateException(306);

            CommandTrack.Add(left);
            CommandTrack.Add(right);
            CommandTrack.Add(current);

            Operands.Push(new Literal(GetType(Convert.ToString(value)), Convert.ToString(value)));
            AddToTable(Literals, Operands.Peek());
        }
        void InterpretType(Operand left, Operand right)
        {
            if ((right.DataType == Token.FLOAT && left.DataType == Token.INT) || (left.DataType == Token.FLOAT && right.DataType == Token.INT))
            {
                return;
            }

            if (right.DataType != left.DataType)
            {
                throw GenerateException(304, $"({left.DataType} {left.Text}) і ({right.DataType} {right.Text})");
            }
        }
        void InterpretLogic()
        {
            Operand right = Operands.Pop();
            if (right.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                right = new Literal(GetType(read), read, right.LineNumber);
            }
            if (right.Value == null)
            {
                throw GenerateException(302, right.Text);
            }

            Operand left = Operands.Pop();
            if (left.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                left = new Literal(GetType(read), read, left.LineNumber);
            }
            if (left.Value == null)
            {
                throw GenerateException(302, left.Text);
            }


            InterpretType(left, right);
            if (right.Type == Token.BOOL)
            {
                CalculateLogicBool(right, left);
            }
            else
            {
                CalculateLogicArith(right, left);
            }

        }

        void InterpretAssign()
        {
            Operand right = Operands.Pop();
            if (right.Type == Token.KEYWORD)
            {
                Console.Write(">");
                string read = Console.ReadLine();
                right = new Literal(GetType(read), read, right.LineNumber);
            }
            if (right.Value == null)
                throw GenerateException(302, right.Text);

            List<Operand> identList = new List<Operand>();
            while (Operands.Count > 0)
            {
                identList.Add(Operands.Pop());
            }

            for (int i = 0; i < identList.Count; i++)
            {
                CommandTrack.Add(identList[i]);

                identList[i].Value = Cast(identList[i], right);
            }

            CommandTrack.Add(right);
            CommandTrack.Add(current);
        }

        dynamic Cast(Operand left, Operand right)
        {
            if (right.Type == Token.KEYWORD)
            {
                Console.WriteLine("ты путин");
                Console.Write(">");
                string read = Console.ReadLine();
                if (left.Type == Token.TEXT)
                {
                    right = new Literal(Token.TEXT, read, right.LineNumber);
                }
                else
                {
                    right = new Literal(GetType(read), read, right.LineNumber);
                }
            }

            InterpretType(left, right);

            if (left.DataType == right.DataType)
            {
                return right.Value;
            }

            if (left.DataType == Token.INT)
            {
                return (int)right.Value;
            }
            else if (left.DataType == Token.FLOAT)
            {
                return Convert.ToDouble(right.Value);
            }
            else if (left.DataType == Token.BOOL)
            {
                return Convert.ToBoolean(right.Value);
            }
            else
            {
                return Convert.ToString(right.Value);
            }
        }
        Token GetType(string read)
        {
            if (double.TryParse(read, out double res))
            {
                return Token.FLOAT;
            }
            else if (read.ToLower() == "true" || read.ToLower() == "false")
            {
                return Token.BOOL;
            }
            else
            {
                return Token.TEXT;
            }
        }

        void AddToTable(List<Lexeme> list, Lexeme lexeme)
        {
            int idx = Misc.IndexOf(lexeme, list);
            if (idx == -1)
            {
                (lexeme as Operand).Index = Literals.Count;
                list.Add(lexeme);
            }

        }

        Exception GenerateException(int state, string message = null)
        {
            this.state = state;
            return state switch
            {
                301 => new Exception($"Неоголошена змінна: {message}"),
                302 => new Exception($"Неініціалізована змінна: {message}"),
                303 => new Exception($"Недопустимий тип в {message}"),
                304 => new Exception($"Несумісність типів: {message}"),
                305 => new Exception($"Ділення на нуль: {message}"),
                306 => new Exception($"Результат не існує"),
                _ => null,
            };
        }

        void Display(int index)
        {
            int count = PostfixCode.Count - index;
            Console.WriteLine($"Крок інтерпретації: {position}");
            string l;
            if (current is Operand op)
            {
                string dataType = op.DataType != null ? op.DataType.ToString() : "undefined";
                string value = op.Value != null ? op.Value.ToString() : "null";
                l = $"({op.Text}, {dataType}, {value})";
            }
            else
            {
                l = $"({current.Type}, \'{current.Text}\')";
            }
            Console.WriteLine($"Лексема: {l}");
            Misc.DisplayPostfix(PostfixCode.GetRange(index, count));
            Misc.Display(Operands);
            Console.WriteLine();
        }
    }
}
