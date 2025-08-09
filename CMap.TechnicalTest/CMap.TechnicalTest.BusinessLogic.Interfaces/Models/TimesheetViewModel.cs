using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CMap.TechnicalTest.BusinessLogic.Interfaces.Models;

public class TimesheetViewModel(
    Guid id,
    string userName,
    string projectName,
    DateTime date,
    decimal hours,
    string? description)
{
    public Guid Id { get; } = id;

    [DisplayName("User")]
    public string UserName { get; } = userName;

    [DisplayName("Project")]
    public string ProjectName { get; } = projectName;

    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime Date { get; } = date;

    public decimal Hours { get; } = hours;

    public string? Description { get; } = description;
}