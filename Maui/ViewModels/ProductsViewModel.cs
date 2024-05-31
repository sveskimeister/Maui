using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Maui.Models;
using SQLite;

namespace Maui.ViewModels
{
    public class ProductsViewModel : INotifyPropertyChanged
    {
        private Product _operatingProduct;
        private bool _isBusy;
        private readonly SQLiteAsyncConnection _database;

        public ObservableCollection<Product> Products { get; set; }
        public Product OperatingProduct
        {
            get => _operatingProduct;
            set => SetProperty(ref _operatingProduct, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value, nameof(IsBusy));
        }

        public string BusyText { get; set; }

        public ICommand SetOperatingProductCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProductsViewModel()
        {
            _database = InitializeDatabase();

            Products = new ObservableCollection<Product>();
            OperatingProduct = new Product();
            BusyText = "Loading products...";

            SetOperatingProductCommand = new Command<Product>(SetOperatingProduct);
            SaveProductCommand = new Command(async () => await SaveProduct());
            DeleteProductCommand = new Command<int>(async (id) => await DeleteProduct(id));

            LoadProducts();
        }

        private SQLiteAsyncConnection InitializeDatabase()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Products.db3");
            var database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<Product>().Wait();
            return database;
        }

        private void SetOperatingProduct(Product product)
        {
            OperatingProduct = product ?? new Product();
        }

        private async Task SaveProduct()
        {
            IsBusy = true;

            if (OperatingProduct.Id != 0)
            {
                await _database.UpdateAsync(OperatingProduct);
            }
            else
            {
                await _database.InsertAsync(OperatingProduct);
            }

            OperatingProduct = new Product();
            await LoadProducts();

            IsBusy = false;
        }

        private async Task DeleteProduct(int productId)
        {
            var product = await _database.Table<Product>().Where(p => p.Id == productId).FirstOrDefaultAsync();
            if (product != null)
            {
                await _database.DeleteAsync(product);
                await LoadProducts();
            }
        }

        private async Task LoadProducts()
        {
            IsBusy = true;

            var products = await _database.Table<Product>().ToListAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            IsBusy = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
