namespace TeamCapacityBalancing.Models;

public class User
{
    public string Username { get; set; }

    public string DisplayName { get; set; }

    public bool HasTeam { get; set; }

    public int Id { get; set; }

    public string UserInComboBox { get; set; }

    public Wrapper<int> HoursPerDay { get; set; }

    public User()
    {
        Username = string.Empty;
        DisplayName = string.Empty;
        HasTeam = false;
        Id = 0;
        HoursPerDay = new Wrapper<int>() { Value = 40 };
    }
    public User(string username, string displayName = "", int id = 0)
    {
        Username = username;
        HasTeam = false;
        Id = id;
        DisplayName = displayName;
        HoursPerDay = new Wrapper<int>() { Value = 40 };
        UserInComboBox= DisplayName + " (" + Username+")";
    }
}
