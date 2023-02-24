// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace RapiBarFetch;

public static class Safe
{
    public static bool TryGetValue<T>(Func<T> func, out T value)
    {
        try
        {
            value = func();

            return true;
        }
        catch
        {
            value = default!;

            return false;
        }
    }
}