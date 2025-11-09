using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Data.Entity;

namespace _522_Shalman.Pages
{
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            LoadData();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadData()
        {
            var context = Entities.GetContext();
            DataGridPayment.ItemsSource = context.Payment
                .Include(p => p.User)
                .Include(p => p.Category)
                .ToList();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                var context = Entities.GetContext();


                context.ChangeTracker.Entries()
                    .Where(entry => entry.State != EntityState.Added)
                    .ToList()
                    .ForEach(entry => entry.Reload());

                LoadData();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridPayment.SelectedItems.Cast<Payment>().ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Выберите записи для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Удалить {selected.Count} запись(ей)?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = Entities.GetContext();
                    context.Payment.RemoveRange(selected);
                    context.SaveChanges();
                    MessageBox.Show("Записи удалены!", "Успех", MessageBoxButton.OK);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var payment = (sender as Button).DataContext as Payment;
            if (payment != null)
                NavigationService?.Navigate(new AddPaymentPage(payment));
        }
    }
}
