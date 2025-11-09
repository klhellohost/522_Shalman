using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _522_Shalman.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            UpdateUsers();
        }

        /// <summary>
        /// Обновляет список пользователей с учетом фильтров, сортировки и чекбокса
        /// </summary>
        private void UpdateUsers()
        {
            if (!IsInitialized)
                return;

            try
            {
                // Получаем список пользователей из базы
                List<User> currentUsers = Entities.GetContext().User.ToList();

                // Фильтр по ФИО
                if (!string.IsNullOrWhiteSpace(tbFioFilter.Text))
                {
                    currentUsers = currentUsers
                        .Where(x => x.FIO != null &&
                                    x.FIO.ToLower().Contains(tbFioFilter.Text.ToLower()))
                        .ToList();
                }

                // Фильтр: только администраторы
                if (cbAdminOnly.IsChecked == true)
                {
                    currentUsers = currentUsers
                        .Where(x => x.Role != null && x.Role.ToLower().Contains("admin"))
                        .ToList();
                }

                // Сортировка по ФИО
                if (cmbSort.SelectedIndex == 0)
                    currentUsers = currentUsers.OrderBy(x => x.FIO).ToList();
                else
                    currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList();

                // Привязываем данные к ListView
                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка пользователей:\n{ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // События для фильтров и сортировки
        private void tbFioFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            tbFioFilter.Text = string.Empty;
            cmbSort.SelectedIndex = 0;
            cbAdminOnly.IsChecked = false;
            UpdateUsers();
        }
    }
}
