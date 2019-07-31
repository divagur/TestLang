using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using lexer;
using parser;

namespace lexer_test
{
    public class Var<T>
    {
        T val;
        public Var(T v)
        {
            val = v;
        }
        public T Value
        {
            get { return val; }
            set
            { val = value; }

        }

    }
    public class EvalTag
    {
        public const int ADD = 0,
            SUB = 1, MUL = 2, DIV = 3,
            ELSE = 260, EQ = 261, FALSE = 262, GE = 263,
            ID = 264, IF = 265, INDEX = 266, LE = 267,
            MINUS = 268, NE = 269, NUM = 270, OR = 271,
            REAL = 272, TEMP = 273, TRUE = 274, WHILE = 275, SEQ = 276;
    }
    public class EvalRow
    {
        public int EvalType;
        public int Var1;
        public int Var2;
        public int ResId;
    }
    public class Eval
    {
        bool _isCompile;
        int currentCommand;

        Parser parcer;
        List<EvalRow> commandList;
        public Eval(Parser parser_param)
        {
            parcer = parser_param;
            commandList = new List<EvalRow>();
        }
        public bool Compile()
        {
            
            parcer.program();
            return true;
        }
        public void Exec()
        {
            currentCommand = 0;
            foreach(EvalRow row in commandList)
            {
                switch (row.EvalType)
                {
                    case EvalTag.ADD:
                        int Res = row.Var1 + row.Var2;
                        break; 
                }
            }
        }

    }


    class Program
    {
        class Registr
        {
            public byte DataType;
            public string SAX;
            public int IAX;
            public DateTime DAX;
            public object OAX;
           
        }

     public class AstNodePrinter
    {
        public const byte ConnectCharDosCode = 0xB3,
        MiddleNodeCharDosCode = 0xC3,
        LastNodeCharDosCode = 0xC0;
        public static readonly char ConnectChar = '|',
        MiddleNodeChar = '*',
        LastNodeChar = '-';
        static AstNodePrinter()
        {
            Encoding dosEncoding = null;
            try
            {
                dosEncoding = Encoding.GetEncoding("cp866");
            }
            catch { }
            if (dosEncoding != null)
            {
                ConnectChar = dosEncoding.GetChars(
                new byte[] { ConnectCharDosCode })[0];
                MiddleNodeChar = dosEncoding.GetChars(
                new byte[] { MiddleNodeCharDosCode })[0];
                LastNodeChar = dosEncoding.GetChars(
                new byte[] { LastNodeCharDosCode })[0];
            }
        }
        private static string getStringSubTree(AstNode node,
        string indent, bool root)
        {
            if (node == null)
                return "";
            string result = indent;
            if (!root)
                if (node.Index < node.Parent.ChildCount - 1)
                {
                    result += MiddleNodeChar + " ";
                    indent += ConnectChar + " ";
                }
                else
                {
                    result += LastNodeChar + " ";
                    indent += " ";
                }
            result += node + "\n";
            for (int i = 0; i < node.ChildCount; i++)
                result += getStringSubTree(node.GetChild(i), indent, false);
            return result;
        }
        public static string astNodeToAdvancedDosStringTree(
        AstNode node)
        {
        return getStringSubTree(node, "", true);
        }
        public static void Print(AstNode node)
        {
            string tree = astNodeToAdvancedDosStringTree(node);
            Console.WriteLine(tree);
        }
    }


    static void Main(string[] args)
        {
            Lexer lex = new Lexer();
            /*
                   lex.SourceText = @"{
                      int i; int j; float v; float x; float[100] a; 
       while( true ) {
       do i=i+1; while(a[i]<v);
       do j=j-1; while(a[j]>v);
       if(i>=j) break;
       x=a[i];a[i]=a[j];a[j]=x;
       }
       }   
                   ";
       b = 1; c=2;d=3;*/
            lex.SourceText = @"{int a; int b; int c;
                                 c=(a+b)/(a+4);
                        }";
            /*
                                         if ((a+b)==1)
                                if(1==1)
                                    b=a+b*3;                
                             a=1;
             */
            /*
            Parser parse = new Parser(lex);

            parse.program();

            Console.WriteLine();
            Console.WriteLine();
            Var<int> nVal = new Var<int>(250);
            Var<string> sVal = new Var<string>("String value");
            List<int>[] arList = new List<int>[5];

            List<Variable> VarList = new List<Variable>();

            Registr[] reg = new Registr[5];

            VarList.Add(new StringVariable());
            VarList.Add(new NumberVariable());

            for (int i = 0; i < 5; i++)
                reg[i] = new Registr();

            reg[0].IAX = 5;
            reg[1].IAX = 5;
            (VarList[0] as StringVariable).Value = "string value";
            (VarList[1] as NumberVariable).Value = 50;

            nVal.Value = 500;

    
            Console.WriteLine("value = " + (reg[0].IAX + reg[1].IAX));
            */

            /*
            double result = MathExprIntepreter.Execute("3+4*(2+7-3)+5*10");

            Console.WriteLine("result = "+result.ToString());
            Console.ReadLine();
*/
            // в зависимости от наличия параметров командной строки
            // разбираем либо файл с именем, переданным первым параметром
            // командной строки, либо стандартный ввод
            //TextReader reader = args.Length >= 1 ? new StreamReader(args[0]) : Console.In;
            //String source = reader.ReadToEnd();
            String source = @"a=2 
b=3
c=a+b*a
print c";
            try
            {
                AstNode program = MathLangParser.Parse(source);
                AstNodePrinter.Print(program);
                Console.WriteLine("------------------------");
                MathLangIntepreter.Execute(program);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            Console.ReadLine();
        }
    }
}
