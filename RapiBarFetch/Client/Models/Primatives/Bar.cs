// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace RapiBarFetch;

public class Bar
{
    public required DateTime CloseOn { get; init; }
    public required double Open { get; init; }
    public required double High { get; init; }
    public required double Low { get; init; }
    public required double Close { get; init; }

    public override string ToString() => ToCsvString();

    public string ToCsvString() =>
        $"{CloseOn:MM/dd/yyyy HH:mm:ss.fff},{Open},{High},{Low},{Close}";

    public string ToNinjaString() =>
        $"{CloseOn:yyyyMMdd HHmmss fffffff};{Open};{High};{Low};{Close};0";
}