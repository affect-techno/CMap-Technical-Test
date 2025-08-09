using System.ComponentModel.DataAnnotations;

namespace CMap.TechnicalTest.Models;

/// <summary>
/// Project model
/// </summary>
public class Project
{
    /// <summary>
    /// ID of the Project
    /// </summary>
    [Required]
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Name of the Project
    /// </summary>
    [Required]
    public string? Name { get; set; }
}