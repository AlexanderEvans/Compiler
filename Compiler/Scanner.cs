using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    //performs a lexical analysis of a string
    class Scanner
    {
        Dictionary<string, Globals.Token> lookupTable = new Dictionary<string, Globals.Token>();

        //initialize lookup table for scanner
        public Scanner()
        {
            lookupTable.Add("class", Globals.Token.classT);
            lookupTable.Add("public", Globals.Token.publicT);
            lookupTable.Add("static", Globals.Token.staticT);
            lookupTable.Add("void", Globals.Token.voidT);
            lookupTable.Add("main", Globals.Token.mainT);
            lookupTable.Add("String", Globals.Token.StringT);
            lookupTable.Add("extends", Globals.Token.extendsT);
            lookupTable.Add("return", Globals.Token.returnT);
            lookupTable.Add("int", Globals.Token.intT);
            lookupTable.Add("boolean", Globals.Token.booleanT);
            lookupTable.Add("if", Globals.Token.ifT);
            lookupTable.Add("else", Globals.Token.elseT);
            lookupTable.Add("while", Globals.Token.whileT);
            lookupTable.Add("System.out.println", Globals.Token.printlnT);
            lookupTable.Add("length", Globals.Token.lengthT);
            lookupTable.Add("true", Globals.Token.trueT);
            lookupTable.Add("false", Globals.Token.falseT);
            lookupTable.Add("this", Globals.Token.thisT);
            lookupTable.Add("new", Globals.Token.newT);
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

        //clears the scanner variables
        void clrVal()
        {
            Lexeme = "";
            Value = null;
            ValueR = null;
            hasLiteral = false;
            Literal = "";
        }

        StringBuilder sb = new StringBuilder();

        //try to scan the next token and update the scanner state.
        public void GetNextToken()
        {
            //handle eof
            if(inputIndex>=input.Length)
            {
                clrVal();
                token = Globals.Token.eofT;
                return;
            }

            switch (input[inputIndex])
            {
                //handle simple symbol lexems
                case '(':
                    clrVal();
                    token = Globals.Token.oParenT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case ')':
                    clrVal();
                    token = Globals.Token.cParenT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '{':
                    clrVal();
                    token = Globals.Token.oBraceT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '}':
                    clrVal();
                    token = Globals.Token.cBraceT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '[':
                    clrVal();
                    token = Globals.Token.oBrackT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case ']':
                    clrVal();
                    token = Globals.Token.cBrackT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case ';':
                    clrVal();
                    token = Globals.Token.semicolonT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '.':
                    clrVal();
                    token = Globals.Token.dotT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case ',':
                    clrVal();
                    token = Globals.Token.commaT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '+':
                    clrVal();
                    token = Globals.Token.addOpT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '-':
                    clrVal();
                    token = Globals.Token.addOpT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                case '*':
                    clrVal();
                    token = Globals.Token.mulOpT;
                    Lexeme = "" + input[inputIndex];
                    inputIndex++;
                    break;
                //handle compound 2 char lexemes
                case '=':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '=')
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.assignOpT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '>':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '=')
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '<':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '=')
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '!':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '=')
                    {
                        token = Globals.Token.relOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.unknownT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '|':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '|')
                    {
                        token = Globals.Token.addOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.unknownT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '&':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '&')
                    {
                        token = Globals.Token.mulOpT;
                        Lexeme = "" + input[inputIndex] + input[inputIndex + 1];
                        inputIndex += 2;
                    }
                    else
                    {
                        token = Globals.Token.unknownT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                //handle whitespace
                case ' ':
                    inputIndex++;
                    GetNextToken();
                    break;
                case '\n':
                    inputIndex++;
                    GetNextToken();
                    break;
                case '\t':
                    inputIndex++;
                    GetNextToken();
                    break;
                case '\r':
                    inputIndex++;
                    GetNextToken();
                    break;
                //handle variable length lexemes
                case '/':
                    clrVal();

                    if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '/')
                    {
                        int cIndex = 0;
                        while (inputIndex + cIndex < input.Length && input[inputIndex + cIndex] != '\n')
                        {
                            cIndex++;
                        }

                        inputIndex += cIndex;
                        GetNextToken();
                    }
                    else if (inputIndex + 1 < input.Length && input[inputIndex + 1] == '*')
                    {
                        int cIndex = 2;
                        while (inputIndex + cIndex + 1 < input.Length && !(input[inputIndex + cIndex] == '*' && input[inputIndex + cIndex + 1] == '/'))
                        {
                            cIndex++;
                        }
                        cIndex++;

                        if (inputIndex + cIndex >= input.Length)
                            Printer.WarnLine("Scanner Warning: Unmatched parenthesis, was expecting '*/' but found 'eof'");

                        inputIndex += cIndex;
                        GetNextToken();
                    }
                    else
                    {
                        token = Globals.Token.mulOpT;
                        Lexeme = "" + input[inputIndex];
                        inputIndex++;
                    }
                    break;
                case '"':
                    clrVal();
                    token = Globals.Token.dQuoteT;
                    {
                        sb.Clear();
                        int i = 1;
                        while (inputIndex + i+1 < input.Length && input[inputIndex + i] != '\"' && input[inputIndex + i] != '\n')
                        {
                            if (input[inputIndex + i] == '\\' && input[inputIndex + i] == '\"')
                                i++;
                            i++;
                        }

                        if (inputIndex + i < input.Length && input[inputIndex + i] != '\"')
                            Printer.WarnLine("Scanner Warning: Unmatched parenthesis, was expecting '\"' but found '" + input[inputIndex + i] + "'");
                        else if (inputIndex + i >= input.Length)
                            Printer.WarnLine("Scanner Warning: Unmatched parenthesis, was expecting '\"' but found 'eof'");

                        for (int j = 1; j < i; j++)
                        {
                            sb.Append(input[inputIndex + i]);
                        }

                        Literal = sb.ToString();
                        Lexeme = "\"" + Literal + "\"";
                        inputIndex += i;

                        if (inputIndex + i < input.Length && input[inputIndex + i] == '\"')
                            inputIndex++;
                    }
                    break;
                default:
                    defaultScannerFallback();
                    break;
            }
        }

        //handle numbers, reserved workds, and identifiers
        private void defaultScannerFallback()
        {
            if (!checkDigit())
                if (!checkWord())
                {
                    clrVal();
                    token = Globals.Token.unknownT;
                    inputIndex++;
                }
        }

        //check for reserved words and identifiers
        private bool checkWord()
        {
            clrVal();
            if(char.IsLetter(input[inputIndex]))
            {
                int i = 1;
                while (inputIndex+i+1<input.Length && (char.IsLetterOrDigit(input[inputIndex + i]) || input[inputIndex + i] == '_') && i<31)
                    i++;
                sb.Clear();
                for(int j = 0; j<i; j++)
                {
                    sb.Append(input[inputIndex + j]);
                }
                token = Globals.Token.idT;
                Lexeme = sb.ToString();

                i = 1;
                while (inputIndex + i+1 < input.Length && (char.IsLetterOrDigit(input[inputIndex + i]) || input[inputIndex + i] == '.') && i<256)
                    i++;
                sb.Clear();
                for (int j = 0; j < i; j++)
                {
                    sb.Append(input[inputIndex + j]);
                }

                if (lookupTable.TryGetValue(sb.ToString(), out Globals.Token myTok))
                {
                    clrVal();
                    token = myTok;
                    Lexeme = sb.ToString();
                    inputIndex += Lexeme.Length;
                }
                else
                {
                    inputIndex += Lexeme.Length;
                }
                return true;
            }
            return false;
        }

        //check for integers and floating point numbers
        private bool checkDigit()
        {
            string digits = "0123456789";
            clrVal();
            if(digits.Contains(input[inputIndex]))
            {
                int i = 0;
                int j = 1;
                while (inputIndex+i+1<input.Length && digits.Contains(input[inputIndex+i]))
                {
                    i++;
                }
                token = Globals.Token.numT;
                if(inputIndex + i < input.Length && input[inputIndex + i] == '.')
                {
                    while (inputIndex + i+j+1 < input.Length && digits.Contains(input[inputIndex +i+ j]))
                    {
                        j++;
                    }
                }
                if(j>1)
                {
                    sb.Clear();
                    for(int k = 0; k<i+j; k++)
                    {
                        sb.Append(input[inputIndex + k]);
                    }

                    ValueR = float.Parse(sb.ToString());
                    inputIndex += i + j;
                }
                else
                {
                    sb.Clear();
                    for (int k = 0; k < i; k++)
                    {
                        sb.Append(input[inputIndex + k]);
                    }

                    Value = int.Parse(sb.ToString());
                    inputIndex += i;
                }
                return true;
            }
            else
            {
                token = Globals.Token.unknownT;
                Lexeme = "" + input[inputIndex];
                inputIndex++;
                return false;
            }
        }
    }
}
