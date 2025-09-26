using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;


public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty] string username;
    [ObservableProperty] string password;
    [ObservableProperty] string message;

    [RelayCommand]
    private async Task Register()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Message = "Vul alle velden in.";
            return;
        }

        if (AccountService.AccountExists(Username))
        {
            Message = "Gebruikersnaam bestaat al.";
            return;
        }

        AccountService.SaveAccount(Username, Password);
        Message = "Account aangemaakt! Je kunt nu inloggen.";
        await Task.Delay(1000);
        await Application.Current.MainPage.Navigation.PopAsync();
    }
}