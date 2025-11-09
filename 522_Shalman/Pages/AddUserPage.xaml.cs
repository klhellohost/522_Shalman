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

namespace _522_Shalman.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null) _currentUser = selectedUser;
            DataContext = _currentUser;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login)) errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password)) errors.AppendLine("Укажите пароль!");
            if (string.IsNullOrWhiteSpace(_currentUser.Role)) errors.AppendLine("Выберите роль!");
            if (string.IsNullOrWhiteSpace(_currentUser.FIO)) errors.AppendLine("Укажите ФИО!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentUser.ID == 0)
                Entities.GetContext().User.Add(_currentUser);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = TBPass.Text = TBFio.Text = TBPhoto.Text = "";
            cmbRole.SelectedIndex = -1;
        }
    }
}
