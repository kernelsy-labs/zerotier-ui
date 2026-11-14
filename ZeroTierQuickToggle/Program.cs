using System.Diagnostics;
using System.Text.Json;

namespace ZeroTierQuickToggle;

internal sealed class NetworkInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Status { get; init; } = "";
    public bool Enabled { get; set; }
}

internal sealed class MainForm : Form
{
    private readonly FlowLayoutPanel list = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(12, 5, 0, 0) };
    private readonly Button refresh = new() { Text = "Refresh", Width = 90, Height = 32 };
    private readonly Button add = new() { Text = "Add ID", Width = 90, Height = 32 };
    private const string Cli = @"C:\Program Files (x86)\ZeroTier\One\zerotier-cli.bat";
    private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZeroTierQuickToggle", "networks.json");
    // IDs already present in the user's ZeroTier tray menu.
    private static readonly Dictionary<string, string> DefaultNetworks = new(StringComparer.OrdinalIgnoreCase)
    {
        ["52b337794fbacf56"] = "RMT_1100",
        ["272f5eae165ca938"] = "4011-Mas Didik",
        ["48d6023c467344bd"] = "Dokosari_RB450Gx4",
        ["565799d8f669b790"] = "1100-Rejosari"
    };

    public MainForm()
    {
        Text = "ZeroTier Quick Toggle";
        Width = 520; Height = 470; MinimumSize = new Size(420, 300);
        StartPosition = FormStartPosition.CenterScreen;
        var header = new Panel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(12, 10, 12, 8) };
        var title = new Label { Text = "ZeroTier networks", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Location = new Point(12, 12) };
        void PositionHeaderButtons()
        {
            refresh.Location = new Point(header.ClientSize.Width - refresh.Width - 12, 9);
            add.Location = new Point(header.ClientSize.Width - refresh.Width - add.Width - 20, 9);
        }
        header.Resize += (_, _) => PositionHeaderButtons();
        PositionHeaderButtons();
        add.Click += (_, _) => AddNetworkManually();
        refresh.Click += async (_, _) => await LoadNetworks();
        header.Controls.Add(title); header.Controls.Add(add); header.Controls.Add(refresh);
        Controls.Add(list); Controls.Add(status); Controls.Add(header);
        Shown += async (_, _) => await LoadNetworks();
    }

    private async Task LoadNetworks()
    {
        refresh.Enabled = false; list.Controls.Clear(); status.Text = "Loading...";
        try
        {
            var output = await RunCli("-j", "listnetworks");
            var data = JsonSerializer.Deserialize<List<JsonElement>>(output) ?? [];
            var saved = LoadSaved();
            foreach (var defaultNetwork in DefaultNetworks)
                if (!saved.ContainsKey(defaultNetwork.Key)) saved[defaultNetwork.Key] = defaultNetwork.Value;
            foreach (var item in data)
            {
                var idValue = item.TryGetProperty("nwid", out var id) ? id.GetString() ?? "" : "";
                var n = new NetworkInfo
                {
                    Id = idValue,
                    Name = item.TryGetProperty("name", out var name) && !string.IsNullOrWhiteSpace(name.GetString()) ? name.GetString()! : "Unnamed network",
                    Status = item.TryGetProperty("status", out var s) ? s.GetString() ?? "" : "",
                    Enabled = true
                };
                saved[n.Id] = n.Name;
                AddOrUpdateNetwork(n, saved);
            }
            foreach (var item in saved)
            {
                if (!data.Any(x => x.TryGetProperty("nwid", out var id) && id.GetString() == item.Key))
                    AddNetwork(new NetworkInfo { Id = item.Key, Name = item.Value, Status = "Disabled", Enabled = false }, saved);
            }
            SaveSaved(saved);
            if (list.Controls.Count == 0)
                status.Text = "No networks found. Click Add ID to add a ZeroTier network.";
            else
                status.Text = $"{list.Controls.Count} network(s). Enable joins; Disable leaves the network.";
        }
        catch (Exception ex) { status.Text = "Error: " + ex.Message; }
        finally { refresh.Enabled = true; }
    }

    private void AddOrUpdateNetwork(NetworkInfo n, Dictionary<string, string> saved)
    {
        AddNetwork(n, saved);
    }

    private void AddNetwork(NetworkInfo n, Dictionary<string, string> saved)
    {
        var card = new Panel { Width = list.ClientSize.Width - 28, Height = 72, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), Anchor = AnchorStyles.Left | AnchorStyles.Right };
        var text = new Label { Text = $"{n.Name}\n{n.Id}\nStatus: {n.Status}", AutoSize = true, Location = new Point(12, 9) };
        var toggle = new Button { Text = n.Enabled ? "Disable" : "Enable", Width = 88, Height = 32, Location = new Point(card.Width - 105, 18), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        toggle.Click += async (_, _) =>
        {
            toggle.Enabled = false;
            try
            {
                if (n.Enabled) await RunCli("leave", n.Id);
                else await RunCli("join", n.Id);
                await LoadNetworks();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ZeroTier error", MessageBoxButtons.OK, MessageBoxIcon.Error); toggle.Enabled = true; }
        };
        card.Controls.Add(text); card.Controls.Add(toggle); list.Controls.Add(card);
    }

    private void AddNetworkManually()
    {
        using var dialog = new Form { Text = "Add ZeroTier Network", Width = 390, Height = 165, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        var label = new Label { Text = "Network ID (16 hexadecimal characters):", AutoSize = true, Location = new Point(15, 15) };
        var input = new TextBox { Width = 340, Location = new Point(15, 42) };
        var ok = new Button { Text = "Add", DialogResult = DialogResult.OK, Location = new Point(190, 78), Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(280, 78), Width = 75 };
        dialog.Controls.AddRange([label, input, ok, cancel]); dialog.AcceptButton = ok; dialog.CancelButton = cancel;
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var id = input.Text.Trim().ToLowerInvariant();
        if (id.Length != 16 || id.Any(c => !Uri.IsHexDigit(c))) { MessageBox.Show("Network ID harus 16 karakter hexadecimal."); return; }
        var saved = LoadSaved(); saved[id] = id; SaveSaved(saved);
        _ = LoadNetworks();
    }

    private static Dictionary<string, string> LoadSaved()
    {
        try
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ConfigPath)) ?? [];
            return new Dictionary<string, string>(result, StringComparer.OrdinalIgnoreCase);
        }
        catch { return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); }
    }

    private static void SaveSaved(Dictionary<string, string> saved)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(saved, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task<string> RunCli(params string[] args)
    {
        if (!File.Exists(Cli)) throw new FileNotFoundException("ZeroTier CLI tidak ditemukan.", Cli);
        var psi = new ProcessStartInfo { FileName = Cli, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var arg in args) psi.ArgumentList.Add(arg);
        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Gagal menjalankan ZeroTier CLI.");
        var stdout = await p.StandardOutput.ReadToEndAsync(); var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : stderr.Trim());
        return stdout;
    }
}

internal static class Program
{
    [STAThread]
    private static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); }
}
