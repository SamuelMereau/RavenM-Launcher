﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using RavenM_Launcher.Models;
using RavenM_Launcher.Views;
using ReactiveUI;
using Steamworks;
using Steamworks.Data;
using Timer = System.Timers.Timer;

namespace RavenM_Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public bool ConnectedToSteam => App.ConnectedToSteam;

        private string _lobbyCountText;
        public string LobbyCountText
        {
            get => _lobbyCountText;
            set => this.RaiseAndSetIfChanged(ref _lobbyCountText, value);
        }
        
        private List<LobbyListItem>? _openLobbies;
        public List<LobbyListItem>? Lobbies
        {
            get => _openLobbies;
            set => this.RaiseAndSetIfChanged(ref _openLobbies, value);
        }

        public bool LobbiesFound
        {
            get
            {
                if (Lobbies != null)
                {
                    return Lobbies.Count > 0;
                }
                return false;
            }
        }

        private int _lobbySelectedIndex;

        /// <summary>
        /// Index in Lobbies list
        /// </summary>
        /// <returns></returns>
        public int LobbySelectedIndex
        {
            get => _lobbySelectedIndex;
            set => this.RaiseAndSetIfChanged(ref _lobbySelectedIndex, value);
        }

        private bool _lobbySelected;

        public bool LobbySelected
        {
            get => _lobbySelected;
            set => this.RaiseAndSetIfChanged(ref _lobbySelected, value);
        }

        public Dictionary<string, string> RavenMVersionBuildIds = new Dictionary<string, string>()
        {
            { "cffd5580-bfbc-438d-9f43-b36f71a9221b", "0.3" },
            { "41a44c4a-f4f1-48c2-b29b-9fb53f65f043", "0.4" },
            { "bb3ef199-df63-4e99-a8a1-89a27d9e2fcb", "Dev" }
        };

        private Timer autoRefresh
        {
            get;
            set;
        }

        // private void autoRefresh_Tick(object sender, EventArgs e)
        // {
        //     OnRefreshLobbies();
        // }
        
        private void ClearLobbies()
        {
            if (Lobbies != null)
            {
                if (Lobbies.Any())
                {
                    Lobbies.Clear();
                }
            }
        }

        public void OnLobbyListSelectionChange(bool isSelected)
        {
            LobbySelected = isSelected;
        }
        
        private void SetLobbyCountText(List<LobbyListItem> lobbies)
        {
            LobbyCountText = $"Found {lobbies.Count} {(lobbies.Count == 1 ? "lobby" : "lobbies")}";
        }

        private async Task<Lobby[]> RequestLobbyListAsync()
        {
            return await SteamMatchmaking.LobbyList.RequestAsync();
        }

        /// <summary>
        /// Refreshes the hostname after some time since RequestUserInfo
        /// may not always return user information immediately.
        /// </summary>
        /// <param name="index">Index in Lobbies list</param>
        private async void RefreshHostname(int index, ulong steamid)
        {
            try
            {
                var lobbyItem = Lobbies.ElementAt(index);
                await Task.Delay(TimeSpan.FromSeconds(1));
                var steamuser = new Friend(steamid);
                lobbyItem.HostName = steamuser.Name;
            }
            catch (Exception e)
            {
                var messageBox =
                    MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                        {
                            ButtonDefinitions = ButtonEnum.Ok,
                            ContentTitle = "Error",
                            ContentMessage = $"Error when refreshing lobbies:\n{e.Message}",
                            Icon = Icon.Error,
                        }
                    );
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    await messageBox.ShowDialog(desktop.MainWindow);
                }
            }
        }
        
        private async Task<bool> GetLobbies()
        {
            try
            {
                Lobby[] requestedLobbies = await SteamMatchmaking.LobbyList.RequestAsync();
                
                if (requestedLobbies != null)
                {
                    Lobbies = new List<LobbyListItem>();
                    
                    foreach (var item in requestedLobbies.Select((value, i) => new { i, value }))
                    {
                        Lobby lobby = item.value;
                        bool hidden = lobby.GetData("hidden") != string.Empty;
                        
                        if (!hidden)
                        {
                            string? modList = lobby.GetData("mods");
                            bool gettingOwner = SteamFriends.RequestUserInformation(ulong.Parse(lobby.GetData("owner")), true);
                            Friend owner = new Friend(ulong.Parse(lobby.GetData("owner")));
                            string hotjoin = lobby.GetData("hotjoin") != string.Empty ? "Yes" : "No";
                            string ravenMversion;
                            
                            if (!RavenMVersionBuildIds.TryGetValue(lobby.GetData("build_id"), out ravenMversion))
                            {
                                ravenMversion = "N/A";
                            }

                            Lobbies.Add(new LobbyListItem(
                                lobbyId: lobby.Id,
                                hostName: owner.Name,
                                map: lobby.GetData("customMap") == "" ? "Built-in Map" : lobby.GetData("customMap"),
                                memberCount: lobby.MemberCount,
                                maxMembers: lobby.MaxMembers,
                                modCount: modList != string.Empty ? modList.Split(',').Length : 0,
                                midjoinEnabled: hotjoin,
                                ravenMVersion: ravenMversion,
                                selectedIndexInLobbies: item.i
                            ));

                            if (gettingOwner)
                            {
                                RefreshHostname(item.i, ulong.Parse(lobby.GetData("owner")));
                            }
                        }

                    }
                    SetLobbyCountText(Lobbies);
                }
            }
            catch (Exception e)
            {
                var messageBox =
                    MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
                        {
                            ButtonDefinitions = ButtonEnum.Ok,
                            ContentTitle = "Error",
                            ContentMessage = $"Error when fetching multiplayer lobbies:\n{e.Message}",
                            Icon = Icon.Error,
                        }
                    );
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    await messageBox.ShowDialog(desktop.MainWindow);
                }
            }

            return true;
        }
        
        private async void FindLobbies()
        {
            await GetLobbies();
        }

        public void OnRefreshLobbies()
        {
            if (ConnectedToSteam)
            {
                ClearLobbies();
                FindLobbies();
            }
        }

        private static void CloseCurrentProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            if (AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem !=
                OperatingSystemType.WinNT)
            {
                Task.Delay(TimeSpan.FromSeconds(0.25));
                currentProcess.Kill();
            }
            else
            {
                // CloseMainWindow is only supported on Windows as of .NET6
                currentProcess.CloseMainWindow();
            }
        }

        private static void StartRavenfield(string args = "")
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $@"steam://run/636480{(args != string.Empty ? $"//{args}" : "" )}",
                UseShellExecute = true,
            });
            CloseCurrentProcess();
        }

        public void OnJoinLobby()
        {
            if (Lobbies != null)
            {
                SteamClient.Shutdown();
                LobbyListItem lobby = Lobbies.ElementAt(LobbySelectedIndex);
                StartRavenfield($"-ravenm-lobby={lobby.LobbyId} -nocontentmods");
            }
        }
        
        public void OnStartRavenfield()
        {
            SteamClient.Shutdown();
            StartRavenfield();
        }
        
        public MainWindowViewModel()
        {
            if (ConnectedToSteam)
            {
                FindLobbies();
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (sender, e) =>
                {
                    BackgroundWorker worker = (BackgroundWorker)sender;
                    while (!worker.CancellationPending)
                    {
                        OnRefreshLobbies();
                        Thread.Sleep(30000); // 30 seconds
                    }
                };
                bw.RunWorkerCompleted += (sender, e) => { };
                bw.RunWorkerAsync();
            }
        }
    }
}