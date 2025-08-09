using System.ComponentModel;

namespace CMap.TechnicalTest.BusinessLogic.Interfaces.Models;

public class ProjectHoursViewModel(string projectName, decimal totalHours)
{
    [DisplayName("Project")]
    public string ProjectName { get; } = projectName;
    
    [DisplayName("Total Hours")]
    public decimal TotalHours { get; } = totalHours;
}