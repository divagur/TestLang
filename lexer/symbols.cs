using System;
using System.Collections;
using lexer;
using inter;

namespace symbols
{
    public class Env
    {
        private Hashtable table;
        protected Env prev;
        public Env(Env n)
        {
            table = new Hashtable();
            prev = n;
        }
        public void put(Token w, Id i)
        {
            table.Add(w, i);
        }
        public Id get(Token w)
        {
            for (Env e = this; e != null; e = e.prev)
            {
                Id found = (Id)(e.table[w]);
                if (found != null) return found;
               // Id found = (Id)(e.table.Values(w));
                      
            }
            return null;
        }
    }

    public class Type : Word
    {
        public int width = 0;
        public Type(string s, int tag, int w) : base(s, tag)
        {
            width = w;
        }
        public static Type Int = new Type("int", Tag.BASIC, 4),
            Float = new Type("float", Tag.BASIC, 8),
            Char = new Type("char", Tag.BASIC, 1),
            Bool = new Type("bool",Tag.BASIC,1);

        public static bool numeric(Type p)
        {
            if (p == Type.Char || p == Type.Int || p == Type.Float) return true;
            else return false;
        }
        public static Type max(Type p1, Type p2)
        {
            if (!numeric(p1) || !numeric(p2)) return null;
            else if (p1 == Type.Float || p2 == Type.Float)
                return Type.Float;
            else if (p1 == Type.Int || p2 == Type.Int)
                return Type.Int;
            else return Type.Char;
        }
    }

    public class Array : Type
    {
        public Type of;
        public int size = 1;
        public Array(int sz, Type p):base("[]",Tag.INDEX,sz*p.width)
        {
            size = sz;
            of = p;
        }
        public override string ToString()
        {
            return "[" + size + "]" + of.ToString();
        }
    }
}

