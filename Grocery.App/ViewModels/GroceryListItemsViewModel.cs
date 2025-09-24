using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        private readonly IFileSaverService _fileSaverService;
        
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        [ObservableProperty]
        ObservableCollection<Product> availableProducts = [];

        private List<Product> _allAvailableProducts = new();

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);
        [ObservableProperty]
        string myMessage = string.Empty;
        [ObservableProperty]
        string searchTerm;

        [ObservableProperty]
        private bool hasSearched;

        public bool IsProductListEmptyAndSearched => hasSearched && AvailableProducts.Count == 0;

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService, IFileSaverService fileSaverService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            _fileSaverService = fileSaverService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            _allAvailableProducts = _productService.GetAll()
                .Where(p => MyGroceryListItems.FirstOrDefault(g => g.ProductId == p.Id) == null && p.Stock > 0)
                .ToList();

            foreach (Product p in _allAvailableProducts)
                AvailableProducts.Add(p);
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null) return;
            GroceryListItem item = new(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(item);
            product.Stock--;
            _productService.Update(product);
            _allAvailableProducts.Remove(product);
            FilterAvailableProducts(SearchTerm);
            OnGroceryListChanged(GroceryList);
        }

        [RelayCommand]
        public async Task ShareGroceryList(CancellationToken cancellationToken)
        {
            if (GroceryList == null || MyGroceryListItems == null) return;
            string jsonString = JsonSerializer.Serialize(MyGroceryListItems);
            try
            {
                await _fileSaverService.SaveFileAsync("Boodschappen.json", jsonString, cancellationToken);
                await Toast.Make("Boodschappenlijst is opgeslagen.").Show(cancellationToken);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Opslaan mislukt: {ex.Message}").Show(cancellationToken);
            }
        }

        private void FilterAvailableProducts(string searchTerm)
        {
            AvailableProducts.Clear();
            var filtered = string.IsNullOrWhiteSpace(searchTerm)
                ? _allAvailableProducts
                : _allAvailableProducts.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var p in filtered)
                AvailableProducts.Add(p);

            HasSearched = !string.IsNullOrWhiteSpace(searchTerm);
            OnPropertyChanged(nameof(IsProductListEmptyAndSearched));
        }

        [RelayCommand]
        void Search(string searchTerm)
        {
            FilterAvailableProducts(searchTerm);
        }

        public bool IsProductListEmpty => AvailableProducts.Count == 0;
    }
}
