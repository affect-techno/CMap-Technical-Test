using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CMap.TechnicalTest.Models;

/// <summary>
/// Timesheet Entry model
/// </summary>
public class TimesheetEntry
{
    /// <summary>
    /// ID of the Timesheet Entry
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The ID of the User this Timesheet Entry is for
    /// </summary>
    [Required, DisplayName("User")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The ID of the Project this Timesheet Entry is for
    /// </summary>
    [Required, DisplayName("Project")]
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// The Date this Timesheet Entry is for
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// The number of hours worked under this Timesheet Entry
    /// </summary>
    [Range(0, 24), DisplayName("Hours Worked")]
    public decimal Hours { get; set; }
    
    /// <summary>
    /// The *optional* description of this Timesheet Entry
    /// </summary>
    public string? Description { get; set; }
}