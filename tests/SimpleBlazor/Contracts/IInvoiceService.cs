using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleBlazor.Models;

namespace SimpleBlazor.Contracts;

public interface IInvoiceService
{
    Task AddAsync(Invoice invoice);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
    Task UpdateAsync(Invoice invoice);
}
