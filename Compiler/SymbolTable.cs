using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Compiler
{
    class SymbolTable
    {

        List<Dictionary<string, SymbolTableEntry>> symbolTable = new List<Dictionary<string, SymbolTableEntry>>();
        public enum VariableType { tInt, tString, tBool, tChar };
        public enum EntryType { tVariable, tConstant, tMethod, tClass, tDefault };
        public enum VariablePasssingMode { };

        //interface
        public SymbolTableEntry Insert(string lex, Globals.Token token, int depth, EntryType entryType = EntryType.tDefault)
        {
            //lazy expand table
            while(symbolTable.Count<=depth)
            {
                symbolTable.Add(null);
            }

            //populate entry
            SymbolTableEntry newEntry;
            switch (entryType)
            {
                case EntryType.tClass:
                    newEntry = new SymbolTableEntryClass(); break;
                case EntryType.tConstant:
                    newEntry = new SymbolTableEntryConstant(); break;
                case EntryType.tMethod:
                    newEntry = new SymbolTableEntryMethod(); break;
                case EntryType.tVariable:
                    newEntry = new SymbolTableEntryVariable(); break;
               default:
                    newEntry = new SymbolTableEntry(); break;
            }
            newEntry.lex = lex;
            newEntry.token = token;

            //create bucket
            if (symbolTable[depth] == null)
                symbolTable[depth] = new Dictionary<string, SymbolTableEntry>();

            //insert
            if (symbolTable[depth].TryAdd(lex, newEntry))
                return newEntry;
            else
                return null;
        }
        public int GetOffsetAtDepth(int depth)
        {
            int currentOffset = 0;
            if (!(depth < symbolTable.Count))
                return -2;
            if (symbolTable[depth] != null)
            {
                foreach(KeyValuePair<string, SymbolTableEntry> element in symbolTable[depth])
                {
                    if(element.Value is SymbolTableEntryVariable)
                    {
                        if (((SymbolTableEntryVariable)element.Value).offset < currentOffset)
                            currentOffset = ((SymbolTableEntryVariable)element.Value).offset;
                    }
                    if (element.Value is SymbolTableEntryConstant)
                    {
                        if (((SymbolTableEntryConstant)element.Value).offset < currentOffset)
                            currentOffset = ((SymbolTableEntryConstant)element.Value).offset;
                    }
                }
            }
            return currentOffset;
        }
        public Globals.Token GetTypeAtOffset(int depth, int offset)
        {
            if (!(depth < symbolTable.Count))
                return Globals.Token.StringT;
            if (symbolTable[depth] != null)
            {
                foreach(KeyValuePair<string, SymbolTableEntry> element in symbolTable[depth])
                {
                    if(element.Value is SymbolTableEntryVariable)
                    {
                        if (((SymbolTableEntryVariable)element.Value).offset == offset)
                            return ((SymbolTableEntryVariable)element.Value).token;
                    }
                    if (element.Value is SymbolTableEntryConstant)
                    {
                        if (((SymbolTableEntryConstant)element.Value).offset == offset)
                            return ((SymbolTableEntryConstant)element.Value).token;
                    }
                }
            }
            return Globals.Token.unknownT;
        }

        public (SymbolTableEntry, int depth) LookUp(string lex)
        {
            for(int depth = symbolTable.Count-1; depth>=0; depth--)
            {
                if (symbolTable[depth] != null)
                {
                    if (symbolTable[depth].TryGetValue(lex, out SymbolTableEntry rtnVal))
                        return (rtnVal, depth);
                }
            }
            return (null, 0);
        }
        //added due to the lack of union support in c#.  Use this instead to plymorphicly mutate entries' type with a reallocation and let the old container get released to GC
        public SymbolTableEntry TryMutateType(string lex, EntryType type)
        {
            for (int depth = symbolTable.Count - 1; depth >= 0; depth--)
            {
                if (symbolTable[depth] != null)
                {
                    if (symbolTable[depth].TryGetValue(lex, out SymbolTableEntry old))
                    {
                        SymbolTableEntry newEntry = null;
                        switch (type)
                        {
                            case EntryType.tClass:
                                newEntry = new SymbolTableEntryClass();
                                break;
                            case EntryType.tMethod:
                                newEntry = new SymbolTableEntryMethod();
                                break;
                            case EntryType.tVariable:
                                newEntry = new SymbolTableEntryVariable();
                                break;
                            case EntryType.tConstant:
                                newEntry = new SymbolTableEntryConstant();
                                break;
                        }
                        newEntry.lex = old.lex;
                        newEntry.token = old.token;
                        symbolTable[depth][lex] = newEntry;
                        return newEntry;
                    }
                }
            }
            return null;
        }

        public void DeleteDepth(int depth)
        {
            if (symbolTable.Count > depth)
                if (symbolTable[depth] != null)
                    symbolTable[depth].Clear();
        }

        public bool DepthHasEntries(int depth)
        {
            if (symbolTable.Count > depth)
                if (symbolTable[depth] != null)
                {
                    if (symbolTable[depth].Count > 0)
                        return true;
                }
            return false;
        }

        public void WriteTable(int depth)
        {
            //header
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Printer.WriteLine("EntType\tToken \tLexeme \tDepth("+depth+")");
            Console.ForegroundColor = tmp;

            //loop
            if (symbolTable.Count > depth)
                if (symbolTable[depth] != null)
                {
                    foreach(KeyValuePair<string, SymbolTableEntry> kvp in symbolTable[depth])
                    {
                        kvp.Value.Print();
                    }
                }
        }
        //end interface

        public class SymbolTableEntry
        {
            public string lex="";
            public Globals.Token token=default;

            public virtual void Print()
            {
                string sType = "";

                if (this is SymbolTableEntryClass)
                    sType = "Class";
                else if (this is SymbolTableEntryVariable)
                    sType = "Varible";
                else if (this is SymbolTableEntryConstant)
                    sType = "Constant";
                else if (this is SymbolTableEntryMethod)
                    sType = "Method";
                else
                    sType = "Unkown";

                    Printer.WriteLine(sType + "\t" + token + "\t" + lex);
            }
        }

        public class SymbolTableEntryVariable : SymbolTableEntry
        {
            public VariableType variableType;
            public int size;
            public int offset=0;

            public override void Print()
            {
                Printer.WriteLine("Varible" + "\t" + token + "\t" + lex + "\t" + "offset("+offset+ ")" + "\t" + "size(" + size + ")" + "\t" + "VariableType(" + variableType + ")");
            }
        }

        public class SymbolTableEntryConstant : SymbolTableEntry
        {
            public float real;
            public int integer;
            public int size;
            public int offset=0;
            public VariableType variableType;
            public override void Print()
            {
                Printer.WriteLine("Varible" + "\t" + token + "\t" + lex + "\t" + "real(" + real + ")" + "\t" + "integer(" + integer + ")");
            }
        }

        public class SymbolTableEntryMethod : SymbolTableEntry
        {
            public int totalSizeOfLocals;
            public int paramCount;
            public List<VariableType> variableTypes = new List<VariableType>();
            public List<VariablePasssingMode> variablePasssingModes = new List<VariablePasssingMode>();
        }
        public class SymbolTableEntryClass : SymbolTableEntry
        {
            public int totalSizeOfLocals;
            public LinkedList<string> methodNames = new LinkedList<string>();
            public LinkedList<string> variableNames = new LinkedList<string>();
        } 

        [StructLayout(LayoutKind.Explicit)]
        public struct SymbolTableEntryUnionStyle
        {
            [FieldOffset(0)]
            public string lex;
            [FieldOffset(4)]
            public Globals.Token token;
        }
    }
}
