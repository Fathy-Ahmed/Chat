using Chat.Application.Interfaces;
using System.Collections.Concurrent;

namespace Chat.Infrastructure.Services;

public class InMemoryUserConnectionTracker : IUserConnectionTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();

    public void AddConnection(string userId, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(userId, out var set))
            {
                set = new HashSet<string>();
                _connections[userId] = set;
            }
            set.Add(connectionId);
        }
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(userId, out var set))
            {
                set.Remove(connectionId);
                if (set.Count == 0)
                    _connections.TryRemove(userId, out _);
            }
        }
    }

    public List<string> GetConnections(string userId)
    {
        return _connections.TryGetValue(userId, out var set) ? set.ToList() : new List<string>();
    }

    public bool IsOnline(string userId) => _connections.ContainsKey(userId);

    public List<string> GetOnlineUserIds() => _connections.Keys.ToList();
}
