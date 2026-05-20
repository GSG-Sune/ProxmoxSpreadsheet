using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProxmoxDashboard.Data;
using ProxmoxDashboard.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProxmoxDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _db;

    public List<DatacenterGroup> Datacenters { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, ApplicationDbContext db)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _db = db;
    }

    public async Task OnGetAsync()
    {
        var proxmoxServers = await _db.ProxmoxServers.ToListAsync();

        foreach (var server in proxmoxServers)
        {
            var liveVMs = await GetVMsFromServerAsync(server.BaseURL, server.ApiToken);
            bool isStale;

            if (liveVMs != null)
            {
                // Success: update cache
                var oldCached = await _db.CachedVMs
                    .Where(c => c.ProxmoxServerId == server.Id)
                    .ToListAsync();
                _db.CachedVMs.RemoveRange(oldCached);

                foreach (var vm in liveVMs)
                {
                    _db.CachedVMs.Add(new CachedVM
                    {
                        ProxmoxServerId = server.Id,
                        VMID = vm.VMID,
                        Node = vm.Node,
                        Name = vm.Name,
                        Ip = vm.Ip,
                        RAM = vm.RAM,
                        CPU = vm.CPU,
                        Cores = vm.Cores,
                        Status = vm.Status
                    });
                }

                server.LastSuccessfulFetch = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                isStale = false;
            }
            else
            {
                // Failure: load from cache
                liveVMs = (await _db.CachedVMs
                    .Where(c => c.ProxmoxServerId == server.Id)
                    .ToListAsync())
                    .Select(c => new VM
                    {
                        VMID = c.VMID,
                        Node = c.Node,
                        Name = c.Name,
                        Ip = c.Ip,
                        RAM = c.RAM,
                        CPU = c.CPU,
                        Cores = c.Cores,
                        Status = c.Status
                    })
                    .ToList();

                isStale = true;
            }

            var nodeGroups = liveVMs
                .GroupBy(vm => vm.Node)
                .OrderBy(g => g.Key)
                .Select(g => new NodeGroup
                {
                    NodeName = g.Key,
                    VMs = g.OrderBy(vm => vm.VMID).ToList()
                })
                .ToList();

            Datacenters.Add(new DatacenterGroup
            {
                DatacenterName = server.Name,
                ServerId = server.Id,
                LastSuccessfulFetch = server.LastSuccessfulFetch,
                IsStale = isStale,
                Nodes = nodeGroups
            });
        }
    }

    // Returns null on failure (vs empty list = server up but no VMs)
    private async Task<List<VM>?> GetVMsFromServerAsync(string BaseURL, string ApiToken)
    {
        var client = _httpClientFactory.CreateClient("ProxmoxClient");
        client.BaseAddress = new Uri(BaseURL);

        var parts = ApiToken.Split("=", 2);
        var tokenId = parts[0];
        var secret = parts[1];

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("PVEAPIToken", $"{tokenId}={secret}");

        _logger.LogInformation("Attempting to get cluster resources from {BaseURL}...", BaseURL);

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync("cluster/resources");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Proxmox server at {BaseURL}", BaseURL);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout connecting to Proxmox server at {BaseURL}", BaseURL);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get cluster resources: {StatusCode}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Cluster Resources Response: {json}", json);

        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        var vmList = new List<VM>();

        foreach (var resource in data.EnumerateArray())
        {
            if (!resource.TryGetProperty("type", out var typeElement))
                continue;

            string resourceType = typeElement.GetString() ?? "";

            if (resourceType != "qemu")
            {
                _logger.LogInformation("Skipping resource of type: {type}", resourceType);
                continue;
            }

            var vmid = resource.GetProperty("vmid").GetInt32();
            var node = resource.GetProperty("node").GetString() ?? "";
            var name = resource.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "" : "";
            var status = resource.GetProperty("status").GetString() ?? "";

            _logger.LogInformation("Processing VMID: {vmid} on Node: {node}", vmid, node);

            var configResponse = await client.GetAsync($"nodes/{node}/qemu/{vmid}/config");

            if (configResponse.IsSuccessStatusCode)
            {
                var configJson = await configResponse.Content.ReadAsStringAsync();
                using var configDoc = JsonDocument.Parse(configJson);
                var configData = configDoc.RootElement.GetProperty("data");

                int cores = 0;
                if (configData.TryGetProperty("cores", out var coresElement))
                {
                    if (coresElement.ValueKind == JsonValueKind.Number)
                    {
                        cores = coresElement.GetInt32();
                    }
                    else if (coresElement.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(coresElement.GetString(), out cores);
                    }
                }

                int memory = 0;
                if (configData.TryGetProperty("memory", out var memElement))
                {
                    if (memElement.ValueKind == JsonValueKind.Number)
                    {
                        memory = memElement.GetInt32();
                    }
                    else if (memElement.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(memElement.GetString(), out memory);
                    }
                }
                var cpu = configData.TryGetProperty("cpu", out var cpuElement) ? cpuElement.GetString() ?? "" : "";

                string ip = "";
                try
                {
                    var agentResponse = await client.GetAsync($"nodes/{node}/qemu/{vmid}/agent/network-get-interfaces");
                    _logger.LogInformation("Guest agent response for VMID {vmid}: {statusCode}", vmid, agentResponse.StatusCode);
                    if (agentResponse.IsSuccessStatusCode)
                    {
                        var agentJson = await agentResponse.Content.ReadAsStringAsync();
                        using var agentDoc = JsonDocument.Parse(agentJson);
                        var interfaces = agentDoc.RootElement.GetProperty("data").GetProperty("result");

                        foreach (var iface in interfaces.EnumerateArray())
                        {
                            if (iface.TryGetProperty("ip-addresses", out var addresses))
                            {
                                foreach (var addr in addresses.EnumerateArray())
                                {
                                    if (addr.GetProperty("ip-address-type").GetString() == "ipv4" &&
                                        !addr.GetProperty("ip-address").GetString()!.StartsWith("127."))
                                    {
                                        ip = addr.GetProperty("ip-address").GetString() ?? "";
                                        break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ip))
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get IP from QEMU Guest Agent for VMID: {vmid} on node {node}", vmid, node);
                }

                vmList.Add(new VM
                {
                    VMID = vmid,
                    Node = node,
                    Name = name,
                    Status = status,
                    Ip = ip,
                    RAM = memory / 1024,
                    CPU = cpu,
                    Cores = cores
                });
                _logger.LogInformation("Successfully added VMID: {vmid}", vmid);
            }
            else
            {
                _logger.LogWarning("Failed to get config for VMID: {vmid}. Status: {status}", vmid, configResponse.StatusCode);
            }
        }

        _logger.LogInformation("Finished processing. Total VMs added: {count}", vmList.Count);
        return vmList;
    }
}

public class DatacenterGroup
{
    public string DatacenterName { get; set; } = "";
    public int ServerId { get; set; }
    public DateTime? LastSuccessfulFetch { get; set; }
    public bool IsStale { get; set; }
    public List<NodeGroup> Nodes { get; set; } = new();
}

public class NodeGroup
{
    public string NodeName { get; set; } = "";
    public List<VM> VMs { get; set; } = new();
}
