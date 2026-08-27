namespace DataHandler
{
    public abstract class Data
    {
    }

    class Battery : Data
    {
        public float ADC { get; set; }
        public float VBattery { get; set; }
        public float Charge { get; set; }
    }

    class RadioWave : Data
    {
        public int Frequency { get; set; }
        public int Amplitude { get; set; }
        public long Duration { get; set; }
        public int Timestamp { get; set; }
        public int Type { get; set; }
    }
    public class RadioMeasurements : Data
    {
        public double Period { get; set; }
        public double Wavelength { get; set; }
        public double DurationSeconds { get; set; }
        public double CycleCount { get; set; }
        public double RelativePower { get; set; }
        public double NormalizedAmplitude { get; set; }
        public double FrequencyHz { get; set; }
        public double FrequencyKHz { get; set; }
        public double FrequencyMHz { get; set; }
        public double FrequencyGHz { get; set; }
        public string Band { get; set; } = string.Empty;
    }

    class Status : Data
    {
        public Parameters? Battery { get; set; }
        public Parameters? Antenna1 { get; set; }
        public Parameters? Antenna2 { get; set; }
        public Parameters? Antenna3 { get; set; }
    }
    
    class Parameters
    {
        public bool Functionality { get; set; }
        public Error? Error { get; set; }
    }

    class Error : Data
    {
        public string? Type { get; set; }
    }
}