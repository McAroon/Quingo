﻿using System.Collections.Concurrent;

namespace Quingo.Shared;

public class TempUserStorage
{
    public const string DarkMode = "darkMode";
    public const string TimeZone = "timeZone";

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _data = [];

    public string? Get(string userId, string key)
    {
        return _data.TryGetValue(userId, out var userData) && userData.TryGetValue(key, out var value) ? value : null;
    }

    public bool TryGet(string userId, string key, out string? value)
    {
        value = Get(userId, key);
        return value != null;
    }

    public void Set(string userId, string key, string value)
    {
        if (!_data.ContainsKey(userId)) 
        {
            _data.TryAdd(userId, []);
        }
        
        _data[userId][key] = value;
    }
}
