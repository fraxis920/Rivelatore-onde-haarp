# Per i gay:
# Viva il donatucci

L'interfaccia grafica e un'applicazione WPF basata su .NET. E necessario avere
installato il **.NET 10 SDK** su Windows.

Aprire PowerShell nella cartella principale del repository ed eseguire:

```powershell
cd ".\Rivelatore-onde-haarp-main\Program\DataReader"
dotnet run --project .\CSpr.csproj
```

Se PowerShell e gia aperto nella cartella interna
`Rivelatore-onde-haarp-main`, usare invece:

```powershell
cd "Program\DataReader"
dotnet run --project .\CSpr.csproj
```

Per avviarlo dopo la compilazione senza usare il terminale:

```powershell
Start-Process ".\bin\Debug\net10.0-windows\CSpr.exe"
```
e

# cd ".\Rivelatore-onde-haarp-main\Program\DataReader"
dotnet run --project .\CSpr.csproj

In alternativa, dopo una compilazione, e possibile avviare direttamente
`Program\DataReader\bin\Debug\net10.0-windows\CSpr.exe`.

- `Program/DataReader/MainWindow.xaml`: layout principale della finestra, dashboard, schede e controlli grafici.
- `Program/DataReader/MainWindow.xaml.cs`: gestione dei pulsanti e delle interazioni della finestra.
- `Program/DataReader/App.xaml`: punto di avvio (`StartupUri`) e definizione di colori, font e stili globali.
- `Program/DataReader/App.xaml.cs`: inizializzazione dell'applicazione e gestione degli errori WPF.
- `Program/DataReader/MainViewModel.cs`: dati e logica collegati alla dashboard, tra cui porte COM, batteria e stato ESP32.
- `Program/DataReader/BlockDiagramView.xaml`: vista grafica dello schema a blocchi hardware.
- `Program/DataReader/BlockDiagramView.xaml.cs`: comportamento dello schema a blocchi.

Per modificare l'aspetto della finestra principale usare `MainWindow.xaml`; per
modificare il tema e gli stili comuni usare `App.xaml`.