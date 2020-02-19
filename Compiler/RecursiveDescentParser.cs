﻿using System;
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

        static int Match(Globals.Token desired)
        {
            int rtnVal = 0;
            if (desired == scanner.token)
            {
                scanner.GetNextToken();
            }
            else
            {
                Printer.ErrLine("Error, was expecting \"" + desired + "\" but found \"" + scanner.token + "\"");
                rtnVal = 1;
            }
            return rtnVal;
        }
    }
}
