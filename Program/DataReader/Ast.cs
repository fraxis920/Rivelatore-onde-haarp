namespace DataReader
{
    abstract class Data
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

    class Error
    {
        public string? Type { get; set; }
    }
}