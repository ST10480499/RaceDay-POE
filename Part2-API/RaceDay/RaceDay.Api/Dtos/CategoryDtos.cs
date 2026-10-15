using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// The details an organiser sends to create or update a category
public class CategoryRequest
{
    [Required]
    [MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int MaxParticipants { get; set; } = 100;
}

// What the API sends back for a category
public class CategoryResponse
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
}