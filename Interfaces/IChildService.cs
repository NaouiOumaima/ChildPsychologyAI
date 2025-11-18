using ChildPsychologyAI.Models.Entities;

namespace ChildPsychologyAI.Interfaces;

public interface IChildService
{
    Task<Child> CreateChildAsync(Child child);
    Task<Child?> GetChildByIdAsync(string childId);
    Task<List<Child>> GetChildrenByParentIdAsync(string parentId);
    Task<Child> UpdateChildAsync(Child child);
    Task<bool> DeleteChildAsync(string childId);
    Task<bool> GiveConsentAsync(string childId);
}