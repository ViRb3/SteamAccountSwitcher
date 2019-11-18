using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Web.Script.Serialization;

namespace SteamAccountSwitcher
{
    public partial class MainWindow : Window
    {
        private readonly Steam _steam;
        private SteamData _steamData;
        private static readonly FileInfo SteamDataFileInfo = new FileInfo("steam-data.json");

        public MainWindow()
        {
            InitializeComponent();
            _steamData = new SteamData();

            if (SteamDataFileInfo.Exists)
                ReadSteamData();
            else
                SaveSteamData();

            listBoxAccounts.ItemsSource = _steamData.Accounts;
            UpdateListBoxView();

            _steamData.SteamFilePath = SelectSteamFile(@"C:\Program Files (x86)\Steam");
            if (_steamData.SteamFilePath == null)
            {
                MessageBox.Show(
                    "You cannot use SteamAccountSwitcher without selecting your Steam.exe. Program will close now.",
                    "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _steam = new Steam(_steamData.SteamFilePath);
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

            _steamData.Accounts.Add(newAccWindow.Account);
            UpdateListBoxView();
        }

        private void SaveSteamData()
        {
            var file = new StreamWriter(SteamDataFileInfo.FullName);
            file.Write(new JavaScriptSerializer().Serialize(_steamData));
            file.Close();
        }

        private void ReadSteamData()
        {
            var text = File.ReadAllText(SteamDataFileInfo.FullName);
            _steamData = new JavaScriptSerializer().Deserialize<SteamData>(text);
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

            _steamData.Accounts[listBoxAccounts.SelectedIndex] = newAccWindow.Account;
            UpdateListBoxView();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSteamData();
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

            _steamData.Accounts.Remove((SteamAccount) listBoxAccounts.SelectedItem);
            UpdateListBoxView();
        }

        private void UpdateListBoxView()
        {
            listBoxAccounts.Items.Refresh();
            if (_steamData.Accounts.Any())
                buttonEditAccount.IsEnabled = true;
        }
    }
}