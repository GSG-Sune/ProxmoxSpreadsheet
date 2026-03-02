using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProxmoxDashboard.Data;
using ProxmoxDashboard.Models;

namespace ProxmoxDashboard.Pages;

public class ServersModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ServersModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ProxmoxServer> Servers { get; set; } = new();

    [BindProperty]
    public ProxmoxServer NewServer { get; set; } = new();

    [BindProperty]
    public ProxmoxServer EditServer { get; set; } = new();

    public async Task OnGetAsync()
    {
        Servers = await _db.ProxmoxServers.ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewServer.Name) ||
            string.IsNullOrWhiteSpace(NewServer.BaseURL) ||
            string.IsNullOrWhiteSpace(NewServer.ApiToken))
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            Servers = await _db.ProxmoxServers.ToListAsync();
            return Page();
        }

        _db.ProxmoxServers.Add(new ProxmoxServer
        {
            Name = NewServer.Name,
            BaseURL = NewServer.BaseURL,
            ApiToken = NewServer.ApiToken
        });
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var server = await _db.ProxmoxServers.FindAsync(id);
        if (server != null)
        {
            _db.ProxmoxServers.Remove(server);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var server = await _db.ProxmoxServers.FindAsync(EditServer.Id);
        if (server == null)
        {
            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(EditServer.Name) ||
            string.IsNullOrWhiteSpace(EditServer.BaseURL) ||
            string.IsNullOrWhiteSpace(EditServer.ApiToken))
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            Servers = await _db.ProxmoxServers.ToListAsync();
            return Page();
        }

        server.Name = EditServer.Name;
        server.BaseURL = EditServer.BaseURL;
        server.ApiToken = EditServer.ApiToken;
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}
