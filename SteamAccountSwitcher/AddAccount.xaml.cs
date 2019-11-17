﻿using System;
using System.ComponentModel;
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

        public SteamAccount Account { get; private set; }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Account.Type = (AccountType) comboBoxType.SelectedItem;
                Account.Name = textBoxProfilename.Text;
                Account.Username = textBoxUsername.Text;
                Account.Password = textBoxPassword.Password;
            }
            catch
            {
                Account = null;
            }

            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Account != null)
                if (Account.Username == "" || Account.Username == null)
                    Account = null;
        }
    }
}