using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleBlazor.Contracts;
using SimpleBlazor.Models;

namespace SimpleBlazor.Services;

internal class InvoiceService : IInvoiceService
{
    private readonly List<Invoice> _invoices = new();

    public Task AddAsync(Invoice invoice)
    {
        _invoices.Add(invoice);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var invoice = _invoices.FirstOrDefault(i => i.Id == id);
        if (invoice != null)
        {
            _invoices.Remove(invoice);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Invoice>> GetAllAsync() => Task.FromResult(_invoices.AsEnumerable());

    public Task<Invoice?> GetByIdAsync(Guid id) => Task.FromResult(_invoices.FirstOrDefault(i => i.Id == id));

    public Task UpdateAsync(Invoice invoice)
    {
        var existing = _invoices.FirstOrDefault(i => i.Id == invoice.Id);
        if (existing != null)
        {
            existing.ClientId = invoice.ClientId;
            existing.Date = invoice.Date;
            existing.DueDate = invoice.DueDate;
            existing.Amount = invoice.Amount;
            existing.Description = invoice.Description;
        }
        return Task.CompletedTask;
    }
}
