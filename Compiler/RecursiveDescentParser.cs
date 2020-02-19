using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    class RecursiveDescentParser
    {
        static Scanner scanner=null;
        public static void Parse(Scanner scannerObj)
        {
            if (scannerObj == null)
                Printer.ErrLine("Scanner object must be passed to RecursiveDescentParser!");
            else
                scanner = scannerObj;
        }

    }
}
