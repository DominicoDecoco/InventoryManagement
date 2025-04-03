using InventoryManagement.Model;
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
    /// Логика взаимодействия для StartMenu.xaml
    /// </summary>
    public partial class StartMenu : Window
    {
        private static List<Item> globalInventoryItems = new List<Item>(); // Глобальный инвентарь
        private string connectionString = "Host=127.0.0.1;Username=postgres;Password=1234;Database=ListOfSubjects";
        private List<Button> backpackSlots = new List<Button>();
        private Point startPoint; // Переменная для отслеживания начальной позиции нажатия

        public StartMenu() : this(null) { }

        public StartMenu(Item receivedItem)
        {
            InitializeComponent();
            InitializeBackpackSlots();
            LoadItemsToBackpack();

            if (receivedItem != null)
            {
                AddItemToBackpack(receivedItem);
            }
        }

        private void InitializeBackpackSlots()
        {
            backpackSlots.AddRange(new List<Button> {
            OneRow.Children[0] as Button, OneRow.Children[1] as Button, OneRow.Children[2] as Button, OneRow.Children[3] as Button, OneRow.Children[4] as Button,
            SecondRow.Children[0] as Button, SecondRow.Children[1] as Button, SecondRow.Children[2] as Button, SecondRow.Children[3] as Button, SecondRow.Children[4] as Button,
            ThreeRow.Children[0] as Button, ThreeRow.Children[1] as Button, ThreeRow.Children[2] as Button, ThreeRow.Children[3] as Button, ThreeRow.Children[4] as Button,
            FourRow.Children[0] as Button, FourRow.Children[1] as Button, FourRow.Children[2] as Button, FourRow.Children[3] as Button, FourRow.Children[4] as Button,
        });
        }

        private void AddItemToBackpack(Item item)
        {
            Button freeSlot = backpackSlots.FirstOrDefault(btn => btn.Content == null);
            if (freeSlot != null)
            {
                globalInventoryItems.Add(item); // Сохраняем предмет в глобальном инвентаре

                if (item.ImageQuestion != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(item.ImageQuestion))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    freeSlot.Content = new Image { Source = bitmap, Width = 40, Height = 40 };
                }
                else
                {
                    freeSlot.Content = item.NameItem;
                }
            }
            else
            {
                MessageBox.Show("Нет свободных слотов в рюкзаке!");
            }
        }

        private void LoadItemsToBackpack()
        {
            for (int i = 0; i < globalInventoryItems.Count && i < backpackSlots.Count; i++) // Используем globalInventoryItems
            {
                var item = globalInventoryItems[i];
                if (item.ImageQuestion != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(item.ImageQuestion))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    backpackSlots[i].Content = new Image { Source = bitmap, Width = 40, Height = 40 };
                }
                else
                {
                    backpackSlots[i].Content = item.NameItem;
                }
            }
        }

        private void AdminMenu(object sender, RoutedEventArgs e)
        {
            ItemMenu itemMenu = new ItemMenu();
            itemMenu.Show();
            Close();
        }

        private void DeleteInInventory(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Content != null)
            {
                int index = backpackSlots.IndexOf(clickedButton);
                if (index != -1 && index < globalInventoryItems.Count) // Удаляем из глобального инвентаря
                {
                    globalInventoryItems.RemoveAt(index);
                }
                clickedButton.Content = null;
            }
        }

        private void Slot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null); // Переменная теперь доступна для использования в других методах
        }

        private void Slot_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Button sourceButton = sender as Button;
                if (sourceButton != null && sourceButton.Content != null)
                {
                    Point position = e.GetPosition(null);
                    if (Math.Abs(position.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        DragDrop.DoDragDrop(sourceButton, sourceButton.Content, DragDropEffects.Move);
                    }
                }
            }
        }

        private void Slot_Drop(object sender, DragEventArgs e)
        {
            Button targetButton = sender as Button;
            if (targetButton != null && e.Data.GetDataPresent(typeof(Image)))
            {
                object droppedItem = e.Data.GetData(typeof(Image));

                // Находим исходную кнопку, с которой был перетащен предмет
                Button sourceButton = backpackSlots.FirstOrDefault(btn => btn.Content == droppedItem);
                if (sourceButton != null)
                {
                    // Находим индексы исходной и целевой кнопки в списке слотов
                    int sourceIndex = backpackSlots.IndexOf(sourceButton);
                    int targetIndex = backpackSlots.IndexOf(targetButton);

                    // Проверяем, что индексы в допустимом диапазоне
                    if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex < globalInventoryItems.Count && targetIndex < globalInventoryItems.Count)
                    {
                        // Меняем местами содержимое кнопок
                        var tempContent = backpackSlots[sourceIndex].Content;
                        backpackSlots[sourceIndex].Content = backpackSlots[targetIndex].Content;
                        backpackSlots[targetIndex].Content = tempContent;

                        // Обновляем globalInventoryItems, перемещая элементы в списке
                        var tempItem = globalInventoryItems[sourceIndex];
                        globalInventoryItems[sourceIndex] = globalInventoryItems[targetIndex];
                        globalInventoryItems[targetIndex] = tempItem;

                        // Обновляем отображение инвентаря
                        LoadItemsToBackpack(); // Обновляем UI
                    }
                }
            }
        }
    }
}
