using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace SortingVisualizer.Services
{
    public static class ExcelLoader
    {
        public static double[] Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException("Файл не найден: " + path);

            string ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".csv" || ext == ".txt")
                return LoadCsv(path);

            if (ext == ".xlsx" || ext == ".xlsm")
                return LoadXlsx(path);

            if (ext == ".xls")
                throw new NotSupportedException(
                    "Старый формат .xls не поддерживается. Сохраните файл как .xlsx или .csv.");

            throw new NotSupportedException("Неподдерживаемое расширение: " + ext);
        }

        // ==================== CSV ====================

        private static double[] LoadCsv(string path)
        {
            var list = new List<double>();
            var lines = File.ReadAllLines(path, Encoding.UTF8);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = line.Split(new char[] { ',', ';', '\t' });
                foreach (var cell in cells)
                {
                    string raw = cell.Trim().Trim('"');
                    if (raw.Length == 0) continue;

                    double v;
                    string s1 = raw.Replace('.', ',');
                    if (double.TryParse(s1, NumberStyles.Any, CultureInfo.CurrentCulture, out v)
                        || double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                    {
                        list.Add(v);
                    }
                }
            }

            if (list.Count == 0)
                throw new InvalidDataException("В файле нет числовых значений.");
            return list.ToArray();
        }

        // ==================== XLSX ====================

        private static double[] LoadXlsx(string path)
        {
            var list = new List<double>();

            using (var fs = File.OpenRead(path))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                // 1) sharedStrings
                var sharedStrings = new List<string>();
                var sharedEntry = zip.GetEntry("xl/sharedStrings.xml");
                if (sharedEntry != null)
                {
                    using (var s = sharedEntry.Open())
                    using (var reader = XmlReader.Create(s))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "t")
                                sharedStrings.Add(reader.ReadElementContentAsString());
                        }
                    }
                }

                // 2) лист
                var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
                if (sheetEntry == null)
                    throw new InvalidDataException("В файле нет листа sheet1.");

                using (var s = sheetEntry.Open())
                using (var reader = XmlReader.Create(s))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.Name != "c")
                            continue;

                        string type = reader.GetAttribute("t");
                        string value = null;
                        int depth = reader.Depth;

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "v")
                            {
                                value = reader.ReadElementContentAsString();
                                break;
                            }
                            if (reader.NodeType == XmlNodeType.EndElement
                                && reader.Depth == depth
                                && reader.Name == "c")
                                break;
                        }

                        if (value == null) continue;

                        if (type == "s")
                        {
                            int idx;
                            if (int.TryParse(value, out idx)
                                && idx >= 0 && idx < sharedStrings.Count)
                            {
                                double dv;
                                string s1 = sharedStrings[idx].Trim().Replace('.', ',');
                                if (double.TryParse(s1, NumberStyles.Any,
                                        CultureInfo.CurrentCulture, out dv)
                                    || double.TryParse(sharedStrings[idx].Trim(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture, out dv))
                                {
                                    list.Add(dv);
                                }
                            }
                        }
                        else
                        {
                            double dv;
                            string s1 = value.Trim().Replace('.', ',');
                            if (double.TryParse(s1, NumberStyles.Any,
                                    CultureInfo.CurrentCulture, out dv)
                                || double.TryParse(value.Trim(),
                                    NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out dv))
                            {
                                list.Add(dv);
                            }
                        }
                    }
                }
            }

            if (list.Count == 0)
                throw new InvalidDataException("В файле нет числовых значений.");
            return list.ToArray();
        }
    }
}