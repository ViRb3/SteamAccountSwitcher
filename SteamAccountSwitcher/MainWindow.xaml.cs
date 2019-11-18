using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using SteamAccountSwitcher.Properties;

namespace SteamAccountSwitcher
{
    public partial class MainWindow : Window
    {
        private readonly Steam _steam;
        private AccountList _accountList;
        private static readonly FileInfo AccountFile = new FileInfo("accounts.ini");

        public MainWindow()
        {
            InitializeComponent();

            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            if (Settings.Default.Maximized) WindowState = WindowState.Maximized;

            _accountList = new AccountList();

            if (AccountFile.Exists)
                ReadAccountsFromFile();
            else
                WriteAccountsToFile();

            listBoxAccounts.ItemsSource = _accountList.Accounts;
            listBoxAccounts.Items.Refresh();

            _accountList.InstallDir = SelectSteamFile(@"C:\Program Files (x86)\Steam");
            if (_accountList.InstallDir == null)
            {
                MessageBox.Show(
                    "You cannot use SteamAccountSwitcher without selecting your Steam.exe. Program will close now.",
                    "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _steam = new Steam(_accountList.InstallDir);
        }

        private static string SelectSteamFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Steam |steam.exe";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select your Steam Installation";
            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        private void buttonLogout_Click(object sender, RoutedEventArgs e)
        {
            _steam.LogoutSteam();
        }

        private void buttonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            var newAccWindow = new AddAccount();
            newAccWindow.Owner = this;

            if (newAccWindow.ShowDialog() != true)
                return;

            _accountList.Accounts.Add(newAccWindow.Account);
            listBoxAccounts.Items.Refresh();
        }

        private void WriteAccountsToFile()
        {
            var xmlAccounts = ToXML(_accountList);
            var file = new StreamWriter(AccountFile.FullName);
            file.Write(xmlAccounts);
            file.Close();
        }

        private void ReadAccountsFromFile()
        {
            var text = File.ReadAllText(AccountFile.FullName);
            _accountList = FromXML<AccountList>(text);
        }

        private static T FromXML<T>(string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(stringReader);
            }
        }

        private static string ToXML<T>(T obj)
        {
            using (var stringWriter = new StringWriter(new StringBuilder()))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }

        private void listBoxAccounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedAcc = (SteamAccount) listBoxAccounts.SelectedItem;
            _steam.StartSteamAccount(selectedAcc);
        }

        private void buttonEditAccount_Click(object sender, RoutedEventArgs e)
        {
            var newAccWindow = new AddAccount((SteamAccount) listBoxAccounts.SelectedItem);
            newAccWindow.Owner = this;
            newAccWindow.ShowDialog();

            _accountList.Accounts[listBoxAccounts.SelectedIndex] = newAccWindow.Account;
            listBoxAccounts.Items.Refresh();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WriteAccountsToFile();

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Settings.Default.Top = RestoreBounds.Top;
                Settings.Default.Left = RestoreBounds.Left;
                Settings.Default.Height = RestoreBounds.Height;
                Settings.Default.Width = RestoreBounds.Width;
                Settings.Default.Maximized = true;
            }
            else
            {
                Settings.Default.Top = Top;
                Settings.Default.Left = Left;
                Settings.Default.Height = Height;
                Settings.Default.Width = Width;
                Settings.Default.Maximized = false;
            }

            Settings.Default.Save();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var itemClicked = (Image) e.Source;

            var selectedAcc = (SteamAccount) itemClicked.DataContext;
            var dialogResult =
                MessageBox.Show("Are you sure you want to delete the '" + selectedAcc.Name + "' account?",
                    "Delete Account", MessageBoxButton.YesNo);

            if (dialogResult != MessageBoxResult.Yes)
                return;

            _accountList.Accounts.Remove((SteamAccount) listBoxAccounts.SelectedItem);
            listBoxAccounts.Items.Refresh();
        }
    }
}