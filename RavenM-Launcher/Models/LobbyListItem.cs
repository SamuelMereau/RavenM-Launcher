namespace RavenM_Launcher.Models;

public class LobbyListItem
{
    private string _hostName;
    public string HostName
    {
        get => _hostName;
        set => _hostName = value;
    }
    
    private string _map;
    public string Map
    {
        get => _map;
        set => _map = value;
    }

    private int _memberCount;
    public int MemberCount
    {
        get => _memberCount;
        set => _memberCount = value;
    }
    
    private int _maxMembers;
    public int MaxMembers
    {
        get => _maxMembers;
        set => _maxMembers = value;
    }
    
    private int _modCount;
    public int ModCount
    {
        get => _modCount;
        set => _modCount = value;
    }
    
    private string _midjoinEnabled;
    public string MidjoinEnabled
    {
        get => _midjoinEnabled;
        set => _midjoinEnabled = value;
    }
    
    private string _ravenMVersion;
    public string RavenMVersion
    {
        get => _ravenMVersion;
        set => _ravenMVersion = value;
    }
    
    private int _selectedIndexInLobbies;
    public int SelectedIndexInLobbies
    {
        get => _selectedIndexInLobbies;
        set => _selectedIndexInLobbies = value;
    }

    public LobbyListItem (
        string hostName, 
        string map, 
        int memberCount, 
        int maxMembers, 
        int modCount, 
        string midjoinEnabled, 
        string ravenMVersion, 
        int selectedIndexInLobbies)
    {
        _hostName = hostName;
        _map = map;
        _memberCount = memberCount;
        _maxMembers = maxMembers;
        _modCount = modCount;
        _midjoinEnabled = midjoinEnabled;
        _ravenMVersion = ravenMVersion;
        _selectedIndexInLobbies = selectedIndexInLobbies;
    }
}