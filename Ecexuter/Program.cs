using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecexuter
{
    public struct CommandOperand
    {
        public byte Type;
        public byte DataType;
        public object Value;
    }
    public class Command
    {
        public int Type;
        public CommandOperand OpAddr1;
        public CommandOperand OpAddr2;
        public int ResAddr;
    }
    public class Executer
    {
        public const int ADD = 0, SUB = 1, MUL = 2, DIV = 3, MOV = 4;

        Dictionary<String, int> vars = new Dictionary<string, int>();
        List<Command> commandList = new List<String>();
        int currCommand;

        public void AddCommand(int Type, int OpAddr1, int OpAddr2, int ResAddr)
        {
            commandList.Add(new Command(){ Type = Type, OpAddr1 = OpAddr1, OpAddr2 = OpAddr2, ResAddr =ResAddr });
        }

    }
    class Program
    {
        static void Main(string[] args)
        {

            Console.ReadKey();
        }
    }
}
