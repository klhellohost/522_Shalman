using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using _522_Shalman.Pages;

namespace _522_Shalman
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = false;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += Frame_Navigated;
            MainFrame.NavigationService.Navigated += NavigationService_Navigated;
            ApplyTheme();
        }

            private void ApplyTheme()
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                for (int i = dictionaries.Count - 1; i >= 0; i--)
                {
                    var dict = dictionaries[i];
                    if (dict.Source != null && (dict.Source.OriginalString.Contains("Light.xaml") || dict.Source.OriginalString.Contains("Dark.xaml")))
                    {
                        dictionaries.RemoveAt(i);
                    }
                }


                var themeUri = new Uri(_isDarkTheme ? "Dark.xaml" : "Light.xaml", UriKind.Relative);
                var themeDictionary = new ResourceDictionary { Source = themeUri };
                dictionaries.Insert(0, themeDictionary);


                btnThemeToggle.Content = "Сменить тему";
            }

            private void btnThemeToggle_Click(object sender, RoutedEventArgs e)
            {
                _isDarkTheme = !_isDarkTheme;
                ApplyTheme();
            }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (MainFrame.Content is Page page)
            {
                string pageName = page.GetType().Name;
                switch (pageName)
                {
                    case "AuthPage":
                        this.Title = "Страница авторизации";
                        break;
                    case "RegPage":
                        this.Title = "Страница регистрации";
                        break;
                    case "AdminPage":
                        this.Title = "Страница администратора";
                        break;
                    case "UserPage":
                        this.Title = "Страница пользователя";
                        break;
                    case "UserTabPage":
                        this.Title = "Страница таблицы пользователей";
                        break;
                    case "AdminTabPage":
                        this.Title = "Страница таблицы администраторов";
                        break;
                    case "PaymentTabPage":
                        this.Title = "Страница таблицы оплат";
                        break;
                    case "DiagrammTabPage":
                        this.Title = "Страница диаграмм";
                        break;
                    case "AddCategoryPage":
                        this.Title = "Страница управления категориями";
                        break;
                    case "AddPaymentPage":
                        this.Title = "Страница управления платежами";
                        break;
                    case "AddUserPage":
                        this.Title = "Страница управления пользователями";
                        break;
                    default:
                        this.Title = pageName;
                        break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (o, t) => { tbDateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверенны, что хотите выйти из программы?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            btnBack.IsEnabled = MainFrame.CanGoBack;
        }
    }
}
