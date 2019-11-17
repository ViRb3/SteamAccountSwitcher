using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using SteamAccountSwitcher.Properties;

namespace SteamAccountSwitcher
{
    /// ****
    /// SteamAccountSwitcher
    /// Copyright by Christoph Wedenig
    /// ****
    public partial class MainWindow : Window
    {
        private AccountList accountList;

        private readonly string settingsSave;
        private readonly Steam steam;

        public MainWindow()
        {
            InitializeComponent();

            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            if (Settings.Default.Maximized) WindowState = WindowState.Maximized;

            accountList = new AccountList();

            //Get directory of Executable
            settingsSave = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                .TrimStart(@"file:\\".ToCharArray());


            try
            {
                ReadAccountsFromFile();
            }
            catch
            {
                //Maybe create file?
            }


            listBoxAccounts.ItemsSource = accountList.Accounts;
            listBoxAccounts.Items.Refresh();

            if (accountList.InstallDir == "" || accountList.InstallDir == null)
            {
                accountList.InstallDir = SelectSteamFile(@"C:\Program Files (x86)\Steam");
                if (accountList.InstallDir == null)
                {
                    MessageBox.Show(
                        "You cannot use SteamAccountSwitcher without selecting your Steam.exe. Program will close now.",
                        "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }

            steam = new Steam(accountList.InstallDir);
        }

        private string SelectSteamFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter =
                "Steam |steam.exe";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select your Steam Installation";
            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        private void buttonLogout_Click(object sender, RoutedEventArgs e)
        {
            steam.LogoutSteam();
        }

        private void buttonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            var newAccWindow = new AddAccount();
            newAccWindow.Owner = this;
            newAccWindow.ShowDialog();

            if (newAccWindow.Account != null)
            {
                accountList.Accounts.Add(newAccWindow.Account);

                listBoxAccounts.Items.Refresh();
            }
        }

        public void WriteAccountsToFile()
        {
            var xmlAccounts = ToXML(accountList);
            var file = new StreamWriter(settingsSave + "\\accounts.ini");
            file.Write(xmlAccounts);
            file.Close();
        }

        public void ReadAccountsFromFile()
        {
            var text = File.ReadAllText(settingsSave + "\\accounts.ini");
            accountList = FromXML<AccountList>(text);
        }

        public static T FromXML<T>(string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(stringReader);
            }
        }

        public string ToXML<T>(T obj)
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
            steam.StartSteamAccount(selectedAcc);
        }


        private void buttonEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAccounts.SelectedItem != null)
            {
                var newAccWindow = new AddAccount((SteamAccount) listBoxAccounts.SelectedItem);
                newAccWindow.Owner = this;
                newAccWindow.ShowDialog();

                if (newAccWindow.Account.Username != "" && newAccWindow.Account.Password != "")
                {
                    accountList.Accounts[listBoxAccounts.SelectedIndex] = newAccWindow.Account;

                    listBoxAccounts.Items.Refresh();
                }
            }
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
            if (dialogResult == MessageBoxResult.Yes)
            {
                accountList.Accounts.Remove((SteamAccount) listBoxAccounts.SelectedItem);
                listBoxAccounts.Items.Refresh();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                //do something else
            }
        }
    }
}