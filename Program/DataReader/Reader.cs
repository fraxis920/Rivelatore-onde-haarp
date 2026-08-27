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
                DebugLogger.Error("PortDetector", "No device found");
                return null;
            }

            DebugLogger.Info("PortDetector", "Devices found:");

            foreach (var device in devices)
            {
                DebugLogger.Info("PortDetector", $"Port: {device.Port}");
                DebugLogger.Info("PortDetector", $"Name: {device.Name}");
                DebugLogger.Info("PortDetector", $"USB: {device.PnpId}");
            }


            var esp32Devices = devices.FindAll(device =>
                device.PnpId.Contains("VID_303A", StringComparison.OrdinalIgnoreCase) ||
                device.PnpId.Contains("VID_2341", StringComparison.OrdinalIgnoreCase));

            if (esp32Devices.Count == 0)
            {
                DebugLogger.Error("PortDetector", "No compatible microcontroller identified.");
                return null;
            }

            if (esp32Devices.Count > 1)
            {
                DebugLogger.Info("PortDetector", "Multiple ports found");

                foreach (var device in esp32Devices)
                {
                    DebugLogger.Info("PortDetector", $"- {device.Port}: {device.Name}");
                }

                return null;
            }

            string selectedPort = esp32Devices[0].Port;

            DebugLogger.Info("PortDetector", $"Microcontroller identified on port {selectedPort}");

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
                DebugLogger.Error("PortDetector", "Port not found. Connect the device.");
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
                NewLine = "\n"
            };

            try
            {
                serial.Open();
                DebugLogger.Info("Reader", $"Connected to {port}");

                // Attendere il reset provocato dall'apertura della COM
                Thread.Sleep(1500);


                Lexer lexer = new Lexer();
                Parser parser = new Parser();
                while (serial.IsOpen)
                {
                    try
                    {
                        string line = serial.ReadLine();
                        lexer.Set(line);
                        lexer.StartLexing();
                        lexer.ResetVariable();
                        if(IsRadioWaveComplete(lexer.TokenList) || IsErrorComplete(lexer.TokenList) || IsBatteryComplete(lexer.TokenList))
                        {
                            parser.Set(lexer.TokenList);
                            parser.StartParsing();
                            lexer.ResetList();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        throw new Exception($"{ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Reader", $"{ex}");
            }
            finally
            {
                if (serial?.IsOpen == true)
                    serial.Close();
            }
        }
        private bool IsRadioWaveComplete(List<Token> tokens)
        {
            bool frequency = false;
            bool amplitude = false;
            bool duration = false;
            bool timestamp = false;
            bool type = false;
            
            for (int i = 0; i < tokens.Count - 2; i++)
            {
                if (tokens[i].Type != TokenType.Identifier)
                    continue;

                switch (tokens[i].Value)
                {
                    case "Frequenza":
                        if (tokens[i + 2].Type == TokenType.IntValue)
                            frequency = true;
                        break;

                    case "Ampiezza":
                        if (tokens[i + 2].Type == TokenType.IntValue)
                            amplitude = true;
                        break;

                    case "Durata":
                        if (tokens[i + 2].Type == TokenType.IntValue)
                            duration = true;
                        break;

                    case "Timestamp":
                        if (tokens[i + 2].Type == TokenType.IntValue)
                            timestamp = true;
                        break;

                    case "Type":
                        if (tokens[i + 2].Type == TokenType.IntValue)
                            type = true;
                        break;
                }
            }
            bool result  =  frequency &&
                            amplitude &&
                            duration &&
                            timestamp &&
                            type;

            DebugLogger.Debug("Reader", $"IsRadioWaveComplete: {result}");

            return result;
        }
        private bool IsErrorComplete(List<Token> tokens)
        {
            bool Error = false;
            bool ErrorType = false;

            foreach (Token token in tokens)
            {
                if (token.Type != TokenType.Identifier || token.Type != TokenType.Dot)
                    continue;

                switch (token.Value)
                {
                   case "ERROR":
                        Error = true;
                        break;
                    case ".":
                        ErrorType = true;
                        break;
                }
            }

            bool result = Error && ErrorType;

            DebugLogger.Debug("Reader", $"IsErrorComplete: {result}");
            
            return  result;
        }

        private bool IsBatteryComplete(List<Token> tokens)
        {
            bool adcComplete = false;
            bool batteryComplete = false;
            bool chargeComplete = false;

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i].Type != TokenType.Identifier)
                    continue;

                if (tokens[i].Value == "ADC" &&
                    tokens[i + 1].Type == TokenType.FloatValue)
                {
                    adcComplete = true;
                }

                if (tokens[i].Value == "Battery" &&
                    tokens[i + 1].Type == TokenType.FloatValue)
                {
                    batteryComplete = true;
                }

                if (tokens[i].Value == "Charge" &&
                    tokens[i + 1].Type == TokenType.FloatValue)
                {
                    chargeComplete = true;
                }
            }
            
            bool result = adcComplete && batteryComplete && chargeComplete;

            DebugLogger.Debug("Reader", $"IsBatteryComplete: {result}");

            return result;
        }
    }
}