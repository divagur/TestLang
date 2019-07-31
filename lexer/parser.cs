using System;
using System.Collections;
using System.Text;
using System.IO;
using lexer;
using inter;
using symbols;
using System.Collections.Generic;

namespace parser
{


    class Variable
    {
        public string Id;
        public string Name;
        public object Value;
    }


    class StringVariable : Variable
    {
        string _value;

        public new string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }


    class NumberVariable : Variable
    {
        int _value;
        public new int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }


    }


    public class Parser
    {
        private Lexer lex;
        private Token look;
        //public List<string> resultSet;
        public List<CodeRow> compileRows;
        Env top = null;

        int used = 0;

        public Parser(Lexer l)
        {
            //throw new IndexOutOfRangeException()
            lex = l;
            compileRows = new List<CodeRow>();
            move();
        }

        void move()
        {
            look = lex.scan();
        }

        void error(String s)
        {
            //throw new Exception("near line " + Lexer.line + ": " + s);
            Console.WriteLine("near line " + Lexer.line + ": " + s);
        }

        void match(int t)
        {
            if (look.tag == t) move();
            else error("syntax error");
        }
        public void program()
        {
            Stmt s = block();
            int begin = s.newlabel();
            int after = s.newlabel();

            s.emitlabel(begin);
            s.gen(begin, after);
            s.emitlabel(after);

            Stmt ss = s;

            do
            {
                Console.WriteLine(s.ToString());
            }
            while (ss.StmtType == 0);
        }

        private Stmt block()
        {
            match('{');
            Env saveEnv = top;
            top = new Env(top);
            decls();
            Stmt s = stmts();
            match('}');
            top = saveEnv;
            return s;
        }

        void decls()
        {
            while (look.tag == Tag.BASIC)
            {
                symbols.Type p = type();
                Token tok = look;
                match(Tag.ID);
                match(';');
                Id id = new Id((Word)tok, p, used, compileRows);
                top.put(tok, id);
                used = used + p.width;
            }
        }

        symbols.Type type()
        {
            symbols.Type p = (symbols.Type)look;
            match(Tag.BASIC);
            if (look.tag != '[') return p;
            else return dims(p);
        }

        symbols.Type dims(symbols.Type p)
        {
            match('[');
            Token tok = look;
            match(Tag.NUM);
            match(']');
            if (look.tag == '[')
                p = dims(p);
            return new symbols.Array(((Num)tok).value, p);
        }

        Stmt stmts()
        {
            if (look.tag == '}') return Stmt.Null;
            else return new Seq(stmt(), stmts(), compileRows);
        }

        Stmt stmt()
        {
            Expr x;
            Stmt s, s1, s2;
            Stmt savedStmt;

            Stmt stmtResult;

            switch (look.tag)
            {
                case ';':
                    {
                        move();
                        //return Stmt.Null;
                        stmtResult = Stmt.Null;
                        break;
                    }

                case Tag.IF:
                    {
                        match(Tag.IF);
                        match('(');
                        x = bool_expr();
                        match(')');
                        s1 = stmt();
                        if (look.tag != Tag.ELSE)
                            return new If(x, s1, compileRows);
                        match(Tag.ELSE);
                        s2 = stmt();
                        //return new Else(x, s1, s2);
                        stmtResult = new Else(x, s1, s2, compileRows);
                        break;
                    }
                case Tag.WHILE:
                    {
                        While whilenode = new While(compileRows);
                        savedStmt = Stmt.Enclosing;
                        Stmt.Enclosing = whilenode;
                        match(Tag.WHILE);
                        match('(');
                        x = bool_expr();
                        match(')');
                        s1 = stmt();
                        whilenode.init(x, s1);
                        Stmt.Enclosing = savedStmt;
                        //return whilenode;
                        stmtResult = whilenode;
                        break;
                    }
                case Tag.DO:
                    {
                        Do donode = new Do(compileRows);
                        savedStmt = Stmt.Enclosing;
                        Stmt.Enclosing = donode;
                        match(Tag.DO);
                        s1 = stmt();
                        match(Tag.WHILE);
                        match('(');
                        x = bool_expr();
                        match(')');
                        match(';');
                        donode.init(s1, x);
                        Stmt.Enclosing = savedStmt;
                        //return donode;
                        stmtResult = donode;
                        break;
                    }
                case Tag.BREAK:
                    {
                        match(Tag.BREAK);
                        match(';');
                        //return new Break();
                        stmtResult = new Break(compileRows);
                        break;
                    }
                case '{':
                    return block();
                default:
                    //return assign();
                    stmtResult = assign();
                    break;
            }
/*
            for(int i = 0;i< stmtResult.rows.Count;i++)
            {
                compileRows.Add(stmtResult.rows[i]);
            }
         */   
            return stmtResult;
        }

        Stmt assign()
        {
            Stmt stmt;
            Token t = look;
            match(Tag.ID);
            Id id = top.get(t);
            if (id == null)
                error(t.ToString() + " undeclared");
            if (look.tag == '=')
            {
                move();
                stmt = new Set(id, bool_expr(), compileRows);
            }
            else
            {
                Access x = offset(id);
                match('=');
                stmt = new SetElem(x, bool_expr(), compileRows);
            }
            match(';');
            return stmt;
        }

        Expr bool_expr()
        {
            Expr x = join();
            while (look.tag == Tag.OR)
            {
                Token tok = look;
                move();
                x = new Or(tok, x, join(), compileRows);
            }
            return x;
        }

        Expr join()
        {
            Expr x = equality();
            while (look.tag == Tag.AND)
            {
                Token tok = look;
                move();
                x = new And(tok, x, equality(), compileRows);
            }
            return x;
        }

        Expr equality()
        {
            Expr x = rel();
            while (look.tag == Tag.EQ || look.tag == Tag.NE)
            {
                Token tok = look;
                move();
                x = new Rel(tok, x, rel(), compileRows);
            }
            return x;
        }

        Expr rel()
        {
            Expr x = expr();
            switch (look.tag)
            {
                case '<':
                case Tag.LE:
                case Tag.GE:
                case '>':
                    Token tok = look;
                    move();
                    return new Rel(tok, x, expr(), compileRows);
                default:
                    return x;
            }

        }

        Expr expr()
        {
            Expr x = term();
            while (look.tag == '+' || look.tag == '-')
            {
                Token tok = look;
                move();
                x = new Arith(tok, x, term(), compileRows);
            }
            return x;
        }

        Expr term()
        {
            Expr x = unary();
            while (look.tag == '*' || look.tag == '/')
            {
                Token tok = look;
                move();
                x = new Arith(tok, x, unary(), compileRows);
            }
            return x;
        }

        Expr unary()
        {
            if (look.tag == '-')
            {
                move();
                return new Unary(Word.minus, unary(), compileRows);
            }
            else if (look.tag == '!')
            {
                Token tok = look;
                move();
                return new Not(tok, unary(), compileRows);
            }
            else return factor();
        }

        Expr factor()
        {
            Expr x = null;
            switch (look.tag)
            {
                case '(':
                    move();
                    x = bool_expr();
                    match(')');
                    return x;
                case Tag.NUM:
                    x = new Constant(look, symbols.Type.Int, compileRows);
                    move();
                    return x;
                case Tag.REAL:
                    x = new Constant(look, symbols.Type.Float, compileRows);
                    move();
                    return x;
                case Tag.TRUE:
                    x = Constant.True;
                    move();
                    return x;
                case Tag.FALSE:
                    x = Constant.False;
                    move();
                    return x;
                default:
                    error("syntax error");
                    return x;
                case Tag.ID:
                    String s = look.ToString();
                    Id id = top.get(look);
                    if (id == null)
                        error(look.ToString() + " undeclared");
                    move();
                    if (look.tag != '[') return id;
                    else return offset(id);
            }
        }

        Access offset(Id a)
        {
            Expr i;
            Expr w;
            Expr t1, t2;
            Expr loc;
            symbols.Type type = a.type;
            match('[');
            i = bool_expr();
            match(']');
            type = ((symbols.Array)type).of;
            w = new Constant(type.width, compileRows);
            t1 = new Arith(new Token('*'), i, w, compileRows);
            loc = t1;
            while (look.tag == '[')
            {
                match('[');
                i = bool_expr();
                match(']');
                type = ((symbols.Array)type).of;
                w = new Constant(type.width, compileRows);
                t1 = new Arith(new Token('*'), i, w, compileRows);
                t2 = new Arith(new Token('+'), loc, t1, compileRows);
                loc = t2;
            }
            return new Access(a, loc, type, compileRows);
        }


        bool Assembly(Seq root)
        {
            return true;
        }
    }
}