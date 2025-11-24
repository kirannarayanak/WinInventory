using WinInventory.Models;
using System.Collections.Concurrent;

namespace WinInventory.Services;

public class UserDataService
{
    // In-memory storage (for production, use a database)
    private readonly ConcurrentDictionary<string, UserMachineData> _userData = new();

    public void SaveUserMachineData(string userId, string email, string name, string provider, MachineInfo machineInfo, List<string> applications)
    {
        var data = new UserMachineData
        {
            UserId = userId,
            UserEmail = email,
            UserName = name,
            Provider = provider,
            SignInTime = DateTime.UtcNow,
            MachineInfo = machineInfo,
            InstalledApplications = applications
        };

        _userData.AddOrUpdate(userId, data, (key, oldValue) => data);
    }

    public UserMachineData? GetUserMachineData(string userId)
    {
        return _userData.TryGetValue(userId, out var data) ? data : null;
    }

    public List<UserMachineData> GetAllUserData()
    {
        return _userData.Values.ToList();
    }

    public void DeleteUserData(string userId)
    {
        _userData.TryRemove(userId, out _);
    }
}

