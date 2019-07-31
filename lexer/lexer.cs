using System;
using System.Collections;
using System.Text;
using symbols;


namespace lexer
{
    public class Tag
    {
        public const int AND = 256,
            BASIC = 257, BREAK = 258, DO = 259,
            ELSE = 260, EQ = 261, FALSE = 262, GE = 263,
            ID = 264, IF = 265, INDEX = 266, LE = 267,
            MINUS = 268, NE = 269, NUM = 270, OR = 271,
            REAL = 272, TEMP = 273, TRUE = 274, WHILE = 275, SEQ = 276;
    }

    public class Token
    {
        public int tag;
        public Token(int t)
        {
            tag = t;
        }
        public override string ToString()
        {
            return "" + (Char)tag;
        }
    }

    public class Num : Token
    {
        public int value;
        public Num(int v) : base(Tag.NUM)
        {
            value = v;
        }
        public override string ToString()
        {
            return "" + value;
        }
    }

    public class Word : Token
    {
        public string lexeme = "";
        public Word(string s, int tag) : base(tag)
        {
            lexeme = s;
        }
        public override string ToString()
        {
            return lexeme;
        }

        public static Word and = new Word("&&", Tag.AND),
            or = new Word("||", Tag.OR),
            eq = new Word("==", Tag.EQ),
            ne = new Word("!=", Tag.NE),
            le = new Word("<=", Tag.LE),
            ge = new Word(">=", Tag.GE),
            minus = new Word("minus", Tag.MINUS),
            True = new Word("true", Tag.TRUE),
            False = new Word("false", Tag.FALSE),
            temp = new Word("t", Tag.TEMP);
    }

    public class Real : Token
    {
        public readonly double value;
        public Real(double v) : base(Tag.REAL)
        {
            value = v;
        }
        public override string ToString()
        {
            return "" + value;
        }
    }

    public class Lexer
    {
        public static int line = 1;
        string srcText;
        int currChar ;
        char peek = ' ';
        Hashtable words = new Hashtable();
        void reserve(Word w)
        {
            words.Add(w.lexeme, w);
        }
        public string SourceText
        {
            get
            {
                return srcText;
            }
            set
            {
                srcText = value;
            }

        }
        public Lexer()
        {
            reserve(new Word("if", Tag.IF));
            reserve(new Word("else", Tag.ELSE));
            reserve(new Word("while", Tag.WHILE));
            reserve(new Word("do", Tag.DO));
            reserve(new Word("break", Tag.BREAK));
            reserve(Word.True);
            reserve(Word.False);
            reserve(symbols.Type.Int);
            reserve(symbols.Type.Char);
            reserve(symbols.Type.Bool);
            reserve(symbols.Type.Float);
            currChar = 0;
        }

        void readch()
        {
            if (currChar == srcText.Length)
                peek = '\0';
            else 
                peek = (char)srcText[currChar++];
        }
        Boolean readch(char c)
        {
            readch();
            if (peek != c)
                return false;
            peek = ' ';
            return true;
             
        }
        public Token scan()
        {
            for (; ; readch())
            {
                if (peek == ' ' || peek == '\t' || peek == '\n') continue;
                else if (peek == '\r') line++;
                else break;        
            }
            switch (peek)
            {
                case '&':
                    if (readch('&')) return Word.and;
                    else return new Token('&');
                case '|':
                    if (readch('|')) return Word.or;
                    else return new Token('|');
                case '=':
                    if (readch('=')) return Word.eq;
                    else return new Token('=');
                case '!':
                    if (readch('=')) return Word.ne;
                    else return new Token('!');
                case '<':
                    if (readch('=')) return Word.le;
                    else return new Token('<');
                case '>':
                    if (readch('=')) return Word.ge;
                    else return new Token('>');
            }
            if (char.IsDigit(peek))
            {
                int v = 0;
                do
                {
                    v = 10 * v + (int)char.GetNumericValue(peek);
                    readch();
                } while (char.IsDigit(peek));
                if (peek != '.') return new Num(v);
                double x = v;
                double d = 10;
                for (;;)
                {
                    readch();
                    if (!char.IsDigit(peek)) break;
                    x = x + char.GetNumericValue(peek) / 10;
                    d = d * 10;
                }
                return new Real(x);                   
            }
            if (char.IsLetter(peek))
            {
                StringBuilder b = new StringBuilder();
                do
                {
                    b.Append(peek);
                    readch();
                } while (char.IsLetterOrDigit(peek));
                string s = b.ToString();
                Word w = (Word)words[s];
                if (w != null) return w;
                w = new Word(s, Tag.ID);
                words.Add(s,w);
                return w;
            }
            Token tok = new Token(peek);
            peek = ' ';
            return tok;
        }
    }
}
