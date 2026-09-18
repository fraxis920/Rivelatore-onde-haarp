using System.Diagnostics;
using System.IO.Compression;
using System.Management;
using System.Net.Http;

namespace EspDriverInstaller;

public class MainForm : Form
{
    // Chip USB-UART tipicamente montati sulle ESP32 DevKit vendute su Amazon
    private const string SilabsVid = "VID_10C4"; // CP2102 / CP2102N / CP210x - Silicon Labs
    private const string WchVid = "VID_1A86";     // CH340 / CH341 / CH9102 - WCH

    private const string Cp210xDriverUrl =
        "https://www.silabs.com/documents/public/software/CP210x_Universal_Windows_Driver.zip";

    private const string Ch340DownloadPage =
        "https://www.wch.cn/download/CH341SER_ZIP.html";

    private readonly Label _statusLabel = new();
    private readonly Button _refreshButton = new();
    private readonly Button _installCp210xButton = new();
    private readonly Button _openCh340PageButton = new();
    private readonly TextBox _logBox = new();

    public MainForm()
    {
        Text = "ESP32 DevKit - Driver Installer";
        Width = 640;
        Height = 480;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "Installer driver per ESP32 DevKit (chip USB-seriale CP210x / CH340)",
            Left = 15,
            Top = 15,
            Width = 600,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        _statusLabel.Left = 15;
        _statusLabel.Top = 45;
        _statusLabel.Width = 600;
        _statusLabel.Height = 40;
        _statusLabel.Text = "Rilevamento dispositivo in corso...";

        _refreshButton.Text = "Aggiorna rilevamento";
        _refreshButton.Left = 15;
        _refreshButton.Top = 90;
        _refreshButton.Width = 160;
        _refreshButton.Click += (_, _) => DetectDevice();

        _installCp210xButton.Text = "Installa driver CP210x (Silicon Labs)";
        _installCp210xButton.Left = 15;
        _installCp210xButton.Top = 130;
        _installCp210xButton.Width = 280;
        _installCp210xButton.Height = 32;
        _installCp210xButton.Click += async (_, _) => await InstallCp210xAsync();

        _openCh340PageButton.Text = "Apri pagina driver CH340 (WCH)";
        _openCh340PageButton.Left = 305;
        _openCh340PageButton.Top = 130;
        _openCh340PageButton.Width = 280;
        _openCh340PageButton.Height = 32;
        _openCh340PageButton.Click += (_, _) => OpenCh340Page();

        _logBox.Left = 15;
        _logBox.Top = 175;
        _logBox.Width = 595;
        _logBox.Height = 260;
        _logBox.Multiline = true;
        _logBox.ScrollBars = ScrollBars.Vertical;
        _logBox.ReadOnly = true;
        _logBox.Font = new Font(FontFamily.GenericMonospace, 9);

        Controls.Add(title);
        Controls.Add(_statusLabel);
        Controls.Add(_refreshButton);
        Controls.Add(_installCp210xButton);
        Controls.Add(_openCh340PageButton);
        Controls.Add(_logBox);

        Load += (_, _) => DetectDevice();
    }

    private void Log(string message)
    {
        if (_logBox.InvokeRequired)
        {
            _logBox.Invoke(() => Log(message));
            return;
        }

        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    /// <summary>
    /// Interroga WMI per capire quale chip USB-UART è collegato al momento,
    /// così l'utente non deve indovinare quale driver serve.
    /// </summary>
    private void DetectDevice()
    {
        try
        {
            bool foundSilabs = false;
            bool foundWch = false;
            string? detectedName = null;

            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DeviceID FROM Win32_PnPEntity");

            foreach (ManagementObject device in searcher.Get())
            {
                var deviceId = device["DeviceID"]?.ToString() ?? string.Empty;

                if (deviceId.Contains(SilabsVid, StringComparison.OrdinalIgnoreCase))
                {
                    foundSilabs = true;
                    detectedName = device["Name"]?.ToString();
                }
                else if (deviceId.Contains(WchVid, StringComparison.OrdinalIgnoreCase))
                {
                    foundWch = true;
                    detectedName = device["Name"]?.ToString();
                }
            }

            if (foundSilabs)
            {
                _statusLabel.Text = $"Rilevato chip Silicon Labs (CP210x): {detectedName}\n" +
                                     "Usa il pulsante \"Installa driver CP210x\".";
            }
            else if (foundWch)
            {
                _statusLabel.Text = $"Rilevato chip WCH (CH340/CH341/CH9102): {detectedName}\n" +
                                     "Usa il pulsante \"Apri pagina driver CH340\".";
            }
            else
            {
                _statusLabel.Text = "Nessun chip CP210x o CH340 rilevato al momento.\n" +
                                     "Collega la scheda ESP32 via USB e premi \"Aggiorna rilevamento\", " +
                                     "oppure installa entrambi i driver per sicurezza.";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Impossibile interrogare i dispositivi USB (permessi WMI?).";
            Log($"Errore rilevamento dispositivo: {ex.Message}");
        }
    }

    private async Task InstallCp210xAsync()
    {
        _installCp210xButton.Enabled = false;
        var tempDir = Path.Combine(Path.GetTempPath(), "cp210x_driver_" + Guid.NewGuid().ToString("N"));
        var zipPath = Path.Combine(Path.GetTempPath(), "CP210x_Universal_Windows_Driver.zip");

        try
        {
            Directory.CreateDirectory(tempDir);

            Log("Download driver CP210x da Silicon Labs...");
            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromMinutes(3);
                var bytes = await http.GetByteArrayAsync(Cp210xDriverUrl);
                await File.WriteAllBytesAsync(zipPath, bytes);
            }
            Log("Download completato. Estrazione archivio...");

            ZipFile.ExtractToDirectory(zipPath, tempDir, overwriteFiles: true);

            var infPath = Directory.GetFiles(tempDir, "silabser.inf", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (infPath is null)
            {
                Log("ERRORE: file silabser.inf non trovato nell'archivio scaricato.");
                return;
            }

            Log($"Installazione driver tramite pnputil: {infPath}");
            var result = RunElevatedProcess("pnputil.exe", $"/add-driver \"{infPath}\" /install");
            Log(result);

            MessageBox.Show(
                "Installazione completata. Scollega e ricollega la scheda ESP32 per applicare il driver.",
                "Driver CP210x", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DetectDevice();
        }
        catch (Exception ex)
        {
            Log($"ERRORE durante l'installazione: {ex.Message}");
            MessageBox.Show(
                $"Installazione automatica non riuscita:\n{ex.Message}\n\n" +
                "Puoi scaricare manualmente il driver da:\n" + Cp210xDriverUrl,
                "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            try { if (File.Exists(zipPath)) File.Delete(zipPath); } catch { /* best effort */ }
            try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true); } catch { /* best effort */ }
            _installCp210xButton.Enabled = true;
        }
    }

    /// <summary>
    /// WCH non pubblica un URL diretto stabile per lo zip del driver CH340 (la pagina
    /// richiede una navigazione manuale). Per evitare di puntare a mirror non ufficiali,
    /// apriamo direttamente la pagina ufficiale del produttore.
    /// </summary>
    private void OpenCh340Page()
    {
        try
        {
            Process.Start(new ProcessStartInfo(Ch340DownloadPage) { UseShellExecute = true });
            Log("Pagina driver CH340 (WCH) aperta nel browser predefinito.");
            Log("Dopo il download: estrai lo zip, poi tasto destro su CH341SER.INF > Installa " +
                "(oppure esegui SETUP.EXE incluso nell'archivio).");
        }
        catch (Exception ex)
        {
            Log($"Impossibile aprire il browser: {ex.Message}");
            MessageBox.Show(
                $"Apri manualmente questa pagina nel browser:\n{Ch340DownloadPage}",
                "Driver CH340", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private static string RunElevatedProcess(string fileName, string arguments)
    {
        // L'app è già elevata (vedi app.manifest), quindi pnputil eredita i privilegi admin.
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Impossibile avviare {fileName}");

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode == 0
            ? $"OK (exit code 0):\n{output}"
            : $"ERRORE (exit code {process.ExitCode}):\n{output}\n{error}";
    }
}
