using Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ISelenium
    {
        Task<LoginResponse> Login(string user, string password, string challengeId, Dictionary<string, string> cookies, bool twiceLogin, ProxyInfo proxyInfo);
    }
}
