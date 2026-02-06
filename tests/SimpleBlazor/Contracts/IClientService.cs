using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleBlazor.Models;

namespace SimpleBlazor.Contracts;

public interface IClientService
{
    Task AddAsync(Client client);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(Guid id);
    Task UpdateAsync(Client client);
}
