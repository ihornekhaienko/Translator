using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Translator
{
    partial class Parser
    {
        public List<Lexeme> Lexemes { get; }
        public List<Lexeme> Idents { get; }
        public List<Lexeme> Literals { get; }
        public List<Lexeme> PostfixCode { get; }
        public List<Lexeme> Labels { get; }

        int position;
        int length;
        readonly List<string> TYPES = new List<string> { "bool", "int", "float", "text" };
        Lexeme current;
        int indent;
        int state;
        string[] types;
        string mes;
        bool inDeclaration = false;
        public Parser(string path)
        {
            List<List<Lexeme>> list = Misc.Deserialize(new List<List<Lexeme>> { Lexemes, Idents, Literals }, path);
            Lexemes = list[0];
            Idents = list[1];
            Literals = list[2];

            PostfixCode = new List<Lexeme>();
            Labels = new List<Lexeme>();
            position = 0;
            length = Lexemes.Count;
            indent = 0;
        }
        public void Parse()
        {
            try
            {
                Console.WriteLine(new string('-', 120) + "\n");
                ParseMain();

                //Console.WriteLine();

                Console.WriteLine(new string('-', 120));
                Misc.DisplayTranslator(this);
                Misc.DisplayPostfix(PostfixCode);
                //Console.WriteLine($"\nParser: Успішне завершення програми");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(new string('-', 120));
                Misc.DisplayTranslator(this);
                Misc.DisplayPostfix(PostfixCode);
                Console.WriteLine();
                Console.WriteLine(e.Message);
                throw new Exception($"\nParser: Аварійне завершення програми з кодом {state}");
            }
            finally
            {
                Misc.Serialize(new List<List<Lexeme>> { Lexemes, Idents, Literals, PostfixCode, Labels }, @"data\parser.dat");
            }
        }

        bool ParseMain()
        {
            ParseLexeme("main", Token.KEYWORD);
            ParseStatementList();

            return true;
        }

        bool ParseStatementList()
        {
            //Increment();
            //Console.WriteLine(new string(' ', indent) + "ParseStatementList:");
            //Increment();
            ParseLexeme("{", Token.CURLY_OP);

            while (ParseStatement())
            {
                continue;
            }

            //Decrement();
            return true;
        }

        bool ParseStatement()
        {
            //Increment();
            //Console.WriteLine(new string(' ', indent) + "ParseStatement:");

            current = Peek(0);

            if (current == null)
                throw GenerateException(202);

            if (TYPES.Contains(current.Text))
            {
                ParseDeclaration();
                //Decrement();
                return true;
            }
            if (current is Ident)
            {
                //Increment();
                //Console.WriteLine(new string(' ', indent) + "ParseIdentList:");
                //Increment();
                ParseIdentList();
                //Decrement();
                ParseAssign();
                ParseLexeme(";", Token.PUNCT);
                //Decrement();
                //Decrement();
                return true;
            }
            if (current.Text == "if")
            {
                ParseIf();
                //Decrement();
                return true;
            }
            if (current.Text == "for")
            {
                ParseFor();
                //Decrement();
                return true;
            }
            if (current.Text == "write")
            {
                ParseOutput();
                //Decrement();
                return true;
            }
            if (current.Text == "}")
            {
                //Decrement();
                ParseLexeme("}", Token.CURLY_OP);
                //Decrement();
                return false;
            }
            throw GenerateException(204);
        }
        void ParseDeclaration()
        {
            //Increment();
            //Console.WriteLine(new string(' ', indent) + "ParseDeclaration:");
            //Increment();
            types = new string[] { current.Text.ToString().ToUpper() };
            mes = types[0];

            ParseToken(current.Type);
            //Console.WriteLine(new string(' ', indent) + "ParseIdentList:");
            //Increment();
            inDeclaration = true;
            ParseIdentList();
            inDeclaration = false;
            //Decrement();

            if (Peek(0).Type == Token.ASSIGN_OP)
            {
                ParseAssign();
            }
            else
            {
                PostfixCode.Add(Peek(0));
            }

            ParseLexeme(";", Token.PUNCT);

            types = null;
            //Decrement();
            //Decrement();
        }
        bool ParseOutput()
        {
            //Increment();
            //Console.WriteLine(new string(' ', indent) + "ParseOutput:");
            //Increment();
            ParseLexeme("write", Token.KEYWORD);
            //Increment();
            //Console.WriteLine(new string(' ', indent) + "ParseOutputList:");
            //Increment();
            ParseOutputList();
            //Decrement();
            //Decrement();
            //Decrement();
            ParseLexeme(";", Token.PUNCT);
            PostfixCode.Add(new Lexeme(Token.KEYWORD, "write"));
            //Decrement();

            return true;
        }

        bool ParseOutputList()
        {
            ParseBoolExpression();
            current = Peek(0);

            if (current.Text == ",")
            {
                ParseLexeme(",", Token.PUNCT);
                PostfixCode.Add(new Lexeme(Token.KEYWORD, "write"));
                //Misc.Display(this);
                return ParseOutputList();
            }
            else
            {
                return true;
            }
        }
        bool ParseIdentList()
        {
            ParseToken(Token.ID);
            current = Peek(-1);

            if (inDeclaration)
            {
                if ((current as Ident).DataType != null)
                {
                    (current as Ident).CurrentLine--;
                    throw GenerateException(205);
                }
                else
                {
                    (current as Ident).DataType = (Token)Enum.Parse(typeof(Token), mes);
                }
            }

            PostfixCode.Add(current);
            //Misc.Display(this);
            current = Peek(0);

            if (current.Text == ",")
            {
                ParseLexeme(",", Token.PUNCT);
                return ParseIdentList();
            }
            else
            {
                return true;
            }
        }
        bool ParseAssign()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseAssign:");
            //Increment();
            ParseToken(Token.ASSIGN_OP);
            Lexeme op = Peek(-1);

            current = Peek(0);

            if (current.Text == "read")
            {
                ParseLexeme("read", Token.KEYWORD);
                PostfixCode.Add(Peek(-1));
                //Misc.Display(this);
            }
            else
            {
                ParseBoolExpression();
            }
            PostfixCode.Add(op);
            //Misc.Display(this);
            //Decrement();
            return true;
        }

        bool ParseBoolExpression()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseBoolExpression:");
            //Increment();
            ParseAddExpression();

            while (true)
            {
                current = Peek(0);

                if (current.Type == Token.LOGIC_OP)
                {
                    ParseToken(current.Type);
                    Lexeme op = Peek(-1);
                    ParseAddExpression();
                    PostfixCode.Add(op);
                    //Misc.Display(this);
                }
                else
                {
                    break;
                }
            }
            //Decrement();
            return true;
        }

        bool ParseAddExpression()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseAddExpression:");
            //Increment();
            ParseMultExpression();

            while (true)
            {
                current = Peek(0);

                if (current.Type == Token.ADD_OP)
                {
                    ParseToken(current.Type);
                    Lexeme op = Peek(-1);
                    ParseMultExpression();
                    PostfixCode.Add(op);
                    //Misc.Display(this);
                }
                else
                {
                    break;
                }
            }
            //Decrement();
            return true;
        }
        bool ParseMultExpression()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseMultExpression:");
            //Increment();
            ParsePowerExpression();

            while (true)
            {
                current = Peek(0);
                if (current.Type == Token.MULT_OP)
                {
                    ParseToken(current.Type);
                    Lexeme op = Peek(-1);
                    ParsePowerExpression();
                    PostfixCode.Add(op);
                    //Misc.Display(this);
                }
                else
                {
                    break;
                }
            }
            //Decrement();
            return true;
        }

        bool ParsePowerExpression()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseMultExpression:");
            //Increment();
            ParseSignedFactor();

            while (true)
            {
                current = Peek(0);
                if (current.Type == Token.POWER_OP)
                {
                    ParseToken(current.Type);
                    Lexeme op = Peek(-1);
                    ParsePowerExpression();
                    PostfixCode.Add(op);
                    //Misc.Display(this);
                }
                else
                {
                    break;
                }
            }
            //Decrement();
            return true;
        }

        bool ParseSignedFactor()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseSignedFactor:");
            //Increment();
            current = Peek(0);
            Lexeme op = null;

            if (current.Text == "-")
            {
                ParseLexeme("-", Token.ADD_OP);
                Peek(-1).Type = Token.SIGN_OP;
                op = Peek(-1);
            }

            ParseFactor();
            if (op != null)
                PostfixCode.Add(op);
            //Misc.Display(this);
            //Decrement();
            return true;
        }
        bool ParseFactor()
        {

            //Console.WriteLine(new string(' ', indent) + "ParseFactor:");
            //Increment();
            current = Peek(0);

            if (current is Ident || current is Literal)
            {
                ParseToken(current.Type);
                Lexeme op = Peek(-1);
                PostfixCode.Add(op);
                //Misc.Display(this);
                //Decrement();
                return true;
            }
            if (current.Text == "(")
            {
                ParseLexeme("(", Token.BRACKETS_OP);
                ParseBoolExpression();
                ParseLexeme(")", Token.BRACKETS_OP);
                //Decrement();
                return true;
            }
            throw GenerateException(203, "ідентифікатор чи літерал");
        }

        bool ParseIf()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseIf:");
            //Increment();

            ParseLexeme("if", Token.KEYWORD);

            ParseBoolExpression();
            //PostfixCode.Add(new Lexeme(Token.PUNCT, ";"));

            Label labelTrue, labelFalse;
            labelFalse = CreateLabel();
            PostfixCode.Add(labelFalse);
            PostfixCode.Add(new Lexeme(Token.JF, "jf"));


            ParseLexeme("then", Token.KEYWORD);

            ParseStatementList();
            ParseLexeme("fi", Token.KEYWORD);

            current = Peek(0);
            if (current.Text == "else")
            {
                ParseLexeme("else", Token.KEYWORD);
                labelTrue = CreateLabel();
                PostfixCode.Add(labelTrue);
                PostfixCode.Add(new Lexeme(Token.JUMP, "jump"));

                InitializeLabel(ref labelFalse);
                PostfixCode.Add(labelFalse);
                PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));

                current = Peek(0);
                ParseStatementList();

                InitializeLabel(ref labelTrue);
                PostfixCode.Add(labelTrue);
                PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));
            }
            else
            {
                InitializeLabel(ref labelFalse);
                PostfixCode.Add(labelFalse);
                PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));
            }
            //Decrement();
            return true;
        }

        bool ParseFor()
        {
            //Console.WriteLine(new string(' ', indent) + "ParseFor:");
            //Increment();

            ParseLexeme("for", Token.KEYWORD);

            ParseToken(Token.ID);
            PostfixCode.Add(Peek(-1));
            ParseAssign();

            Label m1, m2, m3;
            m1 = CreateLabel();
            m2 = CreateLabel();
            m3 = CreateLabel();

            Ident isInitial;

            isInitial = new Ident(Token.ID, "System.isInitial");
            isInitial.DataType = Token.BOOL;

            PostfixCode.Add(isInitial);
            PostfixCode.Add(new Literal(Token.BOOL, "true"));
            PostfixCode.Add(new Lexeme(Token.ASSIGN_OP, "="));

            InitializeLabel(ref m1);
            PostfixCode.Add(m1);
            PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));
            PostfixCode.Add(isInitial);
            PostfixCode.Add(new Literal(Token.BOOL, "false"));
            PostfixCode.Add(new Lexeme(Token.LOGIC_OP, "=="));
            PostfixCode.Add(m2);
            PostfixCode.Add(new Lexeme(Token.JF, "jf"));

            ParseLexeme("by", Token.KEYWORD);
            ParseToken(Token.ID);
            PostfixCode.Add(Peek(-1));
            ParseAssign();

            InitializeLabel(ref m2);
            PostfixCode.Add(m2);
            PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));
            PostfixCode.Add(isInitial);
            PostfixCode.Add(new Literal(Token.BOOL, "false"));
            PostfixCode.Add(new Lexeme(Token.ASSIGN_OP, "="));

            PostfixCode.Add(new Literal(Token.BOOL, "true"));
            //to
            ParseLexeme("while", Token.KEYWORD);
            ParseBoolExpression();

            PostfixCode.Add(new Lexeme(Token.LOGIC_OP, "=="));
            PostfixCode.Add(m3);
            PostfixCode.Add(new Lexeme(Token.JF, "jf"));

            ParseLexeme("do", Token.KEYWORD);
 
            ParseStatementList();

            PostfixCode.Add(m1);
            PostfixCode.Add(new Lexeme(Token.JUMP, "jump"));

            InitializeLabel(ref m3);
            PostfixCode.Add(m3);
            PostfixCode.Add(new Lexeme(Token.COLON_OP, ":"));

            //Decrement();
            return true;
        }

        bool ParseLexeme(string lexeme, Token token)
        {
            try
            {
                ParseToken(token);

                if (current.Text == lexeme)
                {
                    return true;
                }
                else
                {
                    throw GenerateException(203, lexeme);
                }
            }
            catch
            {
                if (lexeme == "main")
                    throw GenerateException(201);
                else
                    throw;
            }
        }
        bool ParseToken(Token token)
        {
            current = Peek(0);
            if (current == null)
                throw GenerateException(202);

            if (current.Type == token)
            {
                //Console.WriteLine(new string(' ', indent) + $"{current.LineNumber}: ({current.Type}, {current.Text})");
                MoveNext();
                return true;
            }
            else
            {
                throw GenerateException(203, token.ToString());
            }
        }

        Exception GenerateException(int state, string message = null)
        {
            this.state = state;
            return state switch
            {
                201 => new Exception($"Неочікуваний кінець програми: не знайдено точку входу в програму"),
                202 => new Exception($"Неочікуваний кінець програми: вихід за межі таблиці розбору"),
                203 => new Exception($"Неочікуваний елемент в {current.LineNumber} рядку: очікувався {message}, а не ({current.Type} \'{current.Text}\')"),
                204 => new Exception($"Незакритий блок інструкцій в {current.LineNumber} рядку"),
                205 => new Exception($"Спроба повторного оголошення змінної {current.Text} в {current.LineNumber} рядку"),
                206 => new Exception($"Спроба повторного оголошення мітки"),
                _ => null,
            };
        }

        void Increment() { indent += 4; }
        void Decrement() { indent -= 4; }

        Lexeme Peek(int relativePosition)
        {
            int pos = position + relativePosition;
            if (pos >= length)
                return null;
            return Lexemes[pos];
        }

        Lexeme MoveNext()
        {
            position++;

            return Peek(0);
        }

        Label CreateLabel()
        {
            Label label = new Label(Token.LABEL, "l" + Labels.Count.ToString());

            if (Misc.IndexOf(label, Labels) != -1)
            {
                throw GenerateException(206);
            }

            if (Misc.IndexOf(label, Idents) != -1)
            {
                throw GenerateException(205);
            }

            Labels.Add(label);

            return label;
        }

        void InitializeLabel(ref Label label)
        {
            label.Value = PostfixCode.Count;
        }
    }
}
