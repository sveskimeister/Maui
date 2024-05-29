using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Maui.Models;
using SQLite;
using Microsoft.Maui.Controls;

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
            set
            {
                _operatingProduct = value;
                OnPropertyChanged();
            }
        }
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        public string BusyText { get; set; }
        public ICommand SetOperatingProductCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ProductsViewModel()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Products.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Product>().Wait();

            Products = new ObservableCollection<Product>();
            OperatingProduct = new Product();
            IsBusy = false;
            BusyText = "Loading products...";
            SetOperatingProductCommand = new Command<Product>(SetOperatingProduct);
            SaveProductCommand = new Command(async () => await SaveProduct());
            DeleteProductCommand = new Command<int>(async (id) => await DeleteProduct(id));

            LoadProducts();
        }

        private void SetOperatingProduct(Product product)
        {
            OperatingProduct = product ?? new Product();
        }

        private async Task SaveProduct()
        {
            IsBusy = true;
            OnPropertyChanged(nameof(IsBusy));

            if (OperatingProduct.Id != 0)
            {
                await _database.UpdateAsync(OperatingProduct);
            }
            else
            {
                await _database.InsertAsync(OperatingProduct);
            }

            OperatingProduct = new Product();
            OnPropertyChanged(nameof(OperatingProduct));
            await LoadProducts();

            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
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
            OnPropertyChanged(nameof(IsBusy));

            var products = await _database.Table<Product>().ToListAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
