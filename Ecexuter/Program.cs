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
        List<String> commandList = new List<String>();
        int currCommandIdx;

        public void AddCommand(string command)
        {
            commandList.Add(command);
        }

        private bool ExecCommand(int commandIdx)
        {
            string commandMnem = commandList[commandIdx].Substring(0, 6).Trim();
            string commandParam = commandList[commandIdx].Substring(7, commandList[commandIdx].Length-6).Trim();
            string[] param = commandParam.Split(new char[] { ' ' });

            return false;
        }

        public bool Exec()
        {
            currCommandIdx = 0;
            for(int i=0;i<commandList.Count;i++)
            {
                ExecCommand(currCommandIdx++);

            }
            return true;
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
