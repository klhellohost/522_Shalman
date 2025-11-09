using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using _522_Shalman.Pages;

namespace _522_Shalman
{
    /// <summary>
    /// Логика взаимодействия для ChangePassPage.xaml
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }


        private string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(TbLogin.Text) ||
                string.IsNullOrWhiteSpace(CurrentPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(NewPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 6 символов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string hashedOldPass = GetHash(CurrentPasswordBox.Password);
            var user = Entities.GetContext().User
                .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedOldPass);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или текущий пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (CurrentPasswordBox.Password == NewPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль не должен совпадать со старым!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            user.Password = GetHash(NewPasswordBox.Password);
            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Пароль успешно изменён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);


                NavigationService?.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
