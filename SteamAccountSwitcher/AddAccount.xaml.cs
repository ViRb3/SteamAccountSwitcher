using System;
using System.Windows;

namespace SteamAccountSwitcher
{
    /// <summary>
    ///     Interaction logic for AddAccount.xaml
    /// </summary>
    public partial class AddAccount : Window
    {
        public AddAccount()
        {
            Account = new SteamAccount();
            InitializeComponent();
            comboBoxType.ItemsSource = Enum.GetValues(typeof(AccountType));
        }

        public AddAccount(SteamAccount editAccount)
        {
            InitializeComponent();
            Account = editAccount;
            comboBoxType.ItemsSource = Enum.GetValues(typeof(AccountType));

            comboBoxType.SelectedItem = editAccount.Type;
            textBoxProfilename.Text = editAccount.Name;
            textBoxUsername.Text = editAccount.Username;
            textBoxPassword.Password = editAccount.Password;
        }

        public SteamAccount Account { get; }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxType.SelectedItem == null || string.IsNullOrWhiteSpace(textBoxProfilename.Text) ||
                string.IsNullOrWhiteSpace(textBoxUsername.Text) || string.IsNullOrWhiteSpace(textBoxPassword.Password))
            {
                MessageBox.Show("Invalid account data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Account.Type = (AccountType) comboBoxType.SelectedItem;
            Account.Name = textBoxProfilename.Text;
            Account.Username = textBoxUsername.Text;
            Account.Password = textBoxPassword.Password;

            DialogResult = true;
            Close();
        }
    }
}