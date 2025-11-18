namespace ChildPsychologyAI.Models.Entities;

public class Child : EntityBase
{
    public string ParentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentDate { get; set; }
}
