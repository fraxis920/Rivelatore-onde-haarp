using ClosedXML.Excel;

namespace DataHandler
{
    public class ExcelWriter
    {
        private const string Source = "ExcelWriter";

        public void Write(Data data, RadioMeasurements? measurements)
        {
            DebugLogger.Enter(Source, "Write");

            if (data is not RadioWave wave)
            {
                DebugLogger.Warning(
                    Source,
                    $"Unsupported data type: {data.GetType().Name}"
                );

                DebugLogger.Exit(Source, "Write");
                return;
            }

            if (measurements is null)
            {
                DebugLogger.Warning(
                    Source,
                    "RadioMeasurements is null"
                );

                DebugLogger.Exit(Source, "Write");
                return;
            }

            try
            {
                string filePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "RadioData.xlsx"
                );

                using XLWorkbook workbook = File.Exists(filePath)
                    ? new XLWorkbook(filePath)
                    : new XLWorkbook();

                IXLWorksheet worksheet;

                if (workbook.Worksheets.Any())
                    worksheet = workbook.Worksheet(1);
                else
                    worksheet = workbook.Worksheets.Add("RadioWave");

                int row = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 1;

                if (row == 1)
                {
                    worksheet.Cell(row, 1).Value = "Frequency";
                    worksheet.Cell(row, 2).Value = "Amplitude";
                    worksheet.Cell(row, 3).Value = "Duration";
                    worksheet.Cell(row, 4).Value = "Timestamp";
                    worksheet.Cell(row, 5).Value = "Type";

                    worksheet.Cell(row, 6).Value = "Period";
                    worksheet.Cell(row, 7).Value = "Wavelength";
                    worksheet.Cell(row, 8).Value = "Duration Seconds";
                    worksheet.Cell(row, 9).Value = "Cycle Count";
                    worksheet.Cell(row, 10).Value = "Relative Power";
                    worksheet.Cell(row, 11).Value = "Normalized Amplitude";

                    worksheet.Cell(row, 12).Value = "Frequency Hz";
                    worksheet.Cell(row, 13).Value = "Frequency KHz";
                    worksheet.Cell(row, 14).Value = "Frequency MHz";
                    worksheet.Cell(row, 15).Value = "Frequency GHz";
                    worksheet.Cell(row, 16).Value = "Band";

                    worksheet.Cell(row, 17).Value = "Is Wanted";

                    row++;
                }

                worksheet.Cell(row, 1).Value = wave.Frequency;
                worksheet.Cell(row, 2).Value = wave.Amplitude;
                worksheet.Cell(row, 3).Value = wave.Duration;
                worksheet.Cell(row, 4).Value = wave.Timestamp;
                worksheet.Cell(row, 5).Value = wave.Type;

                worksheet.Cell(row, 6).Value = measurements.Period;
                worksheet.Cell(row, 7).Value = measurements.Wavelength;
                worksheet.Cell(row, 8).Value = measurements.DurationSeconds;
                worksheet.Cell(row, 9).Value = measurements.CycleCount;
                worksheet.Cell(row, 10).Value = measurements.RelativePower;
                worksheet.Cell(row, 11).Value = measurements.NormalizedAmplitude;

                worksheet.Cell(row, 12).Value = measurements.FrequencyHz;
                worksheet.Cell(row, 13).Value = measurements.FrequencyKHz;
                worksheet.Cell(row, 14).Value = measurements.FrequencyMHz;
                worksheet.Cell(row, 15).Value = measurements.FrequencyGHz;
                worksheet.Cell(row, 16).Value = measurements.Band;

                worksheet.Cell(row, 17).Value =
                wave.Frequency >= 1 && wave.Frequency <= 30
                    ? "Si"
                    : "Non gestita";

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);

                DebugLogger.Info(
                    Source,
                    $"RadioWave written to Excel: {filePath}"
                );

                DebugLogger.Exit(Source, "Write");
            }
            catch (Exception ex)
            {
                DebugLogger.Error(
                    Source,
                    "Error writing RadioWave to Excel",
                    ex
                );

                DebugLogger.Exit(Source, "Write");
            }
        }
    }
}