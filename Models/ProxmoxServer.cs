using System.ComponentModel.DataAnnotations;

namespace ProxmoxDashboard.Models;

public class ProxmoxServer
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string BaseURL { get; set; } = "";

    [Required]
    public string ApiToken { get; set; } = "";

    public DateTime? LastSuccessfulFetch { get; set; }

    public List<CachedVM> CachedVMs { get; set; } = new();
}
