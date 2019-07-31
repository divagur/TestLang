using System;
using System.Collections.Generic;
using lexer;
using symbols;
namespace inter
{

    public class CodeRow
    {
        public int labelNum;
        public string labelStr;
        public string command;
        public int lineNum;
        public int tag;

        public override string ToString()
        {
            return labelStr + " " + command;
            //return command;
        }
    }

    public class Node
    {
        int lexline = 0;
        static int labels = 0;
        public List<CodeRow> rows;
       // public List<CodeRow> outputList;


        public Node()
        {
            lexline = Lexer.line;
            //rows = new List<CodeRow>();
            //row.lineNum = lexline;
        }
        public void Error(string s)
        {
            //throw new Exception("near line "+lexline+": "+s);
            Console.WriteLine("near line " + lexline + ": " + s);
        }
        public int newlabel()
        {
            return ++labels;

        }
        public void emitlabel(int i)
        {
            Console.WriteLine("L"+i+":");

            //CodeRow row = rows.Count>0?rows[rows.Count - 1]:new CodeRow();
            CodeRow row = new CodeRow();
            row.labelNum = i;
            row.labelStr = "L" + i;
           //row.command = "L" + i + ":";

            rows.Add(row);

        }
        public void emit(string s)
        {
            Console.WriteLine("\t" + s);
/*
            CodeRow row = new CodeRow();
            row.command = "\t" + s;
            rows.Add(row);
            */
            CodeRow row = rows.Count > 0 ? rows[rows.Count - 1] : new CodeRow();
            if (row.command == null)
            {
                row.command = "\t" + s;
            }
            else
            {
                CodeRow rowEx = new CodeRow();
                rowEx.command = "\t" + s;
                rows.Add(rowEx);
            }
            
        }
    }

    public class Expr : Node
    {
        public Token op;
        public symbols.Type type;
        public Expr(Token tok, symbols.Type p, List<CodeRow> outputList)
        {
            op = tok;
            type = p;
            rows = outputList;
        }
        public virtual Expr gen()
        {
            return this;
        }
        public virtual Expr reduce()
        {
            return this;
        }
        public virtual void jumping(int t, int f)
        {
            emitjumps(ToString(),t,f);
            
        }
        public void emitjumps(string test, int t, int f)
        {
            if (t != 0 && f != 0)
            {
                emit("if " + test + " goto L" + t);
                emit("goto L" + f);
            }
            else if (t != 0)
                emit("if " + test + " goto L" + t);
            else if (f != 0)
                emit("iffalse " + test + " goto L" + f);
            //else;
        }
        public override string ToString()
        {
            return op.ToString();
        }
    }

    public class Id: Expr
    {
        public int offset;
        public Id(Word id, symbols.Type p, int b, List<CodeRow> outputList) : base(id, p, outputList)
        {
            offset = b;
            
        }
    }

    public class Temp : Expr
    {
        static int count = 0;
        int number = 0;
        public Temp(symbols.Type p, List<CodeRow> outputList) : base(Word.temp,p, outputList)
        {
            number = ++count;
        }
        public override string ToString()
        {
            return "t" + number;
        }

    }

    public class Op : Expr
    {
        public Op(Token tok, symbols.Type p, List<CodeRow> outputList) : base(tok, p, outputList)
        {

        }

        public override Expr reduce()
        {
            Expr x = gen();
            Temp t = new Temp(type,rows);
            emit(t.ToString() + "=" + x.ToString());
            return t;
        }
    }

    public class Arith : Op
    {
        public Expr expr1, expr2;
        public Arith(Token tok, Expr x1, Expr x2, List<CodeRow> outputList) :base(tok, null, outputList)
        {
            expr1 = x1;
            expr2 = x2;
            type = symbols.Type.max(expr1.type,expr2.type);
            if (type == null)
            {
                Console.WriteLine("error type");
            }
        }
        public override Expr gen()
        {
            return new Arith(op, expr1.reduce(), expr2.reduce(),rows);
        }
        public override string ToString()
        {
            return expr1.ToString() + " " + op.ToString() + " " + expr2.ToString();
        }
    }
    public class Unary:Op
    {
        public Expr expr;
        public Unary(Token tok, Expr x, List<CodeRow> outputList) :base(tok,null, outputList)
        {
            expr = x;
            type = symbols.Type.max(symbols.Type.Int, expr.type);
            if (type == null)
            {
                Console.WriteLine("error type");
            }

        }
        public override Expr gen()
        {
            return new Unary(op,expr.reduce(),rows);
        }
        public override string ToString()
        {
            return op.ToString() + " " + expr.ToString();
        }
    }

    public class Constant:Expr
    {
        public  Constant(Token tok, symbols.Type p, List<CodeRow> outputList) :base(tok,p, outputList) { }
        public Constant(int i, List<CodeRow> outputList) : base(new Num(i), symbols.Type.Int, outputList) { }

        public static Constant
                True = new Constant(Word.True, symbols.Type.Bool,null),
                False = new Constant(Word.False, symbols.Type.Bool,null);
        public override void jumping(int t, int f)
        {
            if (this == True && t != 0)
            {
                emit("goto L" + t);
            }
            else if (this == False && f != 0)
            {
                emit("goto L" + f);
            }
        }
    }
    public class Logical:Expr
    {
        public Expr expr1, expr2;
        public Logical(Token tok, Expr x1, Expr x2, List<CodeRow> outputList) :base(tok, null, outputList)
        {
            expr1 = x1;
            expr2 = x2;
            type = check(expr1.type, expr2.type);
            if(type == null)
            {
                Console.WriteLine("type error");
            }
        
        }
        public virtual symbols.Type check(symbols.Type p1, symbols.Type p2)
        {
            if (p1 == symbols.Type.Bool && p2 == symbols.Type.Bool)
                return symbols.Type.Bool;
            else
                return null;
        }
        public override Expr gen()
        {
            int f = newlabel();
            int a = newlabel();
            Temp temp = new Temp(type,rows);
            this.jumping(0, f);
            emit(temp.ToString() + "=true");
            emit("goto L" + a);
            emitlabel(f);
            emit(temp.ToString() + " = false");
            emitlabel(a);
            return temp;
        }
        public override string ToString()
        {
            return expr1.ToString() + " " + op.ToString() + " " + expr2.ToString();
        }
    }

    public class Or:Logical
    {
        public Or(Token tok, Expr x1, Expr x2, List<CodeRow> outputList) : base(tok, x1, x2,outputList) {  } 
        
        public override void jumping(int t, int f)
        {
            int label = t != 0 ? t : newlabel();
            expr1.jumping(label, 0);
            expr2.jumping(t, f);
            if (t == 0) emitlabel(label);
        }
    }

    public class And : Logical
    {
        public And(Token tok, Expr x1, Expr x2, List<CodeRow> outputList) : base(tok, x1, x2, outputList) { }

        public override void jumping(int t, int f)
        {
            int label = f != 0 ? f : newlabel();
            expr1.jumping(0, label);
            expr2.jumping(t, f);
            if (f == 0) emitlabel(label);
        }
    }

    public class Not : Logical
    {
        public Not(Token tok, Expr expr2, List<CodeRow> outputList) : base(tok, expr2 , expr2, outputList) { }

        public override void jumping(int t, int f)
        {
            expr2.jumping(f, t);
        }

        public override string ToString()
        {
            return op.ToString() + " " + expr2.ToString();
        }
    }

    public class Rel:Logical
    {
        public Rel(Token tok, Expr x1, Expr x2, List<CodeRow> outputList) :base(tok,x1,x2, outputList)
        {

        }

        public override symbols.Type check(symbols.Type p1, symbols.Type p2)
        {
            if (p1 is symbols.Array || p1 is symbols.Array)
                return null;
            else if (p1 == p2) return symbols.Type.Bool;
            else return null;
        }
        public override void jumping(int t, int f)
        {
            Expr a = expr1.reduce();
            Expr b = expr2.reduce();
            String test = a.ToString() + " " + op.ToString() + " " + b.ToString();
            emitjumps(test, t, f);
        }
    }

    public class Access:Op
    {
        public Id array;
        public Expr index;
        public Access(Id a, Expr i, symbols.Type p, List<CodeRow> outputList) :base(new Word("[]",Tag.INDEX),p, outputList)
        {
            array = a;
            index = i;
        }
        public override Expr gen()
        {
            return new Access(array,index.reduce(),type,rows);
        }

        public override void jumping(int t, int f)
        {
            emitjumps(reduce().ToString(), t, f);
        }

        public override string ToString()
        {
            return array.ToString() + " [ " + index.ToString() + " ] ";
        }
    }
    public class Stmt : Node
    {
        public int StmtType;
        public Stmt()
        {
            StmtType = 0;
        }
        public static Stmt Null = new Stmt();
        public virtual void gen(int b, int a)
        {

        }
        public int after = 0;
        public static Stmt Enclosing = Stmt.Null;
    }


    public class If:Stmt
    {
        Expr expr;
        Stmt stmt;
        public If(Expr x, Stmt s, List<CodeRow> outputList)
        {
            expr = x;
            stmt = s;
            StmtType = Tag.IF;
            rows = outputList;

            if (expr.type != symbols.Type.Bool)
            {
                expr.Error("boolean required in if");
            }
                
        }
        public override void gen(int b, int a)
        {
            int label = newlabel();
            expr.jumping(0, a);
            emitlabel(label);
            stmt.gen(label, a);
        }
    }

    public class Else:Stmt
    {
        Expr expr;
        Stmt stmt1, stmt2;

        public Else(Expr x, Stmt s1, Stmt s2, List<CodeRow> outputList)
        {
            rows = outputList;
            expr = x;
            StmtType = Tag.ELSE;
            stmt1 = s1;
            stmt2 = s2;
            if(expr.type != symbols.Type.Bool)
            {
                expr.Error("boolean required in if");
            }
        }

        public override void gen(int b, int a)
        {
            int label1 = newlabel();
            int label2 = newlabel();

            expr.jumping(0, label2);
            emitlabel(label1);
            stmt1.gen(label1, a);
            emit("goto L" + a);
            emitlabel(label2);
            stmt2.gen(label2, a);
        }

    }

    public class While:Stmt
    {
        Expr expr;
        Stmt stmt;

        public While(List<CodeRow> outputList)
        {
            expr = null;
            stmt = null;
            StmtType = Tag.WHILE;
            rows = outputList;
        }

        public void init(Expr x, Stmt s)
        {
            expr = x;
            stmt = s;

            if (expr.type != symbols.Type.Bool)
                expr.Error("boolean required in while");
        }

        public override void gen(int b, int a)
        {
            after = a;
            int label = newlabel();
            stmt.gen(b, label);
            emitlabel(label);
            expr.jumping(b, 0);
        }
    }

    public class Do:Stmt
    {
        Expr expr;
        Stmt stmt;

        public Do(List<CodeRow> outputList)
        {
            expr = null;
            stmt = null;
            StmtType = Tag.DO;
            rows = outputList;
        }
        public void init(Stmt s, Expr x)
        {
            expr = x;
            stmt = s;

            if (expr.type != symbols.Type.Bool)
                expr.Error("boolean required in while");
        }

        public override void gen(int b, int a)
        {
            after = a;
            int label = newlabel();
            stmt.gen(b, label);
            emitlabel(label);
            expr.jumping(b, 0);
        }
    }

    public class Set:Stmt
    {
        public Id id;
        public Expr expr;

        public Set(Id i, Expr x, List<CodeRow> outputList)
        {
            id = i;
            expr = x;
            StmtType = Tag.ID;
            rows = outputList;

            if (check(id.type, expr.type) == null)
                Error("type error");
        }

        public symbols.Type check(symbols.Type p1, symbols.Type p2)
        {
            if (symbols.Type.numeric(p1) && symbols.Type.numeric(p2))
                return p2;
            else if (p1 == symbols.Type.Bool && p2 == symbols.Type.Bool)
                return p2;
            else
                return null;
        }
        public override void gen(int b, int a)
        {
            emit(id.ToString() + " = " + expr.gen().ToString());
        }
    }

    public class SetElem:Stmt
    {
        public Id array;
        public Expr index;
        public Expr expr;
        public SetElem(Access x, Expr y, List<CodeRow> outputList)
        {
            array = x.array;
            index = x.index;
            StmtType = Tag.ID;
            expr = y;
            rows = outputList;

            if (check(x.type, expr.type) == null)
                Error("type error");
        }
        public symbols.Type check(symbols.Type p1, symbols.Type p2)
        {
            if ((p1 is symbols.Array) || (p2 is symbols.Array))
                return null;
            else if (p1 == p2)
                return p2;
            else if (symbols.Type.numeric(p1) && symbols.Type.numeric(p2))
                return p2;
            else
                return null;
        }
        public override void gen(int b, int a)
        {
            String s1 = index.reduce().ToString();
            String s2 = expr.reduce().ToString();
            emit(array.ToString() + " [ " + s1 + " ] = " + s2);
        }
    }

    public class Seq:Stmt
    {
        Stmt stmt1;
        Stmt stmt2;
        //List<CodeRow> outputRows;
        public Seq(Stmt s1, Stmt s2, List<CodeRow> outputList)
        {
            stmt1 = s1;
            stmt2 = s2;
            StmtType = Tag.SEQ;
            rows = outputList;
        }

        public override void gen(int b, int a)
        {
            if (stmt1 == Stmt.Null) stmt2.gen(b, a);
            else if (stmt2 == Stmt.Null) stmt1.gen(b, a);
            else
            {
                int label = newlabel();
                stmt1.gen(b, label);
                emitlabel(label);
                stmt2.gen(label, a);
            }
            /*
            if (stmt1 !=Stmt.Null)
                for(int i=0; i<stmt1.rows.Count;i++)
                {
                    outputRows.Add(stmt1.rows[i]);
                }
            if (stmt2 != Stmt.Null)
                for (int i = 0; i < stmt2.rows.Count; i++)
                {
                    outputRows.Add(stmt2.rows[i]);
                }
                */

        }
    }

    public class Break:Stmt
    {
        Stmt stmt;
        public Break(List<CodeRow> outputList)
        {
            StmtType = Tag.BREAK;
            rows = outputList;

            if (Stmt.Enclosing == null)
                Error("unenclosed break");
            stmt = Stmt.Enclosing;
        }

        public override void gen(int b, int a)
        {
            emit("goto L" + stmt.after);
        }
    }
}