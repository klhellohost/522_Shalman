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
    /// Логика взаимодействия для AddCategoryPage.xaml
    /// </summary>
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory = new Category();

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();
            if (selectedCategory != null) _currentCategory = selectedCategory;
            DataContext = _currentCategory;
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
            {
                MessageBox.Show("Введите название категории!");
                return;
            }

            if (_currentCategory.ID == 0)
                Entities.GetContext().Category.Add(_currentCategory);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Сохранено!");
                NavigationService?.GoBack();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBCategoryName.Text = "";
        }
    }
}
