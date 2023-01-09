using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Translator
{
    [Serializable]
    enum Token
    {
        ID,
        INT,
        FLOAT,
        BOOL,
        TEXT,
        KEYWORD,
        LABEL,
        JUMP,
        JF,
        COLON_OP,
        ASSIGN_OP,
        ADD_OP,
        MULT_OP,
        POWER_OP,
        LOGIC_OP,
        SIGN_OP,
        BRACKETS_OP,
        CURLY_OP,
        PUNCT
    }

    [Serializable]
    class Lexeme
    {
        public Token Type { get; set; }
        public string Text { get; set; }
        public virtual int LineNumber { get; set; }
        public Lexeme() { }
        public Lexeme(Token type, string text)
        {
            Type = type;
            Text = text;
        }
        public Lexeme(Token type, string text, int lineNumber)
        {
            Type = type;
            Text = text;
            LineNumber = lineNumber;
        }
        public override string ToString()
        {
            return $"{this.LineNumber,15}|{this.Text,15}|{this.Type,15}|";
        }
    }
    [Serializable]
    class Operand : Lexeme
    {
        public int Index { get; set; }

        List<int> lines = new List<int>();

        public int CurrentLine { get; set; }

        public override int LineNumber
        {
            get
            {
                try
                {
                    return lines[CurrentLine++];
                }
                catch
                {
                    CurrentLine = 0;
                    return lines[CurrentLine++];
                }
            }

            set
            {
                lines.Add(value);
            }
        }

        public void MoveNext()
        {
            CurrentLine++;
        }

        public dynamic Value { get; set; }
        public Token? DataType { get; set; } = null;

        public Operand(Token type, string text) : base(type, text) { }

        public Operand(Token type, string text, int lineNumber) : base(type, text, lineNumber) { }
    }

    [Serializable]
    class Ident : Operand
    {  
        public Ident(Token type, string text) : base(type, text)
        {
            CurrentLine = 0;
            Value = null;
        }

        public override string ToString()
        {
            string dataType = DataType != null ? DataType.ToString() : "undefined";
            string value;
            try
            {
                value = Value != null ? (this.DataType == Token.FLOAT ? Value.ToString("0.00000") : Value.ToString()) : "null";
            }
            catch
            {
                value = Value.ToString();
            }

            return $"{this.Index,15}|{this.Text,15}|{this.Type,15}|{dataType,15}|{value, 15}|";
        }
    }

    [Serializable]
    class Label : Lexeme
    {
        public int? Value { get; set; } = null;

        public Label(Token type, string text) : base(type, text) { }

        public override string ToString()
        {
            string value = this.Value == null ? "undefined" : this.Value.ToString();
            return $"{this.Text,15}|{value,15}|";
        }
    }

    [Serializable]
    class Literal : Operand
    {
        public Literal(Token type, string text) : base(type, text)
        {
            DataType = type;
            switch (type)
            {
                case Token.INT:
                    {
                        Value = Convert.ToDouble(text);
                        break;
                    }
                case Token.FLOAT:
                    {
                        Value = Convert.ToDouble(text);
                        break;
                    }
                case Token.BOOL:
                    {
                        Value = Convert.ToBoolean(text);
                        break;
                    }
                case Token.TEXT:
                    {
                        Value = text.Trim('\'');
                        break;
                    }
            }
        }
        public Literal(Token type, string text, int lineNumber) : base(type, text, lineNumber)
        {
            DataType = type;
            switch (type)
            {
                case Token.INT:
                    {
                        if (text.Contains(","))
                        {
                            Value = Convert.ToDouble(text);
                        }
                        else
                            Value = Convert.ToDouble(text, new CultureInfo("en-US"));
                        break;
                    }
                case Token.FLOAT:
                    {
                        if (text.Contains(","))
                        {
                            Value = Convert.ToDouble(text);
                        }
                        else
                            Value = Convert.ToDouble(text, new CultureInfo("en-US"));
                        break;
                    }
                case Token.BOOL:
                    {
                        Value = Convert.ToBoolean(text);
                        break;
                    }
                case Token.TEXT:
                    {
                        Value = text.Trim('\'');
                        break;
                    }
            }
        }

        public override string ToString()
        {
            string value = this.DataType == Token.FLOAT ? Value.ToString("0.00000") : Value.ToString();
            return $"{this.Index,15}|{this.DataType,15}|{value,15}|";
        }
    }
}
