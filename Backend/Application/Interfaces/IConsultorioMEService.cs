using Application.DTOs;
using System.Net.Http;

namespace Application.Interfaces
{
    public interface IConsultorioMEService
    {
        Task<string> GetTokenAsync();
        Task<IEnumerable<ConsultorioMEProfessionalDto>> GetProfessionalsAsync();
        Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body = null);
    }
}
