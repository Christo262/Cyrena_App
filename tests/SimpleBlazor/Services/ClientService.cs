using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleBlazor.Contracts;
using SimpleBlazor.Models;

namespace SimpleBlazor.Services;

internal class ClientService : IClientService
{
    private readonly List<Client> _clients = new();

    public Task AddAsync(Client client)
    {
        _clients.Add(client);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client != null)
        {
            _clients.Remove(client);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Client>> GetAllAsync() => Task.FromResult(_clients.AsEnumerable());

    public Task<Client?> GetByIdAsync(Guid id) => Task.FromResult(_clients.FirstOrDefault(c => c.Id == id));

    public Task UpdateAsync(Client client)
    {
        var existing = _clients.FirstOrDefault(c => c.Id == client.Id);
        if (existing != null)
        {
            existing.Name = client.Name;
            existing.Email = client.Email;
            existing.Phone = client.Phone;
            existing.Address = client.Address;
        }
        return Task.CompletedTask;
    }
}
