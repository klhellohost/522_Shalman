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
    /// Логика взаимодействия для UsersTabPage.xaml
    /// </summary>
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            UpdateData();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void UpdateData()
        {
            DataGridUser.ItemsSource = Entities.GetContext().User.ToList();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                UpdateData();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridUser.SelectedItems.Cast<User>().ToList();
            if (!selected.Any()) return;

            if (MessageBox.Show($"Удалить {selected.Count} записей?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Entities.GetContext().User.RemoveRange(selected);
                    Entities.GetContext().SaveChanges();
                    MessageBox.Show("Удалено!");
                    UpdateData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button)?.Tag as User;
            NavigationService?.Navigate(new AddUserPage(user));
        }
    }
}
