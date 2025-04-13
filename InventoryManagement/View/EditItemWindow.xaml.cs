using InventoryManagement.Model;
using Microsoft.Win32;
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

namespace InventoryManagement.View
{
    /// <summary>
    /// Логика взаимодействия для EditItemWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Window
    {
        public Item EditedItem { get; private set; }
        private byte[] newImageData;
        private List<TypeItem> typeItems;
        public EditItemWindow(Item itemToEdit, List<TypeItem> availableTypes)
        {
            InitializeComponent();
            EditedItem = itemToEdit;
            typeItems = availableTypes;

            NameTextBox.Text = itemToEdit.NameItem;
            TypeComboBox.ItemsSource = typeItems;
            TypeComboBox.DisplayMemberPath = "NameTypeItems";
            TypeComboBox.SelectedValuePath = "IDTypeItem";
            TypeComboBox.SelectedValue = itemToEdit.TypeIt;

            if (itemToEdit.ImageQuestion != null)
            {
                BitmapImage bitmap = new BitmapImage();
                using (var stream = new MemoryStream(itemToEdit.ImageQuestion))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                ItemImage.Source = bitmap;
            }
        }
        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg",
            };
            if (openFileDialog.ShowDialog() == true)
            {
                newImageData = File.ReadAllBytes(openFileDialog.FileName);
                ItemImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            EditedItem.NameItem = NameTextBox.Text;
            EditedItem.TypeIt = (int)TypeComboBox.SelectedValue;
            if (newImageData != null)
                EditedItem.ImageQuestion = newImageData;

            DialogResult = true;
            Close();
        }
    }
}
