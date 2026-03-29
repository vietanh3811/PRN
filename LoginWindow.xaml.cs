using System.Windows;

namespace FlowerStore;

public partial class LoginWindow : Window
{
    private const string DefaultUsername = "staff";
    private const string DefaultPassword = "123456";

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (username == DefaultUsername && password == DefaultPassword)
        {
            DialogResult = true;
            Close();
            return;
        }

        ErrorTextBlock.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
        PasswordBox.Clear();
        PasswordBox.Focus();
    }
}
