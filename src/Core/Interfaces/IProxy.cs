using Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IProxy
    {
        Task<List<ProxyInfo>> GetProxies();
    }
}
