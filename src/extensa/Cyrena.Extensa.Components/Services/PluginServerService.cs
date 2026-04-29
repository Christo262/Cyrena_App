using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cyrena.Extensa.Services;

/// <summary>
/// Service for managing plugin distribution servers.
/// </summary>
internal class PluginServerService : IPluginServerService
{
    private readonly PluginServerOptions _options;
    private readonly IStore<PluginServer> _store;
    private readonly ILogger<PluginServerService> _logger;

    public PluginServerService(
        IOptions<PluginServerOptions> options,
        IStore<PluginServer> store,
        ILogger<PluginServerService> logger)
    {
        _options = options.Value;
        _store = store;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<IReadOnlyList<PluginServer>>? ServersChanged;

    public async Task<IEnumerable<PluginServer>> GetAllServers()
    {
        var ext = await _store.FindManyAsync(x => true);
        var model = new List<PluginServer>();
        model.AddRange(ext);
        model.AddRange(_options.DefaultServers.Select(x => new PluginServer()
        {
            ApplicationId = x.ApplicationId,
            BaseUrl = x.BaseUrl,
            IsEnabled = true,
            Name = x.Name,
            Id = x.Id,
            Priority = x.Priority,
            IsDefault = true,
        }));
        return model;
    }

    public async Task<IEnumerable<PluginServer>> GetEnabledServers()
    {
        var ext = await _store.FindManyAsync(x => x.IsEnabled);
        var model = new List<PluginServer>();
        model.AddRange(ext);
        model.AddRange(_options.DefaultServers.Select(x => new PluginServer()
        {
            ApplicationId = x.ApplicationId,
            BaseUrl = x.BaseUrl,
            IsEnabled = true,
            Name = x.Name,
            Id = x.Id,
            Priority = x.Priority,
        }));
        return model;
    }

    public async Task<PluginServer?> GetServer(string serverId)
    {
        var d = _options.DefaultServers.FirstOrDefault(x => x.Id == serverId);
        if (d == null)
            return await _store.FindAsync(x => x.Id == serverId);
        else
            return new PluginServer()
            {
                ApplicationId = d.ApplicationId,
                BaseUrl = d.BaseUrl,
                IsEnabled = true,
                Name = d.Name,
                Id= d.Id,
                Priority= d.Priority,
            };
    }

    public async Task<PluginServer> AddServerAsync(PluginServer server, CancellationToken cancellationToken = default)
    {
        server.Id = Guid.NewGuid().ToString();
        await _store.AddAsync(server, cancellationToken);
        _logger.LogInformation("Added plugin server {ServerId}: {ServerName}", server.Id, server.Name);
        await RaiseServersChangedAsync();
        return server;
    }

    public async Task<PluginServer?> UpdateServerAsync(PluginServer server, CancellationToken cancellationToken = default)
    {
        var ext = await _store.FindAsync(x => x.Id == server.Id);
        if(ext == null)
            return null;
        await _store.UpdateAsync(server, cancellationToken);
        _logger.LogInformation("Updated plugin server {ServerId}: {ServerName}", server.Id, server.Name);
        await RaiseServersChangedAsync();
        return server;
    }

    public async Task<bool> RemoveServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        var ext = await _store.FindAsync(x => x.Id == serverId);
        if( ext == null) return false;
        await _store.DeleteAsync(ext);
        _logger.LogInformation("Removed plugin server {ServerId}", serverId);
        await RaiseServersChangedAsync();
        return true;
    }

    public async Task<bool> SetServerEnabledAsync(string serverId, bool enabled, CancellationToken cancellationToken = default)
    {
        var ext = await _store.FindAsync(x => x.Id == serverId);
        if (ext == null)
            return false;
        ext.IsEnabled = enabled;
        await _store.UpdateAsync(ext, cancellationToken);
        _logger.LogInformation("Plugin server {ServerId} set to {Status}", serverId, enabled ? "enabled" : "disabled");
        await RaiseServersChangedAsync();
        return true;
    }

    private async Task RaiseServersChangedAsync()
    {
        var servers = await GetAllServers();
        ServersChanged?.Invoke(this, servers.ToList());
    }
}
