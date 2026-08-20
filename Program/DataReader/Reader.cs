using System.IO.Ports;
using System;
using System.Management;
using System.Text.RegularExpressions;

namespace DataReader
{
    class PortDetector
    {
        public string? FindPort()
        {
            using ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(
                    "SELECT Name, PNPDeviceID FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

            List<(string Port, string Name, string PnpId)> devices = new();

            foreach (ManagementObject device in searcher.Get())
            {
                string? name = device["Name"]?.ToString();
                string? pnpId = device["PNPDeviceID"]?.ToString();

                if (name == null || pnpId == null)
                    continue;

                Match match = Regex.Match(name, @"\((COM\d+)\)");

                if (!match.Success)
                    continue;

                string port = match.Groups[1].Value;

                devices.Add((port, name, pnpId));
            }

            if (devices.Count == 0)
            {
                File.AppendAllText("program.log", "[Error] nessun dispositivo trovato\n");
                return null;
            }

            File.AppendAllText("program.log", "[Info] Dispositivi trovati:\n");

            foreach (var device in devices)
            {
                File.AppendAllText("program.log", $"[Info] Porta: {device.Port}\n");
                File.AppendAllText("program.log", $"[Info] Nome: {device.Name}\n");
                File.AppendAllText("program.log", $"[Info] USB: {device.PnpId}\n");
            }


            var esp32Devices = devices.FindAll(device =>
                device.PnpId.Contains("VID_303A", StringComparison.OrdinalIgnoreCase) ||
                device.PnpId.Contains("VID_2341", StringComparison.OrdinalIgnoreCase));

            if (esp32Devices.Count == 0)
            {
                File.AppendAllText("program.log", "[Error] Nessun Microcontrollore compatibile identificato.\n");
                return null;
            }

            if (esp32Devices.Count > 1)
            {
                File.AppendAllText("program.log", "[Info] Piu porte trovate\n");

                foreach (var device in esp32Devices)
                {
                    File.AppendAllText("program.log", $"[Info] - {device.Port}: {device.Name}\n");
                }

                return null;
            }

            string selectedPort = esp32Devices[0].Port;

            File.AppendAllText("program.log", $"[Info] Microcontrollore identificato sulla porta {selectedPort}\n");

            return selectedPort;
        }
    }

    class ReadData
    {
        private SerialPort? serial;

        public void CheckPort()
        {
            PortDetector detector = new PortDetector();

            string? port = detector.FindPort();

            if (port == null)
            {
                throw new Exception("Porta non trovata connetti il dispositivo");
            }

            StartReading(port);
        }

        public void StartReading(string port)
{
    serial = new SerialPort(port, 9600)
    {
        DtrEnable = true,
        RtsEnable = true,
        ReadTimeout = 2000,
        NewLine = "\r"
    };

    try
    {
        serial.Open();
        File.AppendAllText("program.log", $"[Info]: Connesso a {port}");

        // Attendere il reset provocato dall'apertura della COM
        Thread.Sleep(1500);

        while (serial.IsOpen)
        {
            Lexer lexer = new Lexer();
            try
            {
                string line = serial.ReadLine();
                lexer.Set(line);
                lexer.StartLexing();
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{ex}");
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"{ex}");
    }
    finally
    {
        if (serial?.IsOpen == true)
            serial.Close();
    }
}
    }
}