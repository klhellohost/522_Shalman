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
    /// Логика взаимодействия для CategoryTabPage.xaml
    /// </summary>
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            UpdateData();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void UpdateData()
        {
            DataGridCategory.ItemsSource = Entities.GetContext().Category.ToList();
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
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridCategory.SelectedItems.Cast<Category>().ToList();
            if (!selected.Any()) return;

            if (MessageBox.Show($"Удалить {selected.Count} категорий?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    Entities.GetContext().Category.RemoveRange(selected);
                    Entities.GetContext().SaveChanges();
                    UpdateData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var cat = (sender as Button)?.Tag as Category;
            NavigationService?.Navigate(new AddCategoryPage(cat));
        }
    }
}
