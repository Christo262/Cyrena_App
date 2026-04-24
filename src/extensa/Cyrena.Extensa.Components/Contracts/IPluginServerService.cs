using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cyrena.Extensa.Models;

namespace Cyrena.Extensa.Contracts;

/// <summary>
/// Service for managing plugin distribution servers.
/// </summary>
public interface IPluginServerService
{
    /// <summary>
    /// Gets all registered servers.
    /// </summary>
    Task<IEnumerable<PluginServer>> GetAllServers();

    /// <summary>
    /// Gets all enabled servers ordered by priority.
    /// </summary>
    Task<IEnumerable<PluginServer>> GetEnabledServers();

    /// <summary>
    /// Gets a server by its ID.
    /// </summary>
    Task<PluginServer?> GetServer(string serverId);

    /// <summary>
    /// Adds a new server.
    /// </summary>
    Task<PluginServer> AddServerAsync(PluginServer server, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing server.
    /// </summary>
    Task<PluginServer?> UpdateServerAsync(PluginServer server, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a server by its ID.
    /// </summary>
    Task<bool> RemoveServerAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a server.
    /// </summary>
    Task<bool> SetServerEnabledAsync(string serverId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raised when servers are added, updated, removed, or enabled/disabled.
    /// </summary>
    event EventHandler<IReadOnlyList<PluginServer>>? ServersChanged;
}
