// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace RapiBarFetch;

public static class SymbolAs
{
    private static readonly Dictionary<Symbol, string> symbolAs = new();

    static SymbolAs()
    {
        symbolAs.Add(Symbol.BP, "6B");
        symbolAs.Add(Symbol.CL, "CL");
        symbolAs.Add(Symbol.E7, "E7");
        symbolAs.Add(Symbol.ES, "ES");
        symbolAs.Add(Symbol.EU, "6E");
        symbolAs.Add(Symbol.GC, "GC");
        symbolAs.Add(Symbol.J7, "J7");
        symbolAs.Add(Symbol.JY, "6J");
        symbolAs.Add(Symbol.NQ, "NQ");
        symbolAs.Add(Symbol.QM, "QMMY");
        symbolAs.Add(Symbol.QO, "QO");
        symbolAs.Add(Symbol.ZB, "ZB");
        symbolAs.Add(Symbol.ZF, "ZF");
        symbolAs.Add(Symbol.ZN, "ZN");
    }

    public static string GetRithmicSymbol(Symbol symbol) => symbolAs[symbol];
}