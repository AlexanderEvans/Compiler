using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    class SymbolTable
    {

        List<Dictionary<string, SymbolTableEntry>> symbolTable = new List<Dictionary<string, SymbolTableEntry>>();
        public enum VariableType { tInt, tString, tBool, tChar };
        public enum EntryType { tVariable, tConstant, tMethod, tClass };
        public enum VariablePasssingMode { };

        //interface
        public SymbolTableEntry Insert(string lex, Globals.Token token, int depth)
        {
            //lazy expand table
            while(symbolTable.Count<=depth)
            {
                symbolTable.Add(null);
            }

            //populate entry
            SymbolTableEntry newEntry = new SymbolTableEntry();
            newEntry.lex = lex;
            newEntry.token = token;

            //create bucket
            if (symbolTable[depth] == null)
                symbolTable[depth] = new Dictionary<string, SymbolTableEntry>();

            //insert
            symbolTable[depth].Add(lex, newEntry);

            return newEntry;
        }

        public SymbolTableEntry LookUp(string lex)
        {
            for(int depth = symbolTable.Count-1; depth>=0; depth--)
            {
                if (symbolTable[depth] != null)
                {
                    if (symbolTable[depth].TryGetValue(lex, out SymbolTableEntry rtnVal))
                        return rtnVal;
                }
            }
            return null;
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

        public void WriteTable(int depth)
        {
            //header
            Printer.WriteLine("Token \tLexeme \tvalue");

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
                Printer.WriteLine(token + "\t" + lex);
            }
        }

        public class SymbolTableEntryVariable : SymbolTableEntry
        {
            public VariableType variableType;
            public int size;
            public int offset;
        }

        public class SymbolTableEntryConstant : SymbolTableEntry
        {
            public float real;
            public int integer;
        }

        public class SymbolTableEntryMethod : SymbolTableEntry
        {
            int totalSizeOfLocals;
            int paramCount;
            List<VariableType> variableTypes = new List<VariableType>();
            List<VariablePasssingMode> variablePasssingModes = new List<VariablePasssingMode>();
        }
        public class SymbolTableEntryClass : SymbolTableEntry
        {
            int totalSizeOfLocals;
            LinkedList<string> methodNames = new LinkedList<string>();
            LinkedList<string> variableNames = new LinkedList<string>();
        } 

    }
}
