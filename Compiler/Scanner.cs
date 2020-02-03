using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    class Scanner
    {
        List<StructuralKeyValuePair<Globals.Token, string[]>> lookupTable = new List<StructuralKeyValuePair<Globals.Token, string[]>>();

        public Scanner()
        {
            StructuralKeyValuePair<Globals.Token, string[]> kvp;

            kvp.key = Globals.Token.relOpT;
            kvp.value = new string[] { "<", ">", ">=", "<=", "==", "!=" };
            lookupTable.Add(kvp);

            kvp.key = Globals.Token.addOpT;
            kvp.value = new string[] { "+", "-", "||" };
            lookupTable.Add(kvp);

            kvp.key = Globals.Token.mulOpT;
            kvp.value = new string[] { "*", "/", "&&" };
            lookupTable.Add(kvp);
        }

        public Globals.Token token;
        public string Lexeme;
        public int? Value;
        public double? ValueR;
        public bool hasLiteral = false;
        public string Literal;

        string input = "";
        int inputIndex = 0;

        public void LoadInput(string input) => this.input = input;

        public void GetNextToken()
        {

        }
    }
}
