using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace lexer
{
    // класс констант для перечисления возможных типов токенов
    public class AstNodeType
    {
        public const int UNKNOWN = 0;
        public const int NUMBER = 1;
        public const int IDENT = 5;
        public const int ADD = 11;
        public const int SUB = 12;
        public const int MUL = 13;
        public const int DIV = 14;
        public const int ASSIGN = 51;
        public const int INPUT = 52;
        public const int PRINT = 53;
        public const int BLOCK = 100;
        public const int PROGRAM = 101;
        public static string AstNodeTypeToString(int type)
        {
            switch (type)
            {
                case UNKNOWN: return "?";
                case NUMBER: return "NUM";
                case IDENT: return "ID";
                case ADD: return "+";
                case SUB: return "-";
                case MUL: return "*";
                case DIV: return "/";
                case ASSIGN: return "=";
                case INPUT: return "input";
                case PRINT: return "print";
                case BLOCK: return "..";
                case PROGRAM: return "program";
                default: return "";
            }
        }
    }
    public class AstNode
    {
        // тип узла (см. описание ниже)
        public virtual int Type { get; set; }
        // текст, связанный с узлом
        public virtual string Text { get; set; }
        // родительский узел для данного узла дерева
        private AstNode parent = null;
        // потомки (ветви) данного узла дерева
        private IList<AstNode> childs = new List<AstNode>();
        // конструкторы с различными параметрами (для удобства
        public AstNode(int type, string text, AstNode child1, AstNode child2)
        {
            Type = type;
            Text = text;
            if (child1 != null)
                AddChild(child1);
            if (child2 != null)
                AddChild(child2);
        }
        public AstNode(int type, AstNode child1, AstNode child2): this(type, null, child1, child2)
        {
        }
        public AstNode(int type, AstNode child1): this(type, child1, null)
        {
        }
        public AstNode(int type, string label) : this(type, label, null, null)
        {
        }
        public AstNode(int type): this(type, (string)null)
        {
        }
        // метод добавления дочернего узла
        public void AddChild(AstNode child)
        {
            if (child.Parent != null)
            {
                child.Parent.childs.Remove(child);
            }
            childs.Remove(child);
            childs.Add(child);
            child.parent = this;
        }
        // метод удаления дочернего узла
        public void RemoveChild(AstNode child)
        {
            childs.Remove(child);
            if (child.parent == this)
                child.parent = null;
        }
        // метод получения дочернего узла по индексу
        public AstNode GetChild(int index)
        {
            return childs[index];
        }
        // количество дочерних узлов
        public int ChildCount
        {
            get
            {
                return childs.Count;
            }
        }
        // родительский узел (свойство)
        public AstNode Parent
        {
            get
            {
                return parent;
            }
            set
            {
                value.AddChild(this);
            }
        }
        // индекс данного узла в дочерних узлах родительского узла
        public int Index
        {
            get
            {
                return Parent == null ? -1: Parent.childs.IndexOf(this);
            }
        }
        // представление узла в виде строки
        public override string ToString()
        {
            return Text != null ? Text : AstNodeType.AstNodeTypeToString(Type);
        }
    }

    class ParserBaseException : Exception
    {
        public ParserBaseException(string message)
            : base(message)
        { }
    }
    class IntepreterException : Exception
    {
        public IntepreterException(string message)
            : base(message)
        { }
    }
    public class ParserBase
    {
        // незначащие символы - пробельные символы по умолчанию
        public const string DEFAULT_WHITESPACES = " \n\r\t";
        // разбираемая строка
        private string source = null;
        // позиция указателя
        // (указывает на первый символ неразобранной части вход. строки)
        private int pos = 0;
        public ParserBase(string source)
        {
            this.source = source;
        }
        public string Source
        {
            get { return source; }
        }
        public int Pos
        {
            get { return pos; }
        }
        // предотвращает возникновение ошибки обращения за пределы
        // массива; в этом случае возвращает (char) 0,
        // что означает конец входной строки
        protected char this[int index]
        {
            get
            {
                return index < source.Length ? source[index] : (char)0;
            }
        }
        // символ в текущей позиции указателя
        public char Current
        {
            get { return this[Pos]; }
        }
        // определяет, достигнут ли конец строки
        public bool End
        {
            get
            {
                return Current == 0;
            }
        }
        // передвигает указатель на один символ
        public void Next()
        {
            if (!End)
                pos++;
        }
        // пропускает незначащие (пробельные) символы
        public virtual void Skip()
        {
            while (DEFAULT_WHITESPACES.IndexOf(this[pos]) >= 0)
                Next();
        }
        // распознает одну из строк; при этом указатель смещается и
        // пропускаются незначащие символы;
        // если ни одну из строк распознать нельзя, то возвращается null
        protected string MatchNoExcept(params string[] terms)
        {
            int pos = Pos;
            foreach (string s in terms)
            {
                bool match = true;
                foreach (char c in s)
                    if (Current == c)
                        Next();
                    else
                    {
                        this.pos = pos;
                        match = false;
                        break;
                    }
                if (match)
                {
                    // после разбора терминала пропускаем незначащие символы
                    Skip();
                    return s;
                }
            }
            return null;
        }
        // проверяет, можно ли в текущей позиции указателя, распознать
        // одну из строк; указатель не смещается
        public bool IsMatch(params string[] terms)
        {
            int pos = Pos;
            string result = MatchNoExcept(terms);
            this.pos = pos;
            return result != null;
        }
        // распознает одну из строк; при этом указатель смещается и
        // пропускаются незначащие символы; если ни одну из строк
        // распознать нельзя, то выбрасывается исключение
        public string Match(params string[] terms)
        {
            int pos = Pos;
            string result = MatchNoExcept(terms);
            if (result == null)
            {
                string message = "Ожидалась одна из строк: ";
                bool first = true;
                foreach (string s in terms)
                {
                    if (!first)
                        message += ", ";
                    message += string.Format("\"{0}\"", s);
                    first = false;
                }
                throw new ParserBaseException(
                string.Format("{0} (pos={1})", message, pos));
            }
            return result;
        }
            // то же, что и Match(params string[] a), для удобства
            public string Match(string s)
            {
                int pos = Pos;
                try
                {
                    return Match(new string[] { s });
                }
                catch
                {
                    throw new ParserBaseException(
                    string.Format(
                    "{0}: '{1}' (pos={2})",
                    s.Length == 1 ? "Ожидался символ"
                    : "Ожидалась строка",
                    s, pos
                    )
                    );
                }
            }
    }

    public class MathExprIntepreter : ParserBase
    {
        // "культуронезависимый" формат для чисел (с разделителем ".")
        public static readonly NumberFormatInfo NFI = new NumberFormatInfo();
        // конструктор
        public MathExprIntepreter(string source) : base(source)
        {
        }
        // далее идет реализация в виде функций правил грамматики
        // NUMBER -> <число> (реализация в грамматике не описана)
        public double NUMBER()
        {
            string number = "";
            while (Current == '.' || char.IsDigit(Current))
            {
                number += Current;
                Next();
            }
            if (number.Length == 0)
                throw new ParserBaseException(
                string.Format("Ожидалось число (pos={0})", Pos));
            Skip();
            return double.Parse(number, NFI);
        }
        // group -> "(" add ")" | NUMBER
        public double Group()
        {
            if (IsMatch("("))
            { // выбираем альтернативу
                Match("("); // это выражение в скобках
                double result = Add();
                Match(")");
                return result;
            }
            else
                return NUMBER(); // это число
        }
        // mult -> group ( ( "*" | "/" ) group )*
        public double Mult()
        {
            double result = Group();
            while (IsMatch("*", "/"))
            { // повторяем нужное кол-во раз
                string oper = Match("*", "/"); // здесь выбор альтернативы
                double temp = Group(); // реализован иначе
                result = oper == "*" ? result * temp : result / temp;
            }
            return result;
        }
        // add -> mult ( ( "+" | "-" ) mult )*
        public double Add()
        { // реализация аналогично правилу mult
            double result = Mult();
            while (IsMatch("+", "-"))
            {
                string oper = Match("+", "-");
                double temp = Mult();
                result = oper == "+" ? result + temp : result - temp;
            }
            return result;
        }
        // result -> add
        public double Result()
        {
            return Add();
        }
        // метод, вызывающий начальное и правило грамматики и
        // соответствующие вычисления
        public double Execute()
        {
            Skip();
            double result = Result();
            if (End)
                return result;
            else
                throw new ParserBaseException( // разобрали не всю строку
                string.Format("Лишний символ '{0}' (pos={1})",
                Current, Pos)
                );
        }
        // статическая реализации предыдузего метода (для удобства)
        public static double Execute(string source)
        {
            MathExprIntepreter mei = new MathExprIntepreter(source);
            return mei.Execute();
        }
    }

    public class MathLangParser : ParserBase
    {
        // конструктор
        public MathLangParser(string source) : base(source)
        {
        }
        // далее идет реализация в виде функций правил грамматики
        // NUMBER -> <число>
        public AstNode NUMBER()
        {
            string number = "";
            while (Current == '.' || char.IsDigit(Current))
            {
                number += Current;
                Next();
            }
            if (number.Length == 0)
                throw new ParserBaseException(
                string.Format("Ожидалось число (pos={0})", Pos));
            Skip();
            return new AstNode(AstNodeType.NUMBER, number);
        }
        // IDENT -> <идентификатор>
        public AstNode IDENT()
        {
            string identifier = "";
            if (char.IsLetter(Current))
            {
                identifier += Current;
                Next();
                while (char.IsLetterOrDigit(Current))
                {
                    identifier += Current;
                    Next();
                }
            }
            else
                throw new ParserBaseException(
                string.Format("Ожидался идентификатор (pos={0})", Pos));
            Skip();
            return new AstNode(AstNodeType.IDENT, identifier);
        }
        // group -> "(" term ")" | IDENT | NUMBER
        public AstNode Group()
        {
            if (IsMatch("("))
            { // выбираем альтернативу
                Match("("); // это выражение в скобках
                AstNode result = Term();
                Match(")");
                return result;
            }
            else if (char.IsLetter(Current))
            {
                int pos = Pos; // это идентификатор
                return IDENT();
            }
            else
                return NUMBER(); // число
        }
        // mult -> group ( ( "*" | "/" ) group )*
        public AstNode Mult()
        {
            AstNode result = Group();
            while (IsMatch("*", "/"))
            { 
                // повторяем нужное кол-во раз
                string oper = Match("*", "/"); // здесь выбор альтернативы
                AstNode temp = Group(); // реализован иначе
                result = oper == "*" ? new AstNode(AstNodeType.MUL, result, temp): new AstNode(AstNodeType.DIV, result, temp);
            }
            return result;
        }
        // add -> mult ( ( "+" | "-" ) mult )*
        public AstNode Add()
        { // реализация аналогично правилу mult
            AstNode result = Mult();
            while (IsMatch("+", "-"))
            {
                string oper = Match("+", "-");
                AstNode temp = Mult();
                result =
                oper == "+" ? new AstNode(AstNodeType.ADD, result, temp)
                : new AstNode(AstNodeType.SUB, result, temp);
            }
            return result;
        }
        // term -> add
        public AstNode Term()
        {
            return Add();
        }
        // expr -> "print" term | "input" IDENT | IDENT "=" term
        public AstNode Expr()
        {
            if (IsMatch("print"))
            { // выбираем альтернативу
                Match("print"); // это вывод данных
                AstNode value = Term();
                return new AstNode(AstNodeType.PRINT, value);
            }
            else if (IsMatch("input"))
            {
                Match("input"); // это ввод данных
                AstNode identifier = IDENT();
                return new AstNode(AstNodeType.INPUT, identifier);
            }
            else
            {
                AstNode identifier = IDENT();
                Match("="); // это операция присвоения значения
                AstNode value = Term();
                return new AstNode(AstNodeType.ASSIGN, identifier, value);
            }
        }
        // program -> ( expr )*
        public AstNode Program()
        {
         AstNode programNode = new AstNode(AstNodeType.PROGRAM);
            while (!End) // повторяем до конца входной строки
                programNode.AddChild(Expr());
            return programNode;
        }
        // result -> program
        public AstNode Result()
        {
            return Program();
        }
        // метод, вызывающий начальное и правило грамматики и
        // соответствующий парсинг
        public AstNode Parse()
        {
            Skip();
            AstNode result = Result();
            if (End)
                return result;
            else
                throw new ParserBaseException( // разобрали не всю строку
                string.Format("Лишний символ '{0}' (pos={1})",
                Current, Pos)
                );
        }
        // статическая реализации предыдузего метода (для удобства)
        public static AstNode Parse(string source)
        {
            MathLangParser mlp = new MathLangParser(source);
            return mlp.Parse();
        }
    }


    public class MathLangIntepreter
    {
        // "культуронезависимый" формат для чисел (с разделителем ".")
        public static readonly NumberFormatInfo NFI =  new NumberFormatInfo();
        // таблица переменных
        private Dictionary<string, double> varTable = new Dictionary<string, double>();
        // корневой узел AST-дерева программы
        private AstNode programNode = null;
        // конструктор
        public MathLangIntepreter(AstNode programNode)
        {
            if (programNode.Type != AstNodeType.PROGRAM)
                throw new IntepreterException("AST-дерево не является программой");
            this.programNode = programNode;
        }
        // рекурсивный метод, который вызывается для каждого узла дерева
        private double ExecuteNode(AstNode node)
        {
            switch (node.Type)
            {
                case AstNodeType.UNKNOWN:
                        throw new IntepreterException("Неопределенный тип узла AST-дерева");
                case AstNodeType.NUMBER:
                                return double.Parse(node.Text, NFI);
                case AstNodeType.IDENT:
                    if (varTable.ContainsKey(node.Text))
                            return varTable[node.Text];
                    else
                            throw new ParserBaseException(string.Format("Значение {0} не определено", node.Text));
                 case AstNodeType.ADD:
                                        return ExecuteNode(node.GetChild(0)) + ExecuteNode(node.GetChild(1));
                 case AstNodeType.SUB:
                                        return ExecuteNode(node.GetChild(0)) - ExecuteNode(node.GetChild(1));
                 case AstNodeType.MUL:
                                        return ExecuteNode(node.GetChild(0)) * ExecuteNode(node.GetChild(1));
                 case AstNodeType.DIV:
                                        return ExecuteNode(node.GetChild(0)) /ExecuteNode(node.GetChild(1));
                 case AstNodeType.ASSIGN:
                                        varTable[node.GetChild(0).Text] =  ExecuteNode(node.GetChild(1));
                                        break;
                 case AstNodeType.INPUT:
                                     Console.Write("input {0}: ", node.GetChild(0).Text);
                                                varTable[node.GetChild(0).Text] =
                                                double.Parse(Console.ReadLine(), NFI);
                                                break;
                 case AstNodeType.PRINT:
                                         Console.WriteLine(
                                         ExecuteNode(node.GetChild(0)).ToString(NFI));
                                                    break;
                 case AstNodeType.BLOCK:
                 case AstNodeType.PROGRAM:
                                         for (int i = 0; i < node.ChildCount; i++)
                                                 ExecuteNode(node.GetChild(i));
                                                    break;
                 default:
                         throw new IntepreterException("Неизвестный тип узла AST-дерева");
             }
                         return 0;
 }
    // public-метод для вызова интерпретации
    public void Execute()
    {
        ExecuteNode(programNode);
    }
    // статическая реализации предыдузего метода (для удобства)
    public static void Execute(AstNode programNode)
    {
        MathLangIntepreter mei = new MathLangIntepreter(programNode);
        mei.Execute();
    }
}

}