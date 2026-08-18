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
                Console.WriteLine("Nessun dispositivo seriale trovato.");
                return null;
            }

            Console.WriteLine("Dispositivi seriali trovati:");

            foreach (var device in devices)
            {
                Console.WriteLine();
                Console.WriteLine($"Porta: {device.Port}");
                Console.WriteLine($"Nome:  {device.Name}");
                Console.WriteLine($"USB:   {device.PnpId}");
            }

            /*
             * Qui NON scegliamo ports[0].
             *
             * Prima cerchiamo un dispositivo ESP32.
             */

            var esp32Devices = devices.FindAll(device =>
                device.PnpId.Contains("VID_303A", StringComparison.OrdinalIgnoreCase) ||
                device.PnpId.Contains("VID_2341", StringComparison.OrdinalIgnoreCase));

            if (esp32Devices.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Nessun ESP32/Arduino compatibile identificato.");
                return null;
            }

            if (esp32Devices.Count > 1)
            {
                Console.WriteLine();
                Console.WriteLine("ATTENZIONE: sono state trovate più porte compatibili.");
                Console.WriteLine("Non verrà selezionata una porta casualmente.");

                foreach (var device in esp32Devices)
                {
                    Console.WriteLine($"- {device.Port}: {device.Name}");
                }

                return null;
            }

            string selectedPort = esp32Devices[0].Port;

            Console.WriteLine();
            Console.WriteLine($"ESP32 identificato sulla porta {selectedPort}");

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
                throw new Exception("Port not found. Connect the device.");
            }

            StartReading(port);
        }

        public void StartReading(string port)
        {
            serial = new SerialPort(port, 9600);

            try
            {
                serial.DtrEnable = true;   
                serial.RtsEnable = true;
                serial.Open();

                Console.WriteLine($"Connesso a {port}");
                string line = "";
                while (true)
                {
                    line = serial.ReadLine();
                    Console.WriteLine($"Ricevuto: {line}");      
                   // Lexer b = new Lexer(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
            finally
            {
                if (serial.IsOpen)
                {
                    serial.Close();
                }
            }
        }
    }
}