using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        // 🔥 Click vào ảnh để xem full size
        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ChatMessageViewModel msg && msg.ImageSource != null)
            {
                // Tạo window hiển thị ảnh full
                var imageWindow = new Window
                {
                    Title = $"🖼 {msg.FileName}",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Colors.Black),
                    Content = new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new Image
                        {
                            Source = msg.ImageSource,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(10)
                        }
                    }
                };
                imageWindow.Show();
            }
        }
    }
}