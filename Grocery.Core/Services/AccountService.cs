using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Collections.Generic;

public static class AccountService
{
    private const string AccountsKey = "accounts";

    public static void SaveAccount(string username, string email, string password)
    {
        var accounts = GetAllAccounts();
        accounts[email] = password;
        var json = JsonSerializer.Serialize(accounts);
        Preferences.Set(AccountsKey, json);
    }

    public static bool ValidateAccount(string email, string password)
    {
        var accounts = GetAllAccounts();
        return accounts.TryGetValue(email, out var storedPassword) && storedPassword == password;
    }

    public static bool AccountExists(string email)
    {
        var accounts = GetAllAccounts();
        return accounts.ContainsKey(email);
    }

    private static Dictionary<string, string> GetAllAccounts()
    {
        var json = Preferences.Get(AccountsKey, "");
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, string>();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }
}