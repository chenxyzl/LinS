using System;

namespace Base.Common
{
    public interface IPlugin : IDisposable
    {
        string GetMessage();
    }
}
