using System;
using System.Threading.Tasks;

namespace vrhero.VirtualHere
{
    interface IClient : IDisposable
    {
        Task<string> QueryAsync(string command);
    }
}
