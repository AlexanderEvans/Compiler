using System;

namespace Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            Printer.DetLine("Checking input args...");
            if (args.Length!=2)//ensure we were given a filepath
            {
                return 1;
            }
            //initialize scanner
            Printer.DetLine("Initializing scanner...");
            Scanner scanner = new Scanner();

            //read file
            Printer.DetLine("Reading File...");
            scanner.LoadInput(FileParser.GetStringFromFile(args[2]));

            Printer.DetLine("Scanning File...");
            Printer.WriteLine("Token \tData");
            while (scanner.token != Globals.Token.eofT)
            {
                scanner.GetNextToken();
                Printer.Write(scanner.token+"");
                if (scanner.Value.HasValue)
                    Printer.Write(" \t" + scanner.Value.Value);
                else if (scanner.ValueR.HasValue)
                    Printer.Write(" \t" + scanner.ValueR.Value);
                else if (scanner.hasLiteral)
                    Printer.Write(" \t\"" + scanner.Literal + "\"");
                else
                    Printer.Write(" \t" + scanner.Lexeme);
                Printer.Write("\n");
            }

            return 0;
        }
    }
}
