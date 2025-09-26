using Grocery.App.ViewModels;

namespace Grocery.App.Views;

public partial class RegisterView : ContentPage
{
    public RegisterView()
    {
        InitializeComponent();
        BindingContext = new RegisterViewModel();
    }
}