using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DataHandler
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;

        // Dati di stato e connessione
        private string _title = "MICROCONTROLLER MONITOR";
        private string _deviceModel = "ESP32";
        private string _selectedPort = "COM5";
        private string _connectionStatus = "DISCONNESSO";
        private string _operatingStatus = "OFFLINE";
        private bool _isConnected = false;
        private bool _isDarkMode = true;
        private int _selectedTabIndex = 0; // 0 = Panoramica, 1 = Schema a Blocchi

        // Dati batteria e ADC
        private float _batteryPercentage = 87f;
        private float _batteryVoltage = 3.87f;
        private float _adcVoltage = 2.94f;

        // Check dei singoli blocchi hardware
        private bool _microcontrollerOk = true;
        private bool _batteryOk = true;
        private bool _accOk = true;
        private bool _antenna1Ok = true;
        private bool _antenna2Ok = true;
        private bool _antenna3Ok = true;

        // Misurazioni radiofrequenza
        private double _frequencyMhz = 24;
        private double _amplitude = 47282;
        private double _durationSec = 2.61;
        private string _band = "HF";

        // Blocco selezionato nello schema per vederne i dettagli
        private HardwareBlock? _selectedBlock;

        // Thread per l'ascolto seriale in background
        private CancellationTokenSource? _serialCts;

        // FIX: usiamo lo stesso PortDetector di Reader.cs (filtrato per VID ESP32)
        // invece di prendere alla cieca la prima porta restituita da Windows.
        private readonly PortDetector _portDetector = new();

        public MainViewModel()
        {
            _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            AvailablePorts = new ObservableCollection<string>();
            LogEntries = new ObservableCollection<LogEntry>();
            HardwareBlocks = new ObservableCollection<HardwareBlock>();

            InitializeHardwareBlocks();
            
            // seleziono il primo blocco di default
            SelectedBlock = HardwareBlocks[0];

            // cerco le porte COM disponibili e mi metto in ascolto
            RefreshPorts();
            StartSerialListener();
        }

        #region Proprietà e Binding

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string DeviceModel
        {
            get => _deviceModel;
            set { _deviceModel = value; OnPropertyChanged(); }
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set 
            { 
                if (_selectedPort != value)
                {
                    _selectedPort = value; 
                    OnPropertyChanged(); 
                    // se cambio porta riparto con l'ascolto sulla nuova COM al volo
                    StartSerialListener();
                }
            }
        }

        public ObservableCollection<string> AvailablePorts { get; }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConnectionBrush)); }
        }

        public string OperatingStatus
        {
            get => _operatingStatus;
            set { _operatingStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusBrush)); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set 
            { 
                _isConnected = value; 
                OnPropertyChanged(); 
                ConnectionStatus = _isConnected ? "CONNESSO" : "DISCONNESSO";
                OperatingStatus = _isConnected ? "OPERATIVO" : "OFFLINE";
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set 
            { 
                _isDarkMode = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(ThemeToggleText));
                ApplyTheme(_isDarkMode);
            }
        }

        public string ThemeToggleText => IsDarkMode ? "TEMA CHIARO" : "TEMA SCURO";

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }

        // Gestione valori batteria
        public float BatteryPercentage
        {
            get => _batteryPercentage;
            set 
            { 
                _batteryPercentage = Math.Clamp(value, 0f, 100f); 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(BatteryBarWidth));
            }
        }

        public float BatteryVoltage
        {
            get => _batteryVoltage;
            set { _batteryVoltage = value; OnPropertyChanged(); }
        }

        public float AdcVoltage
        {
            get => _adcVoltage;
            set { _adcVoltage = value; OnPropertyChanged(); }
        }

        public double BatteryBarWidth => (BatteryPercentage / 100.0) * 160.0;

        // Flag di stato componenti
        public bool MicrocontrollerOk
        {
            get => _microcontrollerOk;
            set 
            { 
                _microcontrollerOk = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MicrocontrollerText));
                UpdateBlockState("ESP32", value, value ? "OK" : "ERRORE", value ? "Microcontrollore ESP32 perfettamente operativo." : "Errore hardware microcontrollore.");
            }
        }
        public string MicrocontrollerText => MicrocontrollerOk ? "OK" : "ERROR";

        public bool BatteryOk
        {
            get => _batteryOk;
            set 
            { 
                _batteryOk = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(BatteryStatusText));
                UpdateBlockState("Batteria", value, value ? "OK" : "CRITICA", value ? $"Batteria Li-Po {BatteryVoltage}V (ADC: {AdcVoltage}V)" : "Tensione batteria fuori intervallo (min 3.2V)!");
            }
        }
        public string BatteryStatusText => BatteryOk ? "OK" : "ERROR";

        public bool AccOk
        {
            get => _accOk;
            set 
            { 
                _accOk = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(AccStatusText));
                UpdateBlockState("ACC", value, value ? "OK" : "DISCONNESSO", value ? "Sensore ACC collegato e calibrato." : "WARNING: Sensore ACC non rilevato sul bus I2C.");
            }
        }
        public string AccStatusText => AccOk ? "OK" : "ERROR";

        public bool Antenna1Ok
        {
            get => _antenna1Ok;
            set 
            { 
                _antenna1Ok = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Antenna1Text));
                UpdateBlockState("Antenna 1 (3MHz)", value, value ? "OK" : "ASSENTE", value ? "Antenna 3 MHz pronta su Pin A0." : "ERROR: Antenna 3 MHz disconnessa.");
            }
        }
        public string Antenna1Text => Antenna1Ok ? "OK" : "ERROR";

        public bool Antenna2Ok
        {
            get => _antenna2Ok;
            set 
            { 
                _antenna2Ok = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Antenna2Text));
                UpdateBlockState("Antenna 2 (10MHz)", value, value ? "OK" : "ASSENTE", value ? "Antenna 10 MHz pronta su Pin A1." : "ERROR: Antenna 10 MHz disconnessa.");
            }
        }
        public string Antenna2Text => Antenna2Ok ? "OK" : "ERROR";

        public bool Antenna3Ok
        {
            get => _antenna3Ok;
            set 
            { 
                _antenna3Ok = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Antenna3Text));
                UpdateBlockState("Antenna 3 (24MHz)", value, value ? "OK" : "ASSENTE", value ? "Antenna 24 MHz pronta su Pin A2." : "ERROR: Antenna 24 MHz disconnessa.");
            }
        }
        public string Antenna3Text => Antenna3Ok ? "OK" : "ERROR";

        // Letture radio
        public double FrequencyMhz
        {
            get => _frequencyMhz;
            set { _frequencyMhz = value; OnPropertyChanged(); }
        }

        public double Amplitude
        {
            get => _amplitude;
            set { _amplitude = value; OnPropertyChanged(); }
        }

        public double DurationSec
        {
            get => _durationSec;
            set { _durationSec = value; OnPropertyChanged(); }
        }

        public string Band
        {
            get => _band;
            set { _band = value; OnPropertyChanged(); }
        }

        // Colori dinamici per gli indicatori
        public Brush StatusBrush => IsConnected ? new SolidColorBrush(Color.FromRgb(0, 230, 118)) : new SolidColorBrush(Color.FromRgb(255, 82, 82));
        public Brush ConnectionBrush => IsConnected ? new SolidColorBrush(Color.FromRgb(0, 230, 118)) : new SolidColorBrush(Color.FromRgb(255, 193, 7));

        // Liste per il log e per lo schema a blocchi
        public ObservableCollection<LogEntry> LogEntries { get; }
        public ObservableCollection<HardwareBlock> HardwareBlocks { get; }

        public HardwareBlock? SelectedBlock
        {
            get => _selectedBlock;
            set { _selectedBlock = value; OnPropertyChanged(); }
        }

        #endregion

        #region Lettura seriale e gestione dati

        private void InitializeHardwareBlocks()
        {
            HardwareBlocks.Add(new HardwareBlock { Name = "ESP32", Type = "Microcontrollore", StatusText = "OK", IsOk = true, Details = "Microcontrollore principale ESP32. Frequenza 240MHz, Flash 4MB." });
            HardwareBlocks.Add(new HardwareBlock { Name = "Batteria", Type = "Alimentazione", StatusText = "OK", IsOk = true, Details = "Batteria Li-Po 3.7V / ADC Pin A3. Tensione: 3.87V, Carica: 87%." });
            HardwareBlocks.Add(new HardwareBlock { Name = "ACC", Type = "Sensore", StatusText = "OK", IsOk = true, Details = "Sensore Accelerometro/Inclinazione collegato a ESP32." });
            HardwareBlocks.Add(new HardwareBlock { Name = "Antenna 1 (3MHz)", Type = "Antenna HF", StatusText = "OK", IsOk = true, Details = "Modulo Antenna 3 MHz collegato al Pin analogico A0." });
            HardwareBlocks.Add(new HardwareBlock { Name = "Antenna 2 (10MHz)", Type = "Antenna HF", StatusText = "OK", IsOk = true, Details = "Modulo Antenna 10 MHz collegato al Pin analogico A1." });
            HardwareBlocks.Add(new HardwareBlock { Name = "Antenna 3 (24MHz)", Type = "Antenna HF", StatusText = "OK", IsOk = true, Details = "Modulo Antenna 24 MHz collegato al Pin analogico A2." });
            HardwareBlocks.Add(new HardwareBlock { Name = "Interfaccia PC", Type = "Comunicazione", StatusText = "OK", IsOk = true, Details = "Connessione seriale USB (UART COM @ 9600 Baud)." });
        }

        public void AddLog(string level, string message)
        {
            _dispatcher.Invoke(() =>
            {
                LogEntries.Insert(0, new LogEntry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Level = level,
                    Message = message
                });

                if (LogEntries.Count > 100)
                {
                    LogEntries.RemoveAt(LogEntries.Count - 1);
                }
            });
        }

        private void UpdateBlockState(string name, bool isOk, string statusText, string details)
        {
            foreach (var block in HardwareBlocks)
            {
                if (block.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || block.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    block.IsOk = isOk;
                    block.StatusText = statusText;
                    block.Details = details;
                }
            }
        }

        public void RefreshPorts()
        {
            try
            {
                AvailablePorts.Clear();

                // FIX: SerialPort.GetPortNames() ritorna TUTTE le porte COM viste da
                // Windows, senza ordine garantito e senza distinguere quale dispositivo
                // sia collegato a quale porta. Se erano collegati sia un Arduino Uno
                // che un ESP32, il codice prendeva "AvailablePorts[0]" alla cieca,
                // che spesso corrispondeva alla porta con indice più alto (l'ultima
                // assegnata da Windows) invece che all'ESP32 desiderato.
                // Ora usiamo lo stesso identificatore per VID di Reader.cs, che
                // riconosce esplicitamente l'ESP32 ed esclude altri Arduino.
                var esp32Ports = _portDetector.FindEsp32Ports();

                foreach (var device in esp32Ports)
                {
                    AvailablePorts.Add(device.Port);
                }

                if (AvailablePorts.Count > 0)
                {
                    if (!AvailablePorts.Contains(SelectedPort))
                    {
                        SelectedPort = AvailablePorts[0];
                    }
                }
                else
                {
                    // FIX: niente più fallback fittizio su "COM5". Aggiungere una
                    // porta finta faceva sì che l'app tentasse comunque di aprirla:
                    // se quella porta esisteva per un altro dispositivo, serial.Open()
                    // riusciva comunque e l'interfaccia mostrava "CONNESSO" anche
                    // se l'ESP32 non era affatto collegato.
                    SelectedPort = string.Empty;
                    IsConnected = false;
                    AddLog("WARNING", "Nessun dispositivo ESP32 rilevato sulle porte COM disponibili.");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Error("ViewModel", "Errore nella ricerca porte seriali", ex);
            }
        }

        public void StartSerialListener()
        {
            // stoppo eventuali letture precedenti in background
            _serialCts?.Cancel();
            _serialCts = new CancellationTokenSource();
            CancellationToken token = _serialCts.Token;

            string portToOpen = SelectedPort;

            Task.Run(() => ListenToSerialPort(portToOpen, token), token);
        }

        private void ListenToSerialPort(string portName, CancellationToken token)
        {
            if (string.IsNullOrEmpty(portName))
            {
                // FIX: prima si usciva in silenzio senza avvisare nessuno. Ora
                // segnaliamo esplicitamente che non c'è nessuna porta valida,
                // così lo stato "DISCONNESSO" in UI è coerente con la realtà.
                _dispatcher.Invoke(() =>
                {
                    IsConnected = false;
                    AddLog("WARNING", "Nessuna porta valida selezionata: dispositivo non collegato.");
                });
                return;
            }

            SerialPort? serial = null;
            try
            {
                serial = new SerialPort(portName, 9600)
                {
                    DtrEnable = true,
                    RtsEnable = true,
                    ReadTimeout = 2500,
                    NewLine = "\n"
                };

                serial.Open();

                // FIX: l'apertura riuscita della porta seriale NON significa che
                // dall'altra parte ci sia davvero l'ESP32 collegato e funzionante:
                // Windows apre volentieri una COM anche se il dispositivo è muto,
                // scollegato subito dopo, o è un altro device che risponde a caso.
                // Non impostiamo più IsConnected = true qui: lo facciamo solo dopo
                // aver ricevuto il primo pacchetto di dati realmente valido.
                _dispatcher.Invoke(() =>
                {
                    OperatingStatus = "IN ATTESA DATI";
                    AddLog("INFO", $"Porta {portName} aperta, in attesa di dati dal dispositivo...");
                });

                bool firstValidDataReceived = false;

                Lexer lexer = new Lexer();
                Parser parser = new Parser();
                DataCalculator calculator = new DataCalculator();
                ExcelWriter excelWriter = new ExcelWriter();

                while (!token.IsCancellationRequested && serial.IsOpen)
                {
                    try
                    {
                        string line = serial.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        lexer.Set(line);
                        lexer.StartLexing();
                        lexer.ResetVariable();

                        if (IsRadioWaveComplete(lexer.TokenList) || IsErrorComplete(lexer.TokenList) || IsBatteryComplete(lexer.TokenList))
                        {
                            parser.Set(lexer.TokenList);
                            Data dataParsed = parser.StartParsing();
                            lexer.ResetList();

                            // qua aggiorniamo l'interfaccia principale al volo
                            _dispatcher.Invoke(() =>
                            {
                                if (!firstValidDataReceived)
                                {
                                    firstValidDataReceived = true;
                                    IsConnected = true;
                                    AddLog("INFO", $"Dispositivo confermato sulla porta {portName} @ 9600 Baud.");
                                }

                                ProcessTelemetryData(dataParsed, calculator, excelWriter);
                            });
                        }
                    }
                    catch (TimeoutException)
                    {
                        // timeout normale quando non arrivano dati seriali:
                        // se non abbiamo MAI ricevuto dati validi, restiamo onestamente
                        // "non connessi" invece di mostrare un falso positivo.
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Error("SerialListener", $"Errore lettura seriale: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() =>
                {
                    IsConnected = false;
                    AddLog("WARNING", $"In attesa di connessione sulla porta {portName}...");
                });
                DebugLogger.Error("SerialListener", $"Impossibile aprire la porta {portName}: {ex.Message}");
            }
            finally
            {
                if (serial?.IsOpen == true)
                {
                    serial.Close();
                    serial.Dispose();
                }

                _dispatcher.Invoke(() =>
                {
                    if (IsConnected)
                    {
                        IsConnected = false;
                        AddLog("WARNING", $"Connessione con la porta {portName} interrotta.");
                    }
                });
            }
        }

        private void ProcessTelemetryData(Data dataParsed, DataCalculator calculator, ExcelWriter excelWriter)
        {
            if (dataParsed is Battery b)
            {
                AdcVoltage = b.ADC;
                BatteryVoltage = b.VBattery;
                BatteryPercentage = b.Charge;
                BatteryOk = b.VBattery >= 3.2f;
                AddLog("INFO", $"Telemetria batteria: {BatteryVoltage:F2}V (ADC: {AdcVoltage:F2}V) - {BatteryPercentage:F0}%");
            }
            else if (dataParsed is RadioWave rw)
            {
                RadioMeasurements? measurements = calculator.Calculate(rw);
                if (measurements != null)
                {
                    FrequencyMhz = measurements.FrequencyMHz;
                    Amplitude = rw.Amplitude;
                    DurationSec = measurements.DurationSeconds;
                    Band = measurements.Band;

                    try
                    {
                        excelWriter.Write(rw, measurements);
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Error("ExcelWriter", $"Errore salvataggio Excel: {ex.Message}");
                    }

                    AddLog("INFO", $"Onda Radio: Freq {FrequencyMhz:F0}MHz | Amp {Amplitude:F0} | Banda {Band}");
                }
            }
            else if (dataParsed is Error err)
            {
                AddLog("ERROR", $"Errore hardware dal microcontrollore: {err.Type}");
            }
        }

        private bool IsRadioWaveComplete(List<Token> tokens)
        {
            bool frequency = false, amplitude = false, duration = false, timestamp = false, type = false;
            for (int i = 0; i < tokens.Count - 2; i++)
            {
                if (tokens[i].Type != TokenType.Identifier) continue;

                switch (tokens[i].Value)
                {
                    case "Frequenza":
                        if (tokens[i + 2].Type == TokenType.IntValue) frequency = true;
                        break;
                    case "Ampiezza":
                        if (tokens[i + 2].Type == TokenType.IntValue) amplitude = true;
                        break;
                    case "Durata":
                        if (tokens[i + 2].Type == TokenType.IntValue) duration = true;
                        break;
                    case "Timestamp":
                        if (tokens[i + 2].Type == TokenType.IntValue) timestamp = true;
                        break;
                    case "Type":
                        if (tokens[i + 2].Type == TokenType.IntValue) type = true;
                        break;
                }
            }
            return frequency && amplitude && duration && timestamp && type;
        }

        private bool IsErrorComplete(List<Token> tokens)
        {
            bool err = false, errType = false;
            foreach (Token token in tokens)
            {
                if (token.Type != TokenType.Identifier && token.Type != TokenType.Dot) continue;
                if (token.Value == "ERROR") err = true;
                if (token.Value == ".") errType = true;
            }
            return err && errType;
        }

        private bool IsBatteryComplete(List<Token> tokens)
        {
            bool adcComplete = false, batteryComplete = false, chargeComplete = false;
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i].Type != TokenType.Identifier) continue;

                if (tokens[i].Value == "ADC" && tokens[i + 3].Type == TokenType.FloatValue) adcComplete = true;
                if (tokens[i].Value == "Battery" && tokens[i + 2].Type == TokenType.FloatValue) batteryComplete = true;
                if (tokens[i].Value == "Charge" && tokens[i + 2].Type == TokenType.FloatValue) chargeComplete = true;
            }
            return adcComplete && batteryComplete && chargeComplete;
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        private void ApplyTheme(bool isDark)
        {
            var res = Application.Current.Resources;
            if (isDark)
            {
                // colori tema scuro
                res["BgBrush"] = new SolidColorBrush(Color.FromRgb(0x09, 0x0A, 0x0E));
                res["CardBgBrush"] = new SolidColorBrush(Color.FromRgb(0x12, 0x14, 0x1D));
                res["CardHeaderBgBrush"] = new SolidColorBrush(Color.FromRgb(0x19, 0x1C, 0x28));
                res["BorderBrush"] = new SolidColorBrush(Color.FromRgb(0x26, 0x2B, 0x3D));
                res["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                res["ButtonBgBrush"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1E, 0x2B));
                res["ButtonHoverBgBrush"] = new SolidColorBrush(Color.FromRgb(0x28, 0x2E, 0x42));
                res["DiagramBgBrush"] = new SolidColorBrush(Color.FromRgb(0x0D, 0x0E, 0x14));
                res["ComboBgBrush"] = new SolidColorBrush(Color.FromRgb(0x18, 0x1B, 0x26));
            }
            else
            {
                // colori tema chiaro/bianco
                res["BgBrush"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
                res["CardBgBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                res["CardHeaderBgBrush"] = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
                res["BorderBrush"] = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
                res["TextPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
                res["ButtonBgBrush"] = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
                res["ButtonHoverBgBrush"] = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
                res["DiagramBgBrush"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
                res["ComboBgBrush"] = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9));
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
