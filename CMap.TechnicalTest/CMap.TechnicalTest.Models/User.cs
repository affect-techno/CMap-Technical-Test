using System.ComponentModel.DataAnnotations;

namespace CMap.TechnicalTest.Models;

/// <summary>
/// User model
/// </summary>
public class User
{
    /// <summary>
    /// ID of the User
    /// </summary>
    [Required]
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Name of the User
    /// </summary>
    [Required]
    public string? Name { get; set; }
}