namespace Chat.Application.Interfaces;

public interface IUserConnectionTracker
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    List<string> GetConnections(string userId);
    bool IsOnline(string userId);
    List<string> GetOnlineUserIds();
}
