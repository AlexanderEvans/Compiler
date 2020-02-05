using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    //handles holding output and messageing at different detail levels, with optional auto coloring
    class Printer
    {
        public enum Mode { err = 1, warn = 2, info = 4, detail = 8}
        public static Mode Mask = Mode.err | Mode.warn | Mode.info;

        static int lineNumber = 1;
        public static void ErrLine(string msg, bool suppressColor = false) => Err(msg + '\n', suppressColor);

        public static void Err(string msg, bool suppressColor = false)
        {
            ConsoleColor cache = Console.ForegroundColor;
            if (!suppressColor)
                Console.ForegroundColor = ConsoleColor.Red;

            Write(msg, Mode.err);
            Console.ForegroundColor = cache;
        }

        public static void WarnLine(string msg, bool suppressColor = false) => warn(msg + '\n', suppressColor);

        public static void warn(string msg, bool suppressColor = false)
        {
            ConsoleColor cache = Console.ForegroundColor;
            if (!suppressColor)
                Console.ForegroundColor = ConsoleColor.Yellow;

            Write(msg, Mode.warn);
            Console.ForegroundColor = cache;
        }

        public static void DetLine(string msg, bool suppressColor = false) => Det(msg + '\n', suppressColor);

        public static void Det(string msg, bool suppressColor = false)
        {
            ConsoleColor cache = Console.ForegroundColor;
            if (!suppressColor)
                Console.ForegroundColor = ConsoleColor.Gray;

            Write(msg, Mode.detail);
            Console.ForegroundColor = cache;
        }

        public static void WriteLine(string msg, Mode mode = Mode.info) => Write(msg + '\n', mode);
        public static void Write(string msg, Mode mode = Mode.info)
        {
            if((Mask & mode)!=0)
            {
                string[] lines = msg.Split('\n');
                bool firstrun = true;
                foreach (string s in lines)
                {
                    if (lineNumber % 20 == 0)
                    {
                        Console.Write("\nHolding output, please press any key to continue...");
                        Console.ReadKey(true);
                    }
                    if (firstrun == false)
                    {
                        lineNumber++;
                        Console.Write('\n');
                    }
                    Console.Write(s);
                }
            }
        }
    }
}
