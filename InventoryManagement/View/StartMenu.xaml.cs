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
        private static List<Item> globalInventoryItems = new List<Item>(new Item[20]); // Глобальный инвентарь
        private List<Button> backpackSlots = new List<Button>();
        private Point startPoint; // Переменная для отслеживания начальной позиции нажатия

        public StartMenu() : this(null) { }

        public StartMenu(Item receivedItem)
        {
            InitializeComponent();
            InitializeBackpackSlots();
            LoadItemsToBackpack();
            LoadEquipment();
            InitializeEquipmentSlots();
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
            int freeIndex = globalInventoryItems.FindIndex(i => i == null);
            if (freeIndex != -1) // Есть свободный слот
            {
                globalInventoryItems[freeIndex] = item;
                UpdateSlotContent(freeIndex);
            }
            else
            {
                MessageBox.Show("Нет свободных слотов в рюкзаке!");
            }
        }

        private void LoadItemsToBackpack()
        {
            for (int i = 0; i < backpackSlots.Count; i++)
            {
                var item = globalInventoryItems[i]; // Теперь 100% в пределах границ
                var button = backpackSlots[i];

                if (item != null)
                {
                    if (item.ImageQuestion != null && item.ImageQuestion.Length > 0)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (var stream = new MemoryStream(item.ImageQuestion))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }
                        button.Content = new Image { Source = bitmap, Width = 40, Height = 40 };
                    }
                    else
                    {
                        button.Content = item.NameItem ?? "Без названия";
                    }
                }
                else
                {
                    button.Content = null; // Пустой слот
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
            if (clickedButton != null)
            {
                int index = backpackSlots.IndexOf(clickedButton);
                if (index != -1)
                {
                    globalInventoryItems[index] = null; // Убираем предмет из глобального инвентаря
                    clickedButton.Content = null;
                }
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
                    int index = backpackSlots.IndexOf(sourceButton);
                    if (index >= 0 && index < globalInventoryItems.Count)
                    {
                        Item draggedItem = globalInventoryItems[index];

                        DataObject data = new DataObject(typeof(Item), draggedItem);
                        DragDrop.DoDragDrop(sourceButton, data, DragDropEffects.Move);
                    }
                }
            }
        }
        private Dictionary<string, int> equipmentSlots = new Dictionary<string, int>
        {
        { "Necklace", 1 }, // Ожерелье
        { "Torso", 2 },    // Туловище
        { "Pants", 3 },    // Штаны
        { "Gloves", 4 }    // Перчатки
        };
        private static Dictionary<string, Item> globalEquipment = new Dictionary<string, Item>
{
    { "Necklace", null },
    { "Torso", null },
    { "Pants", null },
    { "Gloves", null }
};

        private void Slot_Drop(object sender, DragEventArgs e)
        {
            Button targetButton = sender as Button;
            if (targetButton == null || !e.Data.GetDataPresent(typeof(Item)))
                return;

            Item droppedItem = e.Data.GetData(typeof(Item)) as Item;
            if (droppedItem == null)
                return;

            // Ищем источник предмета (рюкзак или экипировка)
            int sourceIndex = globalInventoryItems.IndexOf(droppedItem);
            bool fromBackpack = sourceIndex != -1;

            // Если цель — ячейка рюкзака
            if (backpackSlots.Contains(targetButton))
            {
                int targetIndex = backpackSlots.IndexOf(targetButton);

                if (fromBackpack)
                {
                    // Перемещение внутри рюкзака
                    (globalInventoryItems[sourceIndex], globalInventoryItems[targetIndex]) =
                        (globalInventoryItems[targetIndex], globalInventoryItems[sourceIndex]);

                    UpdateSlotContent(sourceIndex);
                    UpdateSlotContent(targetIndex);
                }
                else
                {
                    // Перемещение из экипировки в рюкзак
                    if (globalInventoryItems[targetIndex] == null)
                    {
                        globalInventoryItems[targetIndex] = droppedItem;
                        UpdateSlotContent(targetIndex);

                        // Очищаем экипировку
                        ClearEquipmentSlot(droppedItem);
                    }
                    else
                    {
                        MessageBox.Show("Слот рюкзака занят!");
                    }
                }
            }
            else if (equipmentSlots.TryGetValue(targetButton.Name, out int requiredType))
            {
                if (droppedItem.TypeIt == requiredType)
                {
                    if (globalEquipment[targetButton.Name] != null)
                    {
                        return;
                    }
                    // Если предмет был в рюкзаке, удаляем его
                    if (fromBackpack)
                    {
                        globalInventoryItems[sourceIndex] = null;
                        UpdateSlotContent(sourceIndex);
                    }
                    else
                    {
                        // Удаляем из старой экипировки
                        ClearEquipmentSlot(droppedItem);
                    }

                    SetEquipmentSlot(targetButton, droppedItem);
                }
                else
                {
                    MessageBox.Show("Этот предмет не подходит для выбранного слота экипировки!");
                }
            }
        }

        private void SetEquipmentSlot(Button slot, Item item)
        {
            string slotName = slot.Name;
            globalEquipment[slotName] = item;

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
                slot.Content = new Image { Source = bitmap, Width = 40, Height = 40 };
            }
            else
            {
                slot.Content = item.NameItem ?? "Без названия";
            }

            slot.Tag = item;
        }
        private void LoadEquipment()
        {
            foreach (var pair in globalEquipment.ToList()) // Создаем копию, чтобы избежать ошибки
            {
                Button button = this.FindName(pair.Key) as Button;
                if (button != null && pair.Value != null)
                {
                    // Восстанавливаем без изменения globalEquipment — он уже содержит нужный предмет
                    if (pair.Value.ImageQuestion != null)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (var stream = new MemoryStream(pair.Value.ImageQuestion))
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }
                        button.Content = new Image { Source = bitmap, Width = 40, Height = 40 };
                    }
                    else
                    {
                        button.Content = pair.Value.NameItem ?? "Без названия";
                    }

                    button.Tag = pair.Value;
                }
            }
        }
        private void ClearEquipmentSlot(Item item)
        {
            foreach (var pair in equipmentSlots)
            {
                if (globalEquipment.TryGetValue(pair.Key, out Item equippedItem) && equippedItem == item)
                {
                    globalEquipment[pair.Key] = null; // Удалить из словаря — ВАЖНО

                    Button slot = this.FindName(pair.Key) as Button;
                    if (slot != null)
                    {
                        slot.Content = null;
                        slot.Tag = null;
                    }
                }
            }
        }
        private void EquipmentSlot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button sourceButton = sender as Button;
            if (sourceButton == null || sourceButton.Tag == null)
                return;

            Item draggedItem = sourceButton.Tag as Item;
            if (draggedItem == null)
                return;

            DataObject data = new DataObject(typeof(Item), draggedItem);
            DragDrop.DoDragDrop(sourceButton, data, DragDropEffects.Move);
        }
        private void InitializeEquipmentSlots()
        {
            foreach (var pair in equipmentSlots)
            {
                Button button = this.FindName(pair.Key) as Button;
                if (button != null)
                {
                    button.PreviewMouseLeftButtonDown += EquipmentSlot_MouseDown;
                    button.AllowDrop = true;
                    button.Drop += Slot_Drop; //  Обработка бросания в экипировку
                }
            }
        }
        private void UpdateSlotContent(int index)
        {
            // Проверка, что индекс в допустимых пределах
            if (index < 0 || index >= globalInventoryItems.Count || index >= backpackSlots.Count)
            {
                Console.WriteLine($"Ошибка: индекс {index} выходит за границы массива.");
                return;
            }

            var item = globalInventoryItems[index];
            var button = backpackSlots[index];

            // Проверка, что слот и кнопка существуют
            if (button == null)
            {
                Console.WriteLine($"Ошибка: Кнопка для слота {index} не найдена.");
                return;
            }

            if (item == null)
            {
                // Если в слоте нет предмета, очищаем кнопку
                button.Content = null;
                return;
            }

            if (item.ImageQuestion != null && item.ImageQuestion.Length > 0) // Проверяем, что изображение не пустое
            {
                BitmapImage bitmap = new BitmapImage();
                using (var stream = new MemoryStream(item.ImageQuestion))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                button.Content = new Image { Source = bitmap, Width = 40, Height = 40 };
            }
            else
            {
                button.Content = item.NameItem ?? "Без названия"; // Защита от null
            }
        }

        private void DeleteInventory(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Item)))
                return;

            Item itemToDelete = e.Data.GetData(typeof(Item)) as Item;

            if (itemToDelete == null)
                return;

            // Удаление из рюкзака
            int index = globalInventoryItems.IndexOf(itemToDelete);
            if (index != -1)
            {
                globalInventoryItems[index] = null;
                UpdateSlotContent(index);
                return;
            }

            // Удаление из экипировки
            foreach (var pair in equipmentSlots)
            {
                if (globalEquipment.TryGetValue(pair.Key, out Item equippedItem) && equippedItem == itemToDelete)
                {
                    globalEquipment[pair.Key] = null;

                    Button slot = this.FindName(pair.Key) as Button;
                    if (slot != null && slot.Tag == itemToDelete)
                    {
                        slot.Content = null;
                        slot.Tag = null;
                        break;
                    }
                }
            }
        }
    }
}
