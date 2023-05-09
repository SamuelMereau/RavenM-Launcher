using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using RavenM_Launcher.ViewModels;
using Steamworks;
using Steamworks.Data;

namespace RavenM_Launcher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLobbiesList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid lobbylist = (DataGrid)sender;
                var viewModel = DataContext as MainWindowViewModel;
                
                // Set lobby list index
                viewModel.LobbySelectedIndex = lobbylist.SelectedIndex;
                
                viewModel.OnLobbyListSelectionChange(lobbylist.SelectedItems.Count > 0);
            }
        }
    }
}