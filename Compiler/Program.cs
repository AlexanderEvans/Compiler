using System;

namespace Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            Printer.DetLine("Checking input args...");
            if (args.Length!=1)//ensure we were given a filepath
            {
                Printer.Err("Found " + args.Length + " args... quiting");
                return 1;
            }
            //initialize scanner
            Printer.DetLine("Initializing scanner...");
            Scanner scanner = new Scanner();

            //read file
            Printer.DetLine("Reading File...\""+ args[0] + "\"");
            scanner.LoadInput(FileParser.GetStringFromFile(args[0]));

            Printer.DetLine("Scanning File...");
            Printer.WriteLine("Token      Data");
            Printer.WriteLine("===============");
            while (scanner.token != Globals.Token.eofT)
            {
                scanner.GetNextToken();
                int padFeildOne = 10;
                int tmpLen = (scanner.token + "").Length;
                Printer.Write((scanner.token + ""));
                for (int i = 0; i < (padFeildOne - tmpLen); i++)
                    Printer.Write(" ");
                if (scanner.Value.HasValue)
                    Printer.WriteLine(" " + scanner.Value.Value);
                else if (scanner.ValueR.HasValue)
                    Printer.WriteLine(" " + scanner.ValueR.Value);
                else if (scanner.hasLiteral)
                    Printer.WriteLine(" \"" + scanner.Literal + "\"");
                else
                    Printer.WriteLine(" " + scanner.Lexeme);
            }

            scanner.clrVal();
            scanner.ResetIndex();

            return RecursiveDescentParser.Parse(scanner);
        }
    }
}
