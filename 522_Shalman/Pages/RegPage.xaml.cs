using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Text;

namespace _522_Shalman.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => txtbxLog.Focus();
        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passBxFrst.Focus();
        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passBxScnd.Focus();
        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => txtbxFIO.Focus();

        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = txtbxLog.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = txtbxFIO.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = passBxFrst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = passBxScnd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            Entities db = new Entities();
            var user = db.User.AsNoTracking().FirstOrDefault(u => u.Login == txtbxLog.Text);
            if (user != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!");
                return;
            }

            if (passBxFrst.Password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий (мин. 6 символов)!");
                return;
            }

            bool en = true;
            bool number = false;

            foreach (char c in passBxFrst.Password)
            {
                if (char.IsDigit(c)) number = true;
                else if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z')) en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return;
            }

            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return;
            }

            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }


            User userObject = new User
            {
                FIO = txtbxFIO.Text,
                Login = txtbxLog.Text,
                Password = GetHash(passBxFrst.Password), 
                Role = (comboBxRole.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Photo = "photos_icon_156733"
            };

            try
            {
                db.User.Add(userObject);
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    MessageBox.Show($"Entity of type {eve.Entry.Entity.GetType().Name} has the following validation errors:");
                    foreach (var ve in eve.ValidationErrors)
                    {
                        MessageBox.Show($"- Property: {ve.PropertyName}, Error: {ve.ErrorMessage}");
                    }
                }
            }


            MessageBox.Show("Пользователь успешно зарегистрирован!");

            txtbxLog.Clear();
            txtbxFIO.Clear();
            passBxFrst.Clear();
            passBxScnd.Clear();
            comboBxRole.SelectedIndex = 1;
        }
    }
}
