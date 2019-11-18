using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

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

            if (!File.Exists(_steamData.SteamFilePath))
            {
                var steamFilePath =
                    SelectSteamFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        "Steam"));
                if (steamFilePath == null)
                {
                    MessageBox.Show(
                        "Cannot continue without a valid Steam installation. Program will now close.",
                        "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                _steamData.SteamFilePath = steamFilePath;
            }

            SaveSteamData();

            listBoxAccounts.ItemsSource = _steamData.Accounts;
            UpdateListBoxView();

            _steam = new Steam(_steamData.SteamFilePath);
        }

        private static string SelectSteamFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Steam|steam.exe";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select your Steam installation";
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
            new JsonSerializer().Serialize(file, _steamData);
            file.Close();
        }

        private void ReadSteamData()
        {
            using (var file = File.OpenText(SteamDataFileInfo.FullName))
            using (var text = new JsonTextReader(file))
                _steamData = new JsonSerializer().Deserialize<SteamData>(text);
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
                MessageBox.Show($"Are you sure you want to delete the account '{selectedAcc.Name}'?",
                    "Delete Account", MessageBoxButton.YesNo);

            if (dialogResult != MessageBoxResult.Yes)
                return;

            _steamData.Accounts.Remove((SteamAccount) listBoxAccounts.SelectedItem);
            UpdateListBoxView();
        }

        private void UpdateListBoxView()
        {
            listBoxAccounts.Items.Refresh();
            SaveSteamData();
            if (_steamData.Accounts.Any())
                buttonEditAccount.IsEnabled = true;
        }
    }
}