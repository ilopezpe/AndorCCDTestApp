using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace AndorCCDTestApp
{
    public static class FileIO
    {
        public static void SaveData(List<int[]> ListOfArrays)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "CSV File|*.csv",
                Title = "Save As"
            };
            var result = saveFileDialog1.ShowDialog();

            if (result != DialogResult.OK || saveFileDialog1.FileName == "") return;
            if (File.Exists(saveFileDialog1.FileName)) File.Delete(saveFileDialog1.FileName);

            switch (saveFileDialog1.FilterIndex)
            {
                case 1: // Writes a CSV file
                    if (ListOfArrays != null)
                    {
                        var records = ListOfArrays;
                        using (var writer = new StreamWriter(saveFileDialog1.FileName))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.HasHeaderRecord = false;
                            foreach (var record in records)
                            {
                                csv.WriteField(record);
                                csv.NextRecord();
                            }
                            writer.Flush();
                        }
                    }
                    break;
            }
        }
    }
}
