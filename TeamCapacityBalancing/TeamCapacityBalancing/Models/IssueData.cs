namespace TeamCapacityBalancing.Models;

public class IssueData
{
    public enum IssueType
    {
        Epic,
        Story,
        Task,
    }

    public string? BusinessCase { get; set; }

    public int? EpicID { get; set; }
    public int Id { get; set; }
    public string? Asignee { get; set; }
    public float Remaining { get; set; }
    public string? Name { get; set; }
    public IssueType Type { get; set; }
 

    public IssueData(int id, string name, string businessCase = "DoesntHave")
    {
        Id = id;
        Name = name;
        BusinessCase = businessCase;
    }

    public IssueData(string name, float remaining, string release, string sprint, bool status, IssueType type)   //After sebi does what he does this can die
    {
        Name = name;
        Remaining = remaining;
        Type = type;
    }

    public IssueData(int id, string summary, string assignee, float remaining, int epicId = -1)
    {
        Id = id;
        EpicID = epicId;
        Name = summary;
        Asignee = assignee;
        Remaining = remaining;
    }

    public IssueData()
    {

    }

}
