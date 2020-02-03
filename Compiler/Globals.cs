using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    class Globals
    {
        public enum Token
        {
            classT, publicT, staticT, voidT, mainT, StringT, extendsT, returnT, intT, booleanT,
            ifT, elseT, whileT, printlnT, lengthT, trueT, falseT, thisT, newT,
            relOpT, addOpT, mulOpT, assignOpT,
            oParenT, cParenT, oBraceT, cBraceT, oBrackT, cBrackT,
            dotT, commaT, dQuoteT, semicolonT,
        }
    }
    struct StructuralKeyValuePair<Key, Value>
    {
        public Key key;
        public Value value;
    }
}
