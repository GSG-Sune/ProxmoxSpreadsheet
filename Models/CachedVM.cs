using System.ComponentModel.DataAnnotations.Schema;

namespace ProxmoxDashboard.Models;

public class CachedVM
{
    public int Id { get; set; }

    public int ProxmoxServerId { get; set; }

    [ForeignKey("ProxmoxServerId")]
    public ProxmoxServer ProxmoxServer { get; set; } = null!;

    public int VMID { get; set; }
    public string Node { get; set; } = "";
    public string Name { get; set; } = "";
    public string Ip { get; set; } = "";
    public int RAM { get; set; }
    public string CPU { get; set; } = "";
    public int Cores { get; set; }
    public string Status { get; set; } = "";
}
