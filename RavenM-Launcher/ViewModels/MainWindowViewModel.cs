using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Steamworks;
using Steamworks.Data;

namespace RavenM_Launcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public bool Loaded => App.steamInitialised;

        private string _lobbyCountText;
        public string LobbyCountText
        {
            get => _lobbyCountText;
            set => this.RaiseAndSetIfChanged(ref _lobbyCountText, value);
        }
        
        private List<object>? _openLobbies;
        public List<object>? Lobbies
        {
            get => _openLobbies;
            set => this.RaiseAndSetIfChanged(ref _openLobbies, value);
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
            { "41a44c4a-f41f-48c2-b29b-9fb53f65f043", "0.4" },
            { "bb3ef199-df63-4e99-a8a1-89a27d9e2fcb", "Dev" }
        };

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
        
        private void SetLobbyCountText(List<object> lobbies)
        {
            LobbyCountText = $"Found {lobbies.Count} {(lobbies.Count == 1 ? "lobby" : "lobbies")}";
        }

        private async Task<Lobby[]> RequestLobbyListAsync()
        {
            return await SteamMatchmaking.LobbyList.RequestAsync();
        }

        private async Task<bool> GetLobbies()
        {
            try
            {
                Lobby[] requestedLobbies = await SteamMatchmaking.LobbyList.RequestAsync();
                
                if (requestedLobbies != null)
                {
                    Debug.WriteLine(requestedLobbies);

                    Lobbies = new List<object>();
                    
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
                            
                            Lobbies.Add(new
                            {
                                HostName = owner.Name,
                                Map = lobby.GetData("customMap") == "" ? "Built-in Map" : lobby.GetData("customMap"),
                                MemberCount = lobby.MemberCount,
                                MaxMembers = lobby.MaxMembers,
                                ModCount = modList != string.Empty ? modList.Split(',').Length : 0,
                                MidjoinEnabled = hotjoin,
                                RavenMVersion = ravenMversion,
                                IndexInLobbies = item.i
                            });
                        }

                    }
                    Debug.WriteLine(Lobbies);
                    SetLobbyCountText(Lobbies);
                }
            }
            catch (Exception e)
            {
                var messageBox =
                    MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error",
                        $"Error when fetching multiplayer lobbies: {e.Message}");
                await messageBox.Show();
            }

            return true;
        }
        
        private async void FindLobbies()
        {
            await GetLobbies();
        }

        public void OnRefreshLobbies()
        {
            ClearLobbies();
            FindLobbies();
        }

        private static void CloseCurrentProcess()
        {
            int currentProcessId = Process.GetCurrentProcess().Id;
            Process currentProcess = Process.GetProcessById(currentProcessId);
            currentProcess.CloseMainWindow();
        }

        private static void StartRavenfield(string args = "")
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $@"steam://run/636480{(args != string.Empty ? $"//{args}" : "" )}",
                UseShellExecute = true
            });
        }

        public async void OnJoinLobby()
        {
            if (Lobbies != null)
            {
                object lobby = Lobbies.ElementAt(LobbySelectedIndex);
                var lobbyIndex = lobby.GetType().GetProperty("IndexInLobbies")?.GetValue(lobby, null).ToString();
                
                if (lobbyIndex != null)
                {
                    Lobby[] requestedLobbies = await SteamMatchmaking.LobbyList.RequestAsync();
                    Lobby lobbyObj = requestedLobbies.ElementAt(Int32.Parse(lobbyIndex));

                    await SteamMatchmaking.JoinLobbyAsync(lobbyObj.Id);
                    StartRavenfield($"-ravenm-lobby {lobbyObj.Id}");
                    CloseCurrentProcess();
                }
            }
        }
        
        public void OnStartRavenfield()
        {
            SteamClient.Shutdown();
            StartRavenfield();
            CloseCurrentProcess();
        }
        
        public MainWindowViewModel()
        {
            FindLobbies();
        }
    }
}