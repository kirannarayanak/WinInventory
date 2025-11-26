using WinInventory.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace WinInventory.Services;

public class UserDataService
{
    // In-memory storage
    private readonly ConcurrentDictionary<string, UserMachineData> _userData = new();
    private readonly string _dataFilePath;
    private readonly object _fileLock = new object();

    public UserDataService()
    {
        // Store data file in wwwroot/data directory
        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
        Directory.CreateDirectory(dataDir);
        _dataFilePath = Path.Combine(dataDir, "user-machine-data.json");
        
        // Load existing data on startup
        LoadDataFromFile();
    }

    private void LoadDataFromFile()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dataList = JsonSerializer.Deserialize<List<UserMachineData>>(json);
                    if (dataList != null)
                    {
                        foreach (var data in dataList)
                        {
                            _userData.TryAdd(data.UserId, data);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash - start with empty data
            Console.WriteLine($"Error loading user data: {ex.Message}");
        }
    }

    private void SaveDataToFile()
    {
        try
        {
            lock (_fileLock)
            {
                var dataList = _userData.Values.ToList();
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(dataList, options);
                File.WriteAllText(_dataFilePath, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user data: {ex.Message}");
        }
    }

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
        
        // Persist to file
        SaveDataToFile();
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
        SaveDataToFile();
    }
}

