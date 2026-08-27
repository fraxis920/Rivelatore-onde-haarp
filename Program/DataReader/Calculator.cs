namespace DataHandler
{
    public class DataCalculator
    {
        private const double SpeedOfLight = 299792458.0;

        public RadioMeasurements? Calculate(Data data)
        {
            DebugLogger.Enter("DataCalculator", "Calculate");

            if (data is not RadioWave wave)
            {
                DebugLogger.Warning(
                    "DataCalculator",
                    $"Unsupported data type: {data.GetType().Name}"
                );

                DebugLogger.Exit("DataCalculator", "Calculate", null);
                return null;
            }

            if (wave.Frequency <= 0)
            {
                DebugLogger.Error(
                    "DataCalculator",
                    $"Invalid frequency: {wave.Frequency}"
                );

                DebugLogger.Exit("DataCalculator", "Calculate", null);
                return null;
            }

            double frequencyMHz = wave.Frequency;
            double frequencyHz = frequencyMHz * 1_000_000.0;
            double frequencyKHz = frequencyMHz * 1_000.0;
            double frequencyGHz = frequencyMHz / 1_000.0;

            double period = 1.0 / frequencyHz;
            double wavelength = SpeedOfLight / frequencyHz;
            double durationSeconds = wave.Duration / 1_000_000.0;
            double cycleCount = frequencyHz * durationSeconds;
            double relativePower = Math.Pow(wave.Amplitude, 2);
            double normalizedAmplitude = wave.Amplitude / 65535.0;

            string band = CalculateBand(frequencyHz);

            RadioMeasurements measurements = new RadioMeasurements
            {
                Period = period,
                Wavelength = wavelength,
                DurationSeconds = durationSeconds,
                CycleCount = cycleCount,
                RelativePower = relativePower,
                NormalizedAmplitude = normalizedAmplitude,
                FrequencyHz = frequencyHz,
                FrequencyKHz = frequencyKHz,
                FrequencyMHz = frequencyMHz,
                FrequencyGHz = frequencyGHz,
                Band = band
            };

            DebugLogger.Data("DataCalculator", "FrequencyHz", measurements.FrequencyHz);
            DebugLogger.Data("DataCalculator", "FrequencyKHz", measurements.FrequencyKHz);
            DebugLogger.Data("DataCalculator", "FrequencyMHz", measurements.FrequencyMHz);
            DebugLogger.Data("DataCalculator", "FrequencyGHz", measurements.FrequencyGHz);
            DebugLogger.Data("DataCalculator", "Period", measurements.Period);
            DebugLogger.Data("DataCalculator", "Wavelength", measurements.Wavelength);
            DebugLogger.Data("DataCalculator", "DurationSeconds", measurements.DurationSeconds);
            DebugLogger.Data("DataCalculator", "CycleCount", measurements.CycleCount);
            DebugLogger.Data("DataCalculator", "RelativePower", measurements.RelativePower);
            DebugLogger.Data("DataCalculator", "NormalizedAmplitude", measurements.NormalizedAmplitude);
            DebugLogger.Data("DataCalculator", "Band", measurements.Band);

            DebugLogger.Exit("DataCalculator", "Calculate", measurements);

            return measurements;
        }

        private string CalculateBand(double frequency)
        {
            if (frequency >= 3_000 && frequency < 30_000)
                return "VLF";

            if (frequency >= 30_000 && frequency < 300_000)
                return "LF";

            if (frequency >= 300_000 && frequency < 3_000_000)
                return "MF";

            if (frequency >= 3_000_000 && frequency < 30_000_000)
                return "HF";

            if (frequency >= 30_000_000 && frequency < 300_000_000)
                return "VHF";

            if (frequency >= 300_000_000 && frequency < 3_000_000_000)
                return "UHF";

            if (frequency >= 3_000_000_000 && frequency < 30_000_000_000)
                return "SHF";

            if (frequency >= 30_000_000_000 && frequency <= 300_000_000_000)
                return "EHF";

            return "Unknown";
        }
    }
}