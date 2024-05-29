using Maui.ViewModels;

namespace Pages
{
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
            BindingContext = new ProductsViewModel();
        }
    }
}
