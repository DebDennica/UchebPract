using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace UchebPract2022
{
    /// <summary>
    /// Interaction logic for ProductEditWindow.xaml
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        private Entities DB = new Entities();
        private Products _currentProduct;
        private string _selectedImagePath = null;

        private static int _openWindowsCount = 0;

        public ProductEditWindow(Products product = null)
        {
            InitializeComponent();

            // Загрузка типов товаров
            TypeBox.ItemsSource = DB.ProductTypes.ToList();

            if (product == null) // Добавление
            {
                Title = "Добавление продукции";
                _currentProduct = new Products();
                DeleteButton.Visibility = Visibility.Collapsed;
            }
            else // Редактирование
            {
                Title = "Редактирование продукции";
                _currentProduct = product;
                LoadProductData();
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Проверка после инициализации
            if (_openWindowsCount >= 2)
            {
                Close();
                return;
            }
            _openWindowsCount++;
        }

        private void LoadProductData()
        {
            NameBox.Text = _currentProduct.ProductName;
            DescriptionBox.Text = _currentProduct.Description;
            CompositionBox.Text = _currentProduct.Composition;
            PriceBox.Text = _currentProduct.Price?.ToString() ?? "";
            DiscountBox.Text = _currentProduct.DiscountProcent?.ToString() ?? "";
            TypeBox.SelectedValue = _currentProduct.ProductTypeID;
            ImageNameText.Text = System.IO.Path.GetFileName(_currentProduct.Image);
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e) //выбор картинки
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (ofd.ShowDialog() == true)
            {
                _selectedImagePath = ofd.FileName;
                ImageNameText.Text = System.IO.Path.GetFileName(_selectedImagePath);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                MessageBox.Show("Заполните название и цену!");
                return;
            }

            // Сохранение данных
            _currentProduct.ProductName = NameBox.Text;
            _currentProduct.Composition = CompositionBox.Text;
            _currentProduct.Price = double.TryParse(PriceBox.Text, out double price) ? price : 0;
            _currentProduct.DiscountProcent = double.TryParse(DiscountBox.Text, out double disc) ? disc : 0;
            _currentProduct.ProductTypeID = TypeBox.SelectedValue as int?;
            _currentProduct.Description = DescriptionBox.Text;

            // Сохранение картинки
            if (_selectedImagePath != null)
            {
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(_selectedImagePath);
                string destPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", fileName);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destPath));
                File.Copy(_selectedImagePath, destPath, true);
                _currentProduct.Image = "images/" + fileName;
            }

            // Сохранение в БД
            if (_currentProduct.ProductID == 0)
                DB.Products.Add(_currentProduct);
            DB.SaveChanges();

            MessageBox.Show("Сохранено!");
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить '{_currentProduct.ProductName}'?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var productToDelete = DB.Products.Find(_currentProduct.ProductID);
                if (productToDelete != null)
                {
                    DB.Products.Remove(productToDelete);
                    DB.SaveChanges();
                    MessageBox.Show("Товар удалён.");
                }
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _openWindowsCount--;
            base.OnClosed(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
