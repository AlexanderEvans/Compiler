using System;

namespace Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length!=2)
            {
                return 1;
            }
            Scanner scanner = new Scanner();

            Printer.WriteLine("Reading File...");
            scanner.LoadInput(FileParser.GetStringFromFile(args[2]));

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
