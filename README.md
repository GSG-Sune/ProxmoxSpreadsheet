# Proxmox Dashboard

A lightweight web dashboard for monitoring virtual machines across multiple Proxmox VE servers. Built with ASP.NET Core Razor Pages.

## Features

- **Multi-server support** — monitor VMs across multiple Proxmox clusters from a single dashboard
- **Live data with cache fallback** — fetches live VM data from the Proxmox API, falls back to cached data when a server is unreachable
- **VM overview** — displays status, VMID, name, IP address, RAM, CPU type, and core count for each VM
- **Grouped by datacenter and node** — VMs are organized in collapsible accordions by server and node
- **Server management UI** — add, edit, and delete Proxmox servers from the web interface

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- One or more Proxmox VE servers with API token access

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/ProxmoxDashboard.git
   cd ProxmoxDashboard
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open `http://localhost:5100/ProxmoxDashboard` in your browser.

4. Navigate to **Servers** and add your Proxmox server(s) with their API tokens.

## Proxmox API Token

To create an API token in Proxmox:

1. Go to **Datacenter → Permissions → API Tokens**
2. Create a token for a user (e.g., `apiuser@pve`)
3. Note the token ID and secret — the format used by this app is:
   ```
   user@realm!tokenname=token-secret-uuid
   ```

## Configuration

The app uses SQLite by default (`app.db`). To use a different database, set the `DefaultConnection` connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  }
}
```

The default listen address is `http://0.0.0.0:5100`, configurable in `appsettings.json` under the `Kestrel` section.

## Tech Stack

- ASP.NET Core 9 (Razor Pages)
- Entity Framework Core with SQLite
- Bootstrap 5
