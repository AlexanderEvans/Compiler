using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Compiler
{
    class RecursiveDescentParser
    {
        static Scanner scanner=null;
        static SymbolTable SymTab = null;
        static int currentDepth = 0;
        static List<string> TACs = new List<string>();
        static List<string> ASMs = new List<string>();
        static int tmpCount = 0;

        static void Dump(List<string> toDump, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (string s in toDump)
                    sw.WriteLine(s);
            }
        }

        public static void DumpTAC(string path)
        {
            Dump(TACs, path);
        }
        public static void DumpASM(string path)
        {
            Dump(ASMs, path);
        }

        public static int Parse(Scanner scannerObj, SymbolTable symbolTable)
        {
            TACs.Clear();
            ASMs.Clear();
            tmpCount = 0;
            int rtnVal = 0;
            currentDepth = 0;
            if (scannerObj == null)
                Printer.ErrLine("Scanner object must be passed to RecursiveDescentParser!");
            else
                scanner = scannerObj;
            if (symbolTable == null)
                Printer.ErrLine("SymbolTable object must be passed to RecursiveDescentParser!");
            else
                SymTab = symbolTable;

            EmitASM(".model small");
            EmitASM(".586");
            EmitASM(".stack 100h");
            EmitASM(".data");
            EmitASM(".code");
            EmitASM("S0  DB  \"String\",\"$\"");
            EmitASM("S1  DB  \"Next String\",\"$\"");
            EmitASM("include io.asm");
            EmitASM("");

            Printer.DetLine("Beginning Recursive Descent Parsing...");

            rtnVal = MoreClasses();
            if (rtnVal != 0) return rtnVal;
            rtnVal = MainClass();
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.eofT);

            if (SymTab.DepthHasEntries(currentDepth))
                SymTab.WriteTable(0);

            return rtnVal;
        }

        public struct FormalData
        {
            public int totalSizeOfLocals;
            public int paramCount;
            public List<SymbolTable.VariableType> variableTypes;
        }

        static int FormalList(out FormalData formalData, int offset=4)
        {
            int rtnVal = 0;
            formalData = default;
            if (scanner.token == Globals.Token.intT || scanner.token == Globals.Token.booleanT || scanner.token == Globals.Token.voidT)
            {
                rtnVal = Type(out Globals.Token type);
                if (rtnVal != 0) return rtnVal;


                SymbolTable.SymbolTableEntryVariable stev = (SymbolTable.SymbolTableEntryVariable) SymTab.Insert(scanner.Lexeme, type, currentDepth, SymbolTable.EntryType.tVariable);

                if (stev == null)
                {
                    rtnVal = 4;
                    Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                    return rtnVal;
                }

                stev.offset = offset;
                stev.size = DetermineSize(type);
                switch (type)
                {
                    case Globals.Token.intT:
                        stev.variableType = SymbolTable.VariableType.tInt; break;
                    case Globals.Token.StringT:
                        stev.variableType = SymbolTable.VariableType.tString; break;
                    case Globals.Token.booleanT:
                        stev.variableType = SymbolTable.VariableType.tBool; break;
                }

                offset += DetermineSize(type);

                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = FormalRest(out FormalData formalDataInt, offset);
                formalData = formalDataInt;

                switch(type)
                {
                    case Globals.Token.intT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tInt); break;
                    case Globals.Token.booleanT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tBool); break;
                    case Globals.Token.StringT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tString); break;
                    default:
                        formalData.variableTypes.Add(default);
                        Printer.warn("Warning: '" + type + "' not implemented yet!"); 
                        break;
                }
                formalData.totalSizeOfLocals += DetermineSize(type);
                formalData.paramCount++;
            }
            return rtnVal;
        }

        static int SeqOfStatements()
        {
            int rtnVal = 0;
            if (scanner.token == Globals.Token.idT)
            {
                rtnVal = Statement();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = StatTail();
            }
            return rtnVal;
        }

        static int StatTail()
        {
            int rtnVal = 0;
            if (scanner.token == Globals.Token.idT)
            {
                rtnVal = Statement();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = StatTail();
            }
            return rtnVal;
        }
        static int Statement()
        {
            int rtnVal = 0;
            if(scanner.Lexeme == "read" | scanner.Lexeme == "writeln" | scanner.Lexeme == "write")
            {
                rtnVal = IOStat();
            }
            else if (scanner.token == Globals.Token.idT)
            {
                rtnVal = AssignStat();
            }
            return rtnVal;
        }


        static int AssignStat()
        {
            int rtnVal = 0;

            SymbolTable.SymbolTableEntry tmp = new SymbolTable.SymbolTableEntry();
            tmp.lex = scanner.Lexeme;
            tmp.token = scanner.token;
            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            if(scanner.token==Globals.Token.assignOpT)
            {
                //do assignment
                //...code...
                //end assignment

                rtnVal = Match(Globals.Token.assignOpT);
                if (rtnVal != 0) return rtnVal;
                string result;
                (rtnVal, result) = Expr();
                if (rtnVal != 0) return rtnVal;
                Emit("");
                Emit(tmp.lex + " = " + result);
                EmitASM("");
                EmitASM("MOV "+ tmp.lex + ", "+ result);
            }
            else if(scanner.token==Globals.Token.dotT)
            {
                rtnVal = Match(Globals.Token.dotT);
                if (rtnVal != 0) return rtnVal;
                MethodCall(tmp.lex);
                Emit("");
                Emit(tmp.lex + " = AX");
                EmitASM("");
                EmitASM("MOV " + tmp.lex + ", AX");
            }
            else
            {
                rtnVal = 15;
                Printer.ErrLine("Couldn't parse AssignStat, Error Code: 15!");
            }

            return rtnVal;
        }

        static int MethodCall(string parentClassName)
        {
            int rtnVal = 0;

            SymbolTable.SymbolTableEntry entSymbol;
            int depth;
            (entSymbol, depth)= SymTab.LookUp(parentClassName);

            string methodName = scanner.Lexeme;

            bool isValid = false;
            bool fromOtherClass = false;
            if (entSymbol != null && entSymbol is SymbolTable.SymbolTableEntryClass)
            {
                SymbolTable.SymbolTableEntryClass symClass = (SymbolTable.SymbolTableEntryClass)entSymbol;
                if(symClass.methodNames.Contains(scanner.Lexeme))
                {
                    isValid = true;
                    fromOtherClass = true;
                }
            }

            bool ismethod = false;
            if(!isValid)
            {
                (entSymbol, depth) = SymTab.LookUp(scanner.Lexeme);
                if (entSymbol != null)
                {
                     isValid = true;
                    if (entSymbol is SymbolTable.SymbolTableEntryMethod)
                        ismethod = true;
                }
            }

            if (isValid!=true)
            {
                Printer.ErrLine("Error!  Use of undeclared identifier '" + methodName  + "'!");
                Match(Globals.Token.idT);
                rtnVal = 12;
                Printer.ErrLine("Error Code: " + rtnVal);
            }
            else
            {
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                if (ismethod | fromOtherClass)
                {
                    rtnVal = Match(Globals.Token.oParenT);
                    if (rtnVal != 0) return rtnVal;
                    rtnVal = Params();
                    if (rtnVal != 0) return rtnVal;
                    rtnVal = Match(Globals.Token.cParenT);
                    Emit("");
                    Emit("Call " + methodName);
                    Emit("");
                    EmitASM("");
                    EmitASM("Call " + methodName);
                    EmitASM("");
                }
                else
                {
                    Printer.ErrLine("Error!  Use of undeclared identifier '" + methodName + "'! [not a method...]");
                    rtnVal = 13;
                    Printer.ErrLine("Error Code: "+rtnVal);
                }
            }
            return rtnVal;
        }
        
        static int Params()
        {
            int rtnVal = 0;
            List<string> PUSH_TACs = null;
            if (scanner.token == Globals.Token.idT)
            {
                SymbolTable.SymbolTableEntry ent;
                int depth;
                (ent, depth)= SymTab.LookUp(scanner.Lexeme);

                if (ent == null)
                {
                    Printer.ErrLine("Error!  Are you sure '" + scanner.Lexeme + "' was declared?");
                    rtnVal = Match(Globals.Token.idT);
                }
                else if (ent is SymbolTable.SymbolTableEntryVariable)
                {
                    SymbolTable.SymbolTableEntryVariable symVar = (SymbolTable.SymbolTableEntryVariable)ent;
                    string operationSignOption = symVar.offset < 0 ? "" + symVar.offset : "+" + symVar.offset;
                    string LeftHandVarResult = "[BP" + operationSignOption + "]";

                    string tmpHold = LeftHandVarResult;
                    Match(Globals.Token.idT);
                    (rtnVal, PUSH_TACs) = ParamsTail();

                    if (PUSH_TACs == null)
                        PUSH_TACs = new List<string>();

                    PUSH_TACs.Add("PUSH " + tmpHold);
                }
                else
                {
                    Printer.ErrLine("Error, " + scanner.Lexeme + "is not a variable!");
                    rtnVal = Match(Globals.Token.idT);
                }
            }
            else if (scanner.token == Globals.Token.numT)
            {
                string tmpHold = scanner.Lexeme;
                Match(Globals.Token.numT);
                (rtnVal, PUSH_TACs) = ParamsTail();
                PUSH_TACs.Add("PUSH " + tmpHold);
            }

            if (PUSH_TACs != null)
            {
                Emit(PUSH_TACs);
                EmitASM(PUSH_TACs); //fix
            }

            return rtnVal;
        }

        static (int, List<string>) ParamsTail()
        {
            (int rtnVal, List<string> PUSH_TACs)= (0,null);

            if (scanner.token == Globals.Token.commaT)
            {
                Match(Globals.Token.commaT);
                if (scanner.token == Globals.Token.idT)
                {
                    SymbolTable.SymbolTableEntry ent;
                    (ent, _)= SymTab.LookUp(scanner.Lexeme);
                    if (ent == null)
                    {
                        Printer.ErrLine("Error!  Are you sure '" + scanner.Lexeme + "' was declared?");
                        rtnVal = Match(Globals.Token.idT);
                    }
                    else if (ent is SymbolTable.SymbolTableEntryVariable)
                    {
                        string tmpHold = scanner.Lexeme;
                        Match(Globals.Token.idT);
                        (rtnVal, PUSH_TACs) = ParamsTail();

                        if (PUSH_TACs == null)
                            PUSH_TACs = new List<string>();

                        PUSH_TACs.Add("PUSH " + tmpHold);
                    }
                    else
                    {
                        Printer.ErrLine("Error, " + scanner.Lexeme + "is not a variable!");
                        rtnVal = Match(Globals.Token.idT);
                    }
                }
                else
                {
                    string tmpHold = scanner.Lexeme;
                    rtnVal = Match(Globals.Token.numT);
                    if (rtnVal != 0) return (rtnVal, PUSH_TACs);
                    (rtnVal, PUSH_TACs) = ParamsTail();
                    PUSH_TACs.Add("PUSH " + tmpHold);
                }
            }

            return (rtnVal, PUSH_TACs);
        }

        static int IOStat()
        {
            int rtnVal = 0;

            if(scanner.Lexeme=="write" | scanner.Lexeme == "writeln")
            {
                Out_STAT();
            }
            else if (scanner.Lexeme == "read")
            {
                rtnVal = INSTAT();
            }

            return rtnVal;
        }

        static int Out_STAT()
        {
            int rtnVal = 0;

            if (scanner.Lexeme == "write")
            {
                Write_List();
            }
            else
            {
                if (scanner.Lexeme != "writeln")
                {
                    Printer.ErrLine("Unrecognized IO Statement Lexeme: " + scanner.Lexeme);
                    return 10;
                }
                Write_List(true);
            }
            return rtnVal;
        }

        static int Write_List(bool isLine=false)
        {
            int rtnVal = 0;

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oParenT);
            if (rtnVal != 0) return rtnVal;
            Write_Token(isLine);
            if (rtnVal != 0) return rtnVal;
            Write_List_Tail(isLine);

            rtnVal = Match(Globals.Token.cParenT);
            if (rtnVal != 0) return rtnVal;

            return rtnVal;
        }
        static int Write_List_Tail(bool isLine = false)
        {
            int rtnVal = 0;

            if (scanner.token == Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                Write_Token(isLine);
                if (rtnVal != 0) return rtnVal;
                Write_List_Tail(isLine);
            }

            return rtnVal;
        }

        static int Write_Token(bool isLine = false)
        {
            int rtnVal = 0;

            if(scanner.token==Globals.Token.idT)
            {
                (SymbolTable.SymbolTableEntry, int depth) sym = SymTab.LookUp(scanner.Lexeme);
                if (sym.Item1 is SymbolTable.SymbolTableEntryVariable)
                {
                    SymbolTable.SymbolTableEntryVariable tmp = (SymbolTable.SymbolTableEntryVariable)(sym.Item1);
                    string operationSignOption = tmp.offset < 0 ? "" + tmp.offset : "+" + tmp.offset;
                    string LeftHandVarResult = "[BP" + operationSignOption + "]";
                    if (tmp.token == Globals.Token.intT)
                    {
                        EmitASM("MOV AX, " + LeftHandVarResult);
                        EmitASM("Call writeint");
                    }
                    else
                    {
                        Printer.ErrLine("Error! " + tmp.lex + "(" + tmp.token + ") is not implemented for writing!");
                    }
                }
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
            }
            else if(scanner.token==Globals.Token.numT)
            {
                EmitASM("MOV AX, " + scanner.Lexeme);
                EmitASM("Call writeint");

                rtnVal = Match(Globals.Token.numT);
                if (rtnVal != 0) return rtnVal;
            }
            else if (scanner.token == Globals.Token.dQuoteT)
            {
                EmitASM("_t"+tmpCount+" db \"student1$\"");
                EmitASM("MOV DX, _t" + tmpCount);
                tmpCount++;
                EmitASM("Call writestr");
                rtnVal = Match(Globals.Token.dQuoteT);
                if (rtnVal != 0) return rtnVal;
            }

            if(isLine)
                EmitASM("Call writeln");

            return rtnVal;
        }

        private static int INSTAT()
        {
            int rtnVal = 0;

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oParenT);
            if (rtnVal != 0) return rtnVal;

            rtnVal = Id_List();
            if (rtnVal != 0) return rtnVal;

            rtnVal = Match(Globals.Token.cParenT);
            if (rtnVal != 0) return rtnVal;
            return rtnVal;
        }

        private static int Id_List()
        {
            int rtnVal = 0;
            (SymbolTable.SymbolTableEntry, int depth) sym = SymTab.LookUp(scanner.Lexeme);
            if(sym.Item1 is SymbolTable.SymbolTableEntryVariable)
            {
                SymbolTable.SymbolTableEntryVariable tmp = (SymbolTable.SymbolTableEntryVariable)(sym.Item1);
                string operationSignOption = tmp.offset < 0 ? "" + tmp.offset : "+" + tmp.offset;
                string LeftHandVarResult = "[BP" + operationSignOption + "]";
                if(tmp.token==Globals.Token.intT)
                {
                    EmitASM("Call readint");
                    EmitASM("MOV "+LeftHandVarResult+", BX");
                }
                else
                {
                    Printer.ErrLine("Error!  (" + tmp.token + ") is not implemented for reading!");
                }
            }

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Id_List_Tail();
            return rtnVal;
        }
        private static int Id_List_Tail()
        {
            int rtnVal = 0;
            if(scanner.token==Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                (SymbolTable.SymbolTableEntry, int depth) sym = SymTab.LookUp(scanner.Lexeme);
                if (sym.Item1 is SymbolTable.SymbolTableEntryVariable)
                {
                    SymbolTable.SymbolTableEntryVariable tmp = (SymbolTable.SymbolTableEntryVariable)(sym.Item1);
                    string operationSignOption = tmp.offset < 0 ? "" + tmp.offset : "+" + tmp.offset;
                    string LeftHandVarResult = "[BP" + operationSignOption + "]";
                    if (tmp.token == Globals.Token.intT)
                    {
                        EmitASM("Call readint");
                        EmitASM("MOV " + LeftHandVarResult + ", BX");
                    }
                    else
                    {
                        Printer.ErrLine("Error! "+tmp.lex+"(" + tmp.token + ") is not implemented for reading!");
                    }
                    rtnVal = Match(Globals.Token.intT);
                    if (rtnVal != 0) return rtnVal;
                    Id_List_Tail();
                }
            }

            return rtnVal;
        }
        static (int, string) Expr()
        {
            Printer.DetLine("Resolving expresion...");
            int rtnVal = 0;
            string result = "";
            if (IsFactorTokenStarter())
            {
                (rtnVal, result) = Relation();
            }
            return (rtnVal, result);
        }

        static (int ErrorCode, string result) Relation()
        {
            int rtnVal = 0;
            string result;
            (rtnVal, result) = SimpleExpr();
            return (rtnVal, result);
        }
        static (int ErrorCode, string result) SimpleExpr()
        {
            int rtnVal = 0;
            string result;
            (rtnVal, result) = Term();
            if (rtnVal != 0) return (rtnVal, result);
            (rtnVal, result) = MoreTerm(result);
            return (rtnVal, result);
        }

        static (int ErrorCode, string Result) MoreTerm(string left)
        {
            int rtnVal = 0;
            string result = left;


            EmitASM("PUSH AX");

            Globals.Token type = Globals.Token.intT;

            SymbolTable.SymbolTableEntryVariable stev = (SymbolTable.SymbolTableEntryVariable)SymTab.Insert("_t"+tmpCount, type, currentDepth, SymbolTable.EntryType.tVariable);

            int prevOffset = SymTab.GetOffsetAtDepth(currentDepth);
            Globals.Token prevType = SymTab.GetTypeAtOffset(currentDepth, prevOffset);
            stev.offset =prevOffset - DetermineSize(prevType);

            string operationSignOption = stev.offset < 0 ? "" + stev.offset : "+" + stev.offset;
            string LeftHandVarResult = "[BP" + operationSignOption + "]";
            tmpCount++;

         

            if (scanner.token==Globals.Token.addOpT)
            {

                string tmpOpCache = scanner.Lexeme;
                rtnVal = Addop();
                if (rtnVal != 0) return (rtnVal, result);
                (rtnVal, result) = Term();
                if (rtnVal != 0) return (rtnVal, result);

                if (tmpOpCache == "+")
                {
                    Emit(LeftHandVarResult + " = " + left + " + " + result);
                    EmitASM("MOV AX, " + left);
                    EmitASM("ADD AX, " + result);
                    EmitASM("MOV " + LeftHandVarResult + ", AX");
                }
                else if (tmpOpCache == "-")
                {
                    Emit(LeftHandVarResult + " = " + left + " - " + result);
                    EmitASM("MOV AX, " + left);
                    EmitASM("SUB AX, " + result);
                    EmitASM("MOV " + LeftHandVarResult + ", AX");
                }

                (rtnVal, result) = MoreTerm(LeftHandVarResult);
            }
            return (rtnVal, result);
        }
        static (int ErrorCode, string rtnStr) Term()
        {
            int rtnVal = 0;
            string rtnStr = null;

            (rtnVal, rtnStr) = Factor();
            if (rtnVal != 0) return (rtnVal, rtnStr);

            (rtnVal, rtnStr) = MoreFactor(rtnStr);
            return (rtnVal, rtnStr);
        }
        static (int ErrorCode, string result) MoreFactor(string left)
        {
            int rtnVal = 0;
            string result;

            EmitASM("PUSH AX");

            Globals.Token type = Globals.Token.intT;

            SymbolTable.SymbolTableEntryVariable stev = (SymbolTable.SymbolTableEntryVariable)SymTab.Insert("_t"+tmpCount, type, currentDepth, SymbolTable.EntryType.tVariable);

            int prevOffset = SymTab.GetOffsetAtDepth(currentDepth);
            Globals.Token prevType = SymTab.GetTypeAtOffset(currentDepth, prevOffset);
            stev.offset =  prevOffset- DetermineSize(prevType);

            string operationSignOption = stev.offset < 0 ? "" + stev.offset : "+" + stev.offset;
            string LeftHandVarResult = "[BP" + operationSignOption + "]";
            tmpCount++;
            //SymTab.Insert

            if (scanner.token == Globals.Token.mulOpT)
            {
                string tmpOpCache = scanner.Lexeme;
                rtnVal = Mulop();
                if (rtnVal != 0) return (rtnVal, null);
                (rtnVal, result) = Factor();
                if (rtnVal != 0) return (rtnVal, null);
                if(tmpOpCache == "*")
                {
                    Emit(LeftHandVarResult + " = " + left + " * " + result);
                    EmitASM("MOV AX, " + left);
                    EmitASM("MUL AX, " + result);
                    EmitASM("MOV " + LeftHandVarResult + ", AX");
                }
                else if(tmpOpCache == "/")
                {
                    Emit(LeftHandVarResult + " = " + left + " / " + result);
                    EmitASM("MOV AX, " + left);
                    EmitASM("DIV AX, " + result);
                    EmitASM("MOV " + LeftHandVarResult + ", AX");
                }

                (rtnVal, result) = MoreFactor(LeftHandVarResult);
                if (rtnVal != 0) return (rtnVal, null);
            }
            else
            {
                Emit(LeftHandVarResult + " = " + left);
                EmitASM("MOV AX, " + left);
                EmitASM("MOV " + LeftHandVarResult + ", AX");
            }
            return (rtnVal, LeftHandVarResult);
        }
        static (int, string) Factor()
        {
            int rtnVal = 0;
            string rtnStr = "";
            if (scanner.token==Globals.Token.idT)
            {

                SymbolTable.SymbolTableEntry ent;
                int depth;
                (ent, depth)= SymTab.LookUp(scanner.Lexeme);
                

                if (ent == null)
                {
                    Printer.ErrLine("Error!  Use of undeclared identifier '"+scanner.Lexeme+"'!");
                    rtnVal = 11;
                    Printer.ErrLine("Error Code: " + rtnVal);
                }
                else if(ent is SymbolTable.SymbolTableEntryClass)
                {
                    string parentClass=scanner.Lexeme;
                    rtnVal = Match(Globals.Token.idT);
                    if (rtnVal != 0) return (rtnVal, rtnStr);
                    rtnVal = Match(Globals.Token.dotT);
                    if (rtnVal != 0) return (rtnVal, rtnStr);
                    rtnVal = MethodCall(parentClass);
                    rtnStr = "AX";
                    if (rtnVal != 0) return (rtnVal, rtnStr);
                }
                else if(ent is SymbolTable.SymbolTableEntryVariable || ent is SymbolTable.SymbolTableEntryConstant)
                {
                    string operationSignOption="";
                    //bool useBP = true;
                    if (ent is SymbolTable.SymbolTableEntryVariable)
                    {
                        SymbolTable.SymbolTableEntryVariable symVar = (SymbolTable.SymbolTableEntryVariable)ent;
                        operationSignOption = symVar.offset < 0 ? "" + symVar.offset : "+" + symVar.offset;
                        //if (symVar.offset < 0)
                        //    useBP = false;
                    }
                    if (ent is SymbolTable.SymbolTableEntryConstant)
                    {
                        SymbolTable.SymbolTableEntryConstant symVar = (SymbolTable.SymbolTableEntryConstant)ent;
                        operationSignOption = symVar.offset < 0 ? "" + symVar.offset : "+" + symVar.offset;
                        //if (symVar.offset < 0)
                        //    useBP = false;
                    }
                    string LeftHandVarResult;
                    //if (useBP)
                    LeftHandVarResult = "[BP" + operationSignOption + "]";
                    //else
                    //{
                    //    if (currentDepth - 1 >= depth)
                    //    {
                    //        EmitASM("MOV, BX, BP");
                    //        EmitASM("MOV, AX, [BP+0]");
                    //        EmitASM("SUB, AX, BX");
                    //        depth++;
                    //    }
                    //    while (depth != currentDepth)
                    //    {
                    //        depth++;

                    //        EmitASM("MOV, BX, [AX+0]");
                    //        EmitASM("SUB, AX, BX");
                    //    }

                    //    EmitASM("MOV, BX, AX");
                    //    LeftHandVarResult = "[BX" + operationSignOption + "]";
                    //}
                    rtnStr = LeftHandVarResult;
                    rtnVal = Match(Globals.Token.idT);
                    if (rtnVal != 0) return (rtnVal, rtnStr);
                }

            }
            else if (scanner.token == Globals.Token.numT)
            {
                if (scanner.Value.HasValue)
                    rtnStr = scanner.Value.Value.ToString();
                if (scanner.ValueR.HasValue)
                    rtnStr = scanner.ValueR.Value.ToString();
                ConsoleColor tmpColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Printer.DetLine("Detected literal: "+rtnStr, true);
                Console.ForegroundColor = tmpColor;
                rtnVal = Match(Globals.Token.numT);
            }
            else if (scanner.token == Globals.Token.oParenT)
            {
                rtnVal = Match(Globals.Token.oParenT);
                if (rtnVal != 0) return (rtnVal, rtnStr);
                (rtnVal, rtnStr) = Expr();
                if (rtnVal != 0) return (rtnVal, rtnStr);
                rtnVal = Match(Globals.Token.cParenT);
            }
            else if (scanner.token == Globals.Token.trueT)
            {
                rtnStr = scanner.Lexeme;
                rtnVal = Match(Globals.Token.trueT);
            }
            else if (scanner.token == Globals.Token.falseT)
            {
                rtnStr = scanner.Lexeme;
                rtnVal = Match(Globals.Token.falseT);
            }
            else if (scanner.Lexeme == "!")
            {
                rtnVal = Match(Globals.Token.relOpT);
                if (rtnVal != 0) return (rtnVal, rtnStr);
                (rtnVal, rtnStr) = Factor();
            }
            else if (scanner.Lexeme == "-")
            {
                rtnVal = Signop();
                if (rtnVal != 0) return (rtnVal, rtnStr);
                (rtnVal, rtnStr) = Factor();
            }
            else
            {
                Printer.ErrLine("Error!  Could not resolve expresion!");
                rtnVal = 2;
            }
            return (rtnVal, rtnStr);
        }

        static int Signop()
        {
            int rtnVal = 0;
            rtnVal = Match(Globals.Token.addOpT);
            return rtnVal;
        }
        static int Addop()
        {
            int rtnVal = 0;
            rtnVal = Match(Globals.Token.addOpT);
            return rtnVal;
        }
        static int Mulop()
        {
            int rtnVal = 0;
            rtnVal = Match(Globals.Token.mulOpT);
            return rtnVal;
        }

        static bool IsFactorTokenStarter()
        {
            Globals.Token token = scanner.token;
            return (token == Globals.Token.idT || token == Globals.Token.numT || token == Globals.Token.oParenT || token == Globals.Token.trueT || token == Globals.Token.falseT || scanner.Lexeme == "!" || scanner.Lexeme == "-") ?  true:  false;
        }

        static int FormalRest(out FormalData formalData, int offset)
        {
            int rtnVal = 0;

            formalData = default;

            if(scanner.token==Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type(out Globals.Token type);
                if (rtnVal != 0) return rtnVal;

                SymbolTable.SymbolTableEntryVariable stev = (SymbolTable.SymbolTableEntryVariable)SymTab.Insert(scanner.Lexeme, type, currentDepth, SymbolTable.EntryType.tVariable);

                if(stev==null)
                {
                    rtnVal = 4;
                    Printer.ErrLine("Error!  Duplicate entries ("+scanner.Lexeme+") at depth "+currentDepth+"!");
                    return rtnVal;
                }

                stev.offset = offset;
                stev.size = DetermineSize(type);
                switch (type)
                {
                    case Globals.Token.intT:
                        stev.variableType = SymbolTable.VariableType.tInt; break;
                    case Globals.Token.StringT:
                        stev.variableType = SymbolTable.VariableType.tString; break;
                    case Globals.Token.booleanT:
                        stev.variableType = SymbolTable.VariableType.tBool; break;
                }

                offset += DetermineSize(type);

                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = FormalRest(out FormalData formalDataInt, offset);


                formalData = formalDataInt;
                switch (type)
                {
                    case Globals.Token.intT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tInt); break;
                    case Globals.Token.booleanT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tBool); break;
                    case Globals.Token.StringT:
                        formalData.variableTypes.Add(SymbolTable.VariableType.tString); break;
                    default:
                        formalData.variableTypes.Add(default);
                        Printer.warn("Warning: '" + type + "' not implemented yet!"); 
                        break;
                }
                formalData.totalSizeOfLocals += DetermineSize(type);
                formalData.paramCount++;
            }
            else
            {
                formalData.variableTypes = new List<SymbolTable.VariableType>();
            }

            return rtnVal;
        }

        static int MethodDecl(out List<string> names)
        {
            names = default;
            int rtnVal = 0;
            if(scanner.token== Globals.Token.publicT)
            {
                rtnVal = Match(Globals.Token.publicT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type(out Globals.Token returnType);


                SymbolTable.SymbolTableEntryMethod stem = null;
                if (scanner.token == Globals.Token.idT)
                    stem = (SymbolTable.SymbolTableEntryMethod)SymTab.Insert(scanner.Lexeme, returnType, currentDepth, SymbolTable.EntryType.tMethod);

                if (stem == null)
                {
                    rtnVal = 4;
                    Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                    return rtnVal;
                }
                //stem. = 0;
                //stem.size = DetermineSize(returnType); //size of retValue
                //switch (returnType)
                //{
                //    case Globals.Token.intT:
                //        stem.variableType = SymbolTable.VariableType.tInt; break;
                //    case Globals.Token.StringT:
                //        stem.variableType = SymbolTable.VariableType.tString; break;
                //    case Globals.Token.booleanT:
                //        stem.variableType = SymbolTable.VariableType.tBool; break;
                //}

                //offset -= DetermineSize(Globals.Token.StringT);
                if (rtnVal != 0) return rtnVal;


                EmitASM("");
                EmitASM(scanner.Lexeme+" proc");
                EmitASM("push bp");
                EmitASM("mov bp, s");
                int insertionPoint = ASMs.Count;
                //EmitASM("sub sp,"+ stem.paramCount);
                EmitASM("");
                Emit("");



                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oParenT);
                if (rtnVal != 0) return rtnVal;

                currentDepth++;

                rtnVal = FormalList(out FormalData formalData);
                if (rtnVal != 0) return rtnVal;

                stem.paramCount = formalData.paramCount;
                stem.totalSizeOfLocals = formalData.totalSizeOfLocals;
                stem.variableTypes = formalData.variableTypes;

                rtnVal = Match(Globals.Token.cParenT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl(out VarDeclData varDeclData);
                if (rtnVal != 0) return rtnVal;
                stem.paramCount += varDeclData.paramCount;
                stem.totalSizeOfLocals += varDeclData.totalSizeOfLocals;
                if (stem.variableTypes == null)
                    stem.variableTypes = new List<SymbolTable.VariableType>();
                if(varDeclData.variableTypes!=null)
                    stem.variableTypes.AddRange(varDeclData.variableTypes);

                ASMs.Insert(insertionPoint, "sub sp," + varDeclData.totalSizeOfLocals);

                //EmitASM("call Test");
                rtnVal = SeqOfStatements();
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.returnT);
                if (rtnVal != 0) return rtnVal;

                string resultStr = "";
                //EmitASM("call Test");
                EmitASM("");
                Emit("");
                (rtnVal, resultStr) = Expr();
                if (rtnVal != 0) return rtnVal;

                if (resultStr!=null && resultStr != "")
                {
                    EmitASM("");
                    Emit("AX = " + resultStr);
                }
                else
                {
                    //do nothing?
                }

                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;

                EmitASM("");
                EmitASM("add sp,"+ ((SymTab.GetOffsetAtDepth(currentDepth)*-1)-2));
                EmitASM("pop bp");
                EmitASM("ret");
                EmitASM(stem.lex + " endp");
                EmitASM("");

                if (SymTab.DepthHasEntries(currentDepth))
                    SymTab.WriteTable(currentDepth);
                SymTab.DeleteDepth(currentDepth);
                currentDepth--;

                rtnVal = MethodDecl(out List<string> namesInt);
                names = namesInt;
                if (names == null)
                    names = new List<string>();
                names.Add(stem.lex);

            }
            else
            {
                names = new List<string>();
            }

            return rtnVal;
        }

        static int Type(out Globals.Token token)
        {
            int rtnVal = 0;
            if (scanner.token == Globals.Token.intT)
            {
                rtnVal = Match(Globals.Token.intT);
                token = Globals.Token.intT;
            }
            else if(scanner.token == Globals.Token.booleanT)
            {
                rtnVal = Match(Globals.Token.booleanT);
                token = Globals.Token.booleanT;
            }
            else
            {
                rtnVal = Match(Globals.Token.voidT);
                token = Globals.Token.voidT;
            }
            return rtnVal;
        }

        static int IdentifierList(out List<string> names, out int count, Globals.Token type, ref int offset)
        {
            int rtnVal = 0;
            names = default;
            count = default;
            SymbolTable.SymbolTableEntryVariable stev = null;
            if (scanner.token == Globals.Token.idT)
                stev = (SymbolTable.SymbolTableEntryVariable)SymTab.Insert(scanner.Lexeme, type, currentDepth, SymbolTable.EntryType.tVariable);

            if (stev == null)
            {
                rtnVal = 4;
                Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                return rtnVal;
            }
            stev.offset = offset;
            stev.size = DetermineSize(type);
            switch (type)
            {
                case Globals.Token.intT:
                    stev.variableType = SymbolTable.VariableType.tInt; break;
                case Globals.Token.StringT:
                    stev.variableType = SymbolTable.VariableType.tString; break;
                case Globals.Token.booleanT:
                    stev.variableType = SymbolTable.VariableType.tBool; break;
            }

            offset -= DetermineSize(type);

            string operationSignOption = stev.offset < 0 ? "" + stev.offset : "+" + stev.offset;
            string BPRegisterTermMath = "[BP" + operationSignOption + "]";

            Emit("");
            Emit(type + " " + stev.lex+"; //PUSH AX to create " + BPRegisterTermMath);
            //EmitASM("MOV AX, " + " [BP" + stev.offset + "]");
            EmitASM("PUSH AX");

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;

            if (scanner.token == Globals.Token.commaT)
            {
                rtnVal = Match(Globals.Token.commaT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = IdentifierList(out List<string> namesInt, out int countInt, type, ref offset);
                count = countInt + 1;
                names = namesInt;
                if (names == null)
                    names = new List<string>();
                names.Add(stev.lex);
                if (rtnVal != 0) return rtnVal;
            }
            else
            {
                names = new List<string>();
                count = 1;
            }
            return rtnVal;
        }

        struct VarDeclData
        {
            public int totalSizeOfLocals;
            public int paramCount;
            public List<SymbolTable.VariableType> variableTypes;
            public List<string> variableNames;
        }

        static int VarDecl(out VarDeclData varDeclData, int offset=-2)
        {
            varDeclData = default;
            varDeclData.totalSizeOfLocals = 0;
            int rtnVal = 0;
            if(scanner.token == Globals.Token.finalT)
            {
                rtnVal = Match(Globals.Token.finalT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Type(out Globals.Token type);
                if (rtnVal != 0) return rtnVal;


                SymbolTable.SymbolTableEntryConstant stev = null;
                if (scanner.token == Globals.Token.idT)
                    stev = (SymbolTable.SymbolTableEntryConstant)SymTab.Insert(scanner.Lexeme, type, currentDepth, SymbolTable.EntryType.tConstant);

                if (stev == null)
                {
                    rtnVal = 4;
                    Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                    return rtnVal;
                }


                stev.offset = offset;
                stev.size = DetermineSize(type);
                switch (type)
                {
                    case Globals.Token.intT:
                        stev.variableType = SymbolTable.VariableType.tInt; break;
                    case Globals.Token.StringT:
                        stev.variableType = SymbolTable.VariableType.tString; break;
                    case Globals.Token.booleanT:
                        stev.variableType = SymbolTable.VariableType.tBool; break;
                }

                offset += DetermineSize(type);

                string lefthandRecipient=scanner.Lexeme;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.assignOpT);
                if (rtnVal != 0) return rtnVal;

                string result;
                (rtnVal, result)=Expr();
                if (rtnVal != 0) return rtnVal;
                Emit("");
                Emit(lefthandRecipient + " = " + result);
                EmitASM("");
                EmitASM("MOV AX, " + result);
                string operationSignOption = stev.offset < 0 ? "" + stev.offset : "+"+stev.offset;
                EmitASM("MOV [BP" + operationSignOption + "], AX");

                //EmitASM("PUSH AX");


                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl(out VarDeclData varDeclDataInt, offset);

                varDeclData = varDeclDataInt;
                varDeclData.paramCount++;
                varDeclData.totalSizeOfLocals += DetermineSize(type);

                switch (type)
                {
                    case Globals.Token.intT:
                        varDeclData.variableTypes.Add(SymbolTable.VariableType.tInt); break;
                    case Globals.Token.booleanT:
                        varDeclData.variableTypes.Add(SymbolTable.VariableType.tBool); break;
                    case Globals.Token.StringT:
                        varDeclData.variableTypes.Add(SymbolTable.VariableType.tString); break;
                    default:
                        varDeclData.variableTypes.Add(default);
                        Printer.warn("Warning: '" + type + "' not implemented yet!");
                        break;
                }
                varDeclData.variableNames.Add(stev.lex);
            }
            else if(scanner.token == Globals.Token.intT || scanner.token == Globals.Token.booleanT || scanner.token == Globals.Token.voidT)
            {
                rtnVal = Type(out Globals.Token type);
                if (rtnVal != 0) return rtnVal;
                rtnVal = IdentifierList(out List<string> names, out int countInt, type, ref offset);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.semicolonT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl(out VarDeclData varDeclDataInt, offset);

                varDeclData = varDeclDataInt;
                varDeclData.paramCount+=countInt;
                varDeclData.totalSizeOfLocals += (DetermineSize(type) * countInt);
                varDeclData.variableNames.AddRange(names);
                for(int i = 0; i<countInt; i++)
                    switch (type)
                    {
                        case Globals.Token.intT:
                            varDeclData.variableTypes.Add(SymbolTable.VariableType.tInt); break;
                        case Globals.Token.booleanT:
                            varDeclData.variableTypes.Add(SymbolTable.VariableType.tBool); break;
                        case Globals.Token.StringT:
                            varDeclData.variableTypes.Add(SymbolTable.VariableType.tString); break;
                        default:
                            varDeclData.variableTypes.Add(default);
                            Printer.warn("Warning: '" + type + "' not implemented yet!");
                            break;
                    }
            }
            else
            {
                varDeclData.variableTypes = new List<SymbolTable.VariableType>();
                varDeclData.variableNames = new List<string>();
            }
            return rtnVal;
        }

        static int ClassDecl()
        {
            int rtnVal = 0;

            rtnVal = Match(Globals.Token.classT);
            if (rtnVal != 0) return rtnVal;

            SymbolTable.SymbolTableEntryClass stec = default;
            if (scanner.token == Globals.Token.idT)
                stec=(SymbolTable.SymbolTableEntryClass) SymTab.Insert(scanner.Lexeme, Globals.Token.classT, currentDepth, SymbolTable.EntryType.tClass);

            EmitASM("");
            EmitASM(scanner.Lexeme + " proc");
            EmitASM("push bp");
            EmitASM("mov bp, s");
            int insertionPoint = ASMs.Count;
            //EmitASM("sub sp,"+ stem.paramCount);
            EmitASM("");
            Emit("");

            if (stec == null)
            {
                rtnVal = 4;
                Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                return rtnVal;
            }
            currentDepth++;

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;

            if (scanner.token == Globals.Token.oBraceT)
            {
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl(out VarDeclData varDeclData);
                if (rtnVal != 0) return rtnVal;

                foreach (string name in varDeclData.variableNames)
                    stec.variableNames.AddLast(name);

                stec.totalSizeOfLocals = varDeclData.totalSizeOfLocals;

                rtnVal = MethodDecl(out List<string> methodNames);
                if (rtnVal != 0) return rtnVal;
                foreach (string name in methodNames)
                    stec.methodNames.AddLast(name);


                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;

                ASMs.Insert(insertionPoint, "sub sp," + varDeclData.totalSizeOfLocals);
            }
            else
            {
                rtnVal = Match(Globals.Token.extendsT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.oBraceT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = VarDecl(out VarDeclData varDeclData);
                if (rtnVal != 0) return rtnVal;

                foreach (string name in varDeclData.variableNames)
                    stec.variableNames.AddLast(name);

                stec.totalSizeOfLocals = varDeclData.totalSizeOfLocals;

                rtnVal = MethodDecl(out List<string> methodNames);
                if (rtnVal != 0) return rtnVal;
                foreach (string name in methodNames)
                    stec.methodNames.AddLast(name);

                rtnVal = Match(Globals.Token.cBraceT);
                if (rtnVal != 0) return rtnVal;
                ASMs.Insert(insertionPoint, "sub sp," + varDeclData.totalSizeOfLocals);
            }



            EmitASM("");
            EmitASM("add sp," + ((SymTab.GetOffsetAtDepth(currentDepth) * -1) - 2));
            EmitASM("pop bp");
            EmitASM("");

            if (SymTab.DepthHasEntries(currentDepth))
                SymTab.WriteTable(currentDepth);
            SymTab.DeleteDepth(currentDepth);
            currentDepth--;
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

            SymbolTable.SymbolTableEntryClass stec = default;
            if (scanner.token == Globals.Token.idT)
                stec = (SymbolTable.SymbolTableEntryClass) SymTab.Insert(scanner.Lexeme, Globals.Token.classT, currentDepth, SymbolTable.EntryType.tClass);

            if (stec == null)
            {
                rtnVal = 4;
                Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                return rtnVal;
            }
            currentDepth++;
            
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

            SymbolTable.SymbolTableEntryMethod stem = default;
            if (scanner.token == Globals.Token.mainT)
                stem = (SymbolTable.SymbolTableEntryMethod) SymTab.Insert(scanner.Lexeme, Globals.Token.mainT, currentDepth, SymbolTable.EntryType.tMethod);



            if (stem == null)
            {
                rtnVal = 4;
                Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                return rtnVal;
            }
            currentDepth++;

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

            int currentOffset = -2;
            SymbolTable.SymbolTableEntryVariable stev = null;
            if (scanner.token == Globals.Token.idT)
                stev=(SymbolTable.SymbolTableEntryVariable) SymTab.Insert(scanner.Lexeme, Globals.Token.StringT, currentDepth, SymbolTable.EntryType.tVariable);

            if (stev == null)
            {
                rtnVal = 4;
                Printer.ErrLine("Error!  Duplicate entries (" + scanner.Lexeme + ") at depth " + currentDepth + "!");
                return rtnVal;
            }
            stev.offset = 4;
            stev.size = 0;
            stev.variableType = SymbolTable.VariableType.tString;
            
            currentOffset += DetermineSize(Globals.Token.StringT);

            rtnVal = Match(Globals.Token.idT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.cParenT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = Match(Globals.Token.oBraceT);
            if (rtnVal != 0) return rtnVal;
            rtnVal = SeqOfStatements();
            if (rtnVal != 0) return rtnVal;

            EmitASM("");
            EmitASM("Main proc");
            EmitASM("push bp");
            EmitASM("mov bp, s");
            EmitASM("sub sp,0");
            EmitASM("");

            if (scanner.token == Globals.Token.idT)
            {
                string parentClassName = scanner.Lexeme;
                rtnVal = Match(Globals.Token.idT);
                if (rtnVal != 0) return rtnVal;
                rtnVal = Match(Globals.Token.dotT);
                if (rtnVal != 0) return rtnVal;
                //EmitASM("call Test");
                rtnVal = MethodCall(parentClassName);
                if (rtnVal != 0) return rtnVal;
            }

            EmitASM("");
            EmitASM("add sp,0");
            EmitASM("pop bp");
            EmitASM("ret");
            EmitASM("Main endp");
            EmitASM("");

            rtnVal = Match(Globals.Token.cBraceT);
            if (rtnVal != 0) return rtnVal;

            if (SymTab.DepthHasEntries(currentDepth))
                SymTab.WriteTable(currentDepth);
            SymTab.DeleteDepth(currentDepth);
            currentDepth--;

            rtnVal = Match(Globals.Token.cBraceT);
            if (rtnVal != 0) return rtnVal;

            if (SymTab.DepthHasEntries(currentDepth))
                SymTab.WriteTable(currentDepth);
            SymTab.DeleteDepth(currentDepth);
            currentDepth--;

            EmitASM("");
            EmitASM("myMain proc");
            EmitASM("mov ax, @data");
            EmitASM("mov ds, ax");
            EmitASM("");

            EmitASM("");
            EmitASM("call Main");
            EmitASM("");

            EmitASM("mov ax,04ch");
            EmitASM("int 21h");
            EmitASM("myMain endp");
            EmitASM("end myMain");
            EmitASM("");
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
                Printer.ErrLine("Error, was expecting \"" + desired + "\" but found ("+scanner.Lexeme+"):\"" + scanner.token + "\"");
                rtnVal = 1;
            }
            return rtnVal;
        }

        static int DetermineSize(Globals.Token token)
        {
            switch (token)
            {
                case Globals.Token.booleanT:
                    return 1;
                case Globals.Token.intT:
                    return 2;
                default:
                    return 0;
            }
        }

        static void Emit(string ThreeAdressCode)
        {
            TACs.Add(ThreeAdressCode);
        }
        static void Emit(List<string> ThreeAdressCodes)
        {
            TACs.AddRange(ThreeAdressCodes);
        }
        static void EmitASM(string ThreeAdressCode)
        {
            ASMs.Add(ThreeAdressCode);
        }
        static void EmitASM(List<string> ThreeAdressCodes)
        {
            ASMs.AddRange(ThreeAdressCodes);
        }
    }
}
