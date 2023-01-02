using Utils.Core.Code;

namespace Utils.TestApp;

internal class Program
{
    static void Main(string[] args)
    {
        var txt = UtilitiesWebRequestShared.ServiceRequestJson("https://google.com");
    }
}