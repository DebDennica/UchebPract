using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UchebPract2022;

namespace UchebPract2022
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Entities DB = new Entities();
        private List<Products> _allProducts;
        private Products _selectedProduct;

        public MainWindow()
        {
            InitializeComponent();
            UserNameText.Visibility = Visibility.Hidden;
            AdminPanel.Visibility = Visibility.Collapsed;
            LoadProducts();
        }

        public MainWindow(Users user)
        {
            InitializeComponent();
            UserNameText.Visibility = Visibility.Visible;
            AutorizationButton.Visibility = Visibility.Hidden;
            LogOutButton.Visibility = Visibility.Visible;
            this.DataContext = new { Users = user };

            if (user.RoleID == 3) // Если администратор
            {
                AdminPanel.Visibility = Visibility.Visible; // Показываем панель администратора
            }
            else
            {
                AdminPanel.Visibility = Visibility.Collapsed; // Скрываем панель администратора
            }

            LoadProducts();
        }

        private void AutorizationButton_Click(object sender, RoutedEventArgs e)
        {
            new Autorization().Show();
            Close();
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            UserNameText.Visibility = Visibility.Hidden;
            AutorizationButton.Visibility = Visibility.Visible;
            LogOutButton.Visibility = Visibility.Hidden;
            this.DataContext = null;
        }

        private void LoadProducts()
        {
            _allProducts = DB.Products.Include("ProductTypes").ToList();

            foreach (var product in _allProducts)
            {
                if (string.IsNullOrEmpty(product.Image))
                {
                    product.Image = "/images/picture.png";
                }
            }

            ProductList.ItemsSource = _allProducts;

            if (TypeFilter != null)
            {
                var types = DB.ProductTypes.Select(p => p.ProductType).Distinct().ToList();
                types.Insert(0, "Все типы");
                TypeFilter.ItemsSource = types;
                TypeFilter.SelectedIndex = 0;
            }
        }

        private void FilterProducts(object sender, EventArgs e)
        {
            if (_allProducts == null) return;

            string searchText = SearchBox.Text.ToLower().Trim();
            string selectedType = TypeFilter?.SelectedItem as string;

            string sortOption = "По умолчанию";
            if (SortFilter?.SelectedItem is ComboBoxItem selectedItem)
            {
                sortOption = selectedItem.Content.ToString();
            }

            var filtered = _allProducts.ToList();

            //  Фильтр типа товара
            if (!string.IsNullOrEmpty(selectedType) && selectedType != "Все типы")
            {
                filtered = filtered.Where(p => p.ProductTypes.ProductType == selectedType).ToList();
            }

            // Фильтр поиска
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(p =>
                    (p.ProductName != null && p.ProductName.ToLower().Contains(searchText)) ||
                    (p.Composition != null && p.Composition.ToLower().Contains(searchText)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchText))
                ).ToList();
            }

            // Сортировка по цене
            switch (sortOption)
            {
                case "По возрастанию":
                    filtered = filtered.OrderBy(p => p.Price).ToList();
                    break;
                case "По убыванию":
                    filtered = filtered.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    break;
            }

            ProductList.ItemsSource = filtered;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            new ProductEditWindow().ShowDialog();
            RefreshProducts();
        }

        private void ProductCard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentSession.CurrentUser == null || CurrentSession.CurrentUser.RoleID != 3)
            {
                MessageBox.Show("Только администратор может редактировать товары.");
                return;
            }

            if (e.ClickCount == 2) // проверка что нажалось два раза
            {
                var border = sender as Border;
                var product = border.DataContext as Products;
                if (product != null)
                {
                    new ProductEditWindow(product).ShowDialog();
                    RefreshProducts();
                }
            }
        }
        private void RefreshProducts()
        {
            _allProducts = DB.Products.Include("ProductTypes").ToList();
            ProductList.ItemsSource = _allProducts;
        }
    }
}
