using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    class RecursiveDescentParser
    {
        static Scanner scanner=null;
        public static int Parse(Scanner scannerObj)
        {
            int rtnVal = 0;
            if (scannerObj == null)
                Printer.ErrLine("Scanner object must be passed to RecursiveDescentParser!");
            else
                scanner = scannerObj;

            Printer.DetLine("Beginning Recursive Descent Parsing...");

            rtnVal = MoreClasses();
            if (rtnVal != 0) return rtnVal;
            rtnVal = MainClass();
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.eofT);

            return rtnVal;
        }

        static int FormalList()
        {
            int rtnVal = 0;

            if(scanner.token == Globals.Token.intT || scanner.token == Globals.Token.booleanT || scanner.token == Globals.Token.voidT)
            {
                rtnVal = Type();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = FormalRest();
            }
            return rtnVal;
        }

        static int SeqOfStatements()
        {
            int rtnVal = 0;
            return rtnVal;
        }

        static int Expr()
        {
            int rtnVal = 0;
            return rtnVal;
        }

        static int FormalRest()
        {
            int rtnVal = 0;

            if(scanner.token==Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = FormalRest();
            }

            return rtnVal;
        }

        static int MethodDecl()
        {
            int rtnVal = 0;
            if(scanner.token== Globals.Token.publicT)
            {
                rtnVal = Match(Globals.Token.publicT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oParenT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = FormalList();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.cParenT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = SeqOfStatements();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.returnT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Expr();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = MethodDecl();
            }
            return rtnVal;
        }

        static int Type()
        {
            int rtnVal = 0;
            if (scanner.token == Globals.Token.intT)
            {
                rtnVal = Match(Globals.Token.intT);
            }
            else if(scanner.token == Globals.Token.booleanT)
            {
                rtnVal = Match(Globals.Token.booleanT);
            }
            else
            {
                rtnVal = Match(Globals.Token.voidT);
            }
            return rtnVal;
        }

        static int IdentifierList()
        {
            int rtnVal = 0;
            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            if (scanner.token== Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = IdentifierList();
                if (rtnVal != 0) return rtnVal;
            }
            return rtnVal;
        }
        static int VarDecl()
        {
            int rtnVal = 0;
            if(scanner.token == Globals.Token.finalT)
            {
                rtnVal = Match(Globals.Token.finalT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.assignOpT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.numT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl();
            }
            else if(scanner.token == Globals.Token.intT || scanner.token == Globals.Token.booleanT || scanner.token == Globals.Token.voidT)
            {
                rtnVal = Type();
                if (rtnVal != 0) return rtnVal;
                rtnVal = IdentifierList();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl();
            }
            return rtnVal;
        }

        static int ClassDecl()
        {
            int rtnVal = 0;
            rtnVal = Match(Globals.Token.classT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;

            if (scanner.token == Globals.Token.oBraceT)
            {
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = MethodDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;
            }
            else
            {
                rtnVal = Match(Globals.Token.extendsT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = MethodDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;
            }
            return rtnVal;
        }

        static int MoreClasses()
        {
            int rtnVal = 0;
            if (scanner.token == Globals.Token.classT)
            {
                rtnVal = ClassDecl();
                if (rtnVal != 0) return rtnVal;
                rtnVal = MoreClasses();
            }
            return rtnVal;
        }

        static int MainClass()
        {
            int rtnVal = 0;

            rtnVal = Match(Globals.Token.finalT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.classT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oBraceT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.publicT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.staticT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.voidT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.mainT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oParenT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.StringT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oBrackT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.cBrackT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.cParenT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oBraceT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = SeqOfStatements();
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.cBraceT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.cBraceT);
            if (rtnVal != 0) return rtnVal;
            return rtnVal;
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
