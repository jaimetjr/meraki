using System.Net.Http;

namespace Infrastructure.ConsultorioME
{
    public interface IConsultorioMEService
    {
        Task<string> GetTokenAsync();
        Task<IEnumerable<ConsultorioMEProfessionalDto>> GetProfessionalsAsync();
        Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body = null);
    }
}
