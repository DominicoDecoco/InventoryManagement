using InventoryManagement.Model;
using Microsoft.Win32;
using Npgsql;
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
    /// Логика взаимодействия для ItemMenu.xaml
    /// </summary>
    public partial class ItemMenu : Window
    {
        private string connectionString = "Host=127.0.0.1;Username=postgres;Password=1234;Database=ListOfSubjects";
        private List<Item> items = new List<Item>();
        private byte[] selectedImage;

        public ItemMenu()
        {
            InitializeComponent();
            LoadTypeItems();
            LoadItems();
        }

        private void LoadTypeItems()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("SELECT * FROM TypeItems", conn);
                    var reader = cmd.ExecuteReader();
                    List<TypeItem> typeItems = new List<TypeItem>();

                    while (reader.Read())
                    {
                        typeItems.Add(new TypeItem
                        {
                            IDTypeItem = reader.GetInt32(0),
                            NameTypeItems = reader.GetString(1)
                        });
                    }
                    TypeItemConboBox.ItemsSource = typeItems;
                    TypeItemConboBox.DisplayMemberPath = "NameTypeItems";
                    TypeItemConboBox.SelectedValuePath = "IDTypeItem";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки типов предметов: " + ex.Message);
            }
        }

        private void LoadItems()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Загрузим все типы в словарь
                    var typeCmd = new NpgsqlCommand("SELECT IDTypeItem, NameTypeItems FROM TypeItems", conn);
                    var typeReader = typeCmd.ExecuteReader();
                    var typeDict = new Dictionary<int, TypeItem>();

                    while (typeReader.Read())
                    {
                        var typeItem = new TypeItem
                        {
                            IDTypeItem = typeReader.GetInt32(0),
                            NameTypeItems = typeReader.GetString(1)
                        };
                        typeDict[typeItem.IDTypeItem] = typeItem;
                    }

                    typeReader.Close();

                    // Теперь загрузим сами предметы
                    var itemCmd = new NpgsqlCommand("SELECT IDItem, NameItem, TypeIt, ImageQuestion FROM Items", conn);
                    var itemReader = itemCmd.ExecuteReader();

                    items.Clear();
                    while (itemReader.Read())
                    {
                        var typeId = itemReader.GetInt32(2);
                        var item = new Item
                        {
                            IDItem = itemReader.GetInt32(0),
                            NameItem = itemReader.GetString(1),
                            TypeIt = typeId,
                            ImageQuestion = itemReader.IsDBNull(3) ? null : (byte[])itemReader["ImageQuestion"],
                            TypeItem = typeDict.ContainsKey(typeId) ? typeDict[typeId] : null
                        };
                        items.Add(item);
                    }

                    itemReader.Close();
                    ItemsView.ItemsSource = null;
                    ItemsView.ItemsSource = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки предметов: " + ex.Message);
            }
        }

        private void SelectImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImage = File.ReadAllBytes(openFileDialog.FileName);
                ItemImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemTextBox.Text) || TypeItemConboBox.SelectedValue == null || selectedImage == null)
            {
                MessageBox.Show("Введите название предмета, выберите его тип и загрузите изображение.");
                return;
            }

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("INSERT INTO Items (NameItem, TypeIt, ImageQuestion) VALUES (@name, @type, @image)", conn);
                    cmd.Parameters.AddWithValue("name", ItemTextBox.Text);
                    cmd.Parameters.AddWithValue("type", (int)TypeItemConboBox.SelectedValue);
                    cmd.Parameters.AddWithValue("image", selectedImage);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Предмет добавлен!");
                    ItemTextBox.Clear();
                    ItemImage.Source = null;
                    selectedImage = null;
                    LoadItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления предмета: " + ex.Message);
            }
        }

        private void GiveItem(object sender, RoutedEventArgs e)
        {
            if (ItemsView.SelectedItem is Item selectedItem)
            {
                StartMenu startMenu = new StartMenu(selectedItem);
                startMenu.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Выберите предмет для выдачи.");
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (ItemsView.SelectedItem is Item selectedItem)
            {
                try
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        var cmd = new NpgsqlCommand("DELETE FROM Items WHERE IDItem = @id", conn);
                        cmd.Parameters.AddWithValue("id", selectedItem.IDItem);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Предмет удален!");
                        LoadItems();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления предмета: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите предмет для удаления.");
            }
        }

        private void OpenInventory(object sender, RoutedEventArgs e)
        {
            StartMenu inventory = new StartMenu();
            inventory.Show();
            Close();
            
        }

    }
}
