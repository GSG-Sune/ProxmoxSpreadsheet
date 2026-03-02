namespace ProxmoxDashboard.Models
{
    public class VM
    {
        public int VMID { get; set; }
        public string Node { get; set; }
        public string Name { get; set; }
        public string VM_Name { get; set; }
        public string Ip { get; set; }
        public int RAM { get; set; }
        public string CPU { get; set; }
        public int Cores { get; set; }
        public string Status { get; set; }
    }
}