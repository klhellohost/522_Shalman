using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using _522_Shalman; // Пространство имен контекста

namespace _522_Shalman.Pages
{
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public Payment CurrentPayment => _currentPayment;

        public AddPaymentPage(Payment selectedPayment = null)
        {
            InitializeComponent();
            if (selectedPayment != null)
            {
                _currentPayment = selectedPayment;
            }
            DataContext = new DateTime(2025, 10, 31);
            cmbUser.ItemsSource = Entities.GetContext().User.ToList();
            cmbCategory.ItemsSource = Entities.GetContext().Category.ToList();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(TBName.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            if (cmbUser.SelectedValue == null || (int)cmbUser.SelectedValue <= 0)
            {
                MessageBox.Show("Выберите пользователя!");
                return;
            }

            if (cmbCategory.SelectedValue == null || (int)cmbCategory.SelectedValue <= 0)
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }

            if (!dpDate.SelectedDate.HasValue || dpDate.SelectedDate.Value.Year < 1753)
            {
                MessageBox.Show("Выберите корректную дату!");
                return;
            }


            _currentPayment.Name = TBName.Text.Trim();
            _currentPayment.UserID = (int)cmbUser.SelectedValue;
            _currentPayment.CategoryID = (int)cmbCategory.SelectedValue;
            _currentPayment.Date = dpDate.SelectedDate.Value;
            _currentPayment.Num = int.TryParse(TBNum.Text, out int n) ? n : 0;
            _currentPayment.Price = decimal.TryParse(TBPrice.Text, out decimal p) ? p : 0m;

            try
            {
                var context = Entities.GetContext();

                if (_currentPayment.ID == 0)
                    context.Payment.Add(_currentPayment);
                else
                    context.Entry(_currentPayment).State = EntityState.Modified;

                context.SaveChanges();
                MessageBox.Show("Успешно сохранено!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                var error = new StringBuilder(ex.Message);
                var inner = ex.InnerException;
                while (inner != null)
                {
                    error.AppendLine("\n↳ " + inner.Message);
                    inner = inner.InnerException;
                }
                MessageBox.Show(error.ToString(), "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            _currentPayment = new Payment();
            DataContext = this;

            TBName.Text = "";
            TBNum.Text = "";
            TBPrice.Text = "";
            dpDate.SelectedDate = null;
            cmbUser.SelectedIndex = -1;
            cmbCategory.SelectedIndex = -1;
        }

    }
}