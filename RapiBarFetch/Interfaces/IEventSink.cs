// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace RapiBarFetch;

public interface IEventSink
{
    void BadJobIgnored(Job job);
    void SaveBarSet(BarSet barSet);
    void LoginFailed();
    void LoginSucceeded();
}