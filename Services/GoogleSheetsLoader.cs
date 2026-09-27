using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace SortingVisualizer.Services
{
    public static class GoogleSheetsLoader
    {
        private static readonly HttpClient _http = new HttpClient();

        public static double[] LoadFromUrl(string url)
        {
            if (url == null || !url.Contains("docs.google.com/spreadsheets"))
                throw new ArgumentException("Ссылка не похожа на Google Sheets.");

            string csvUrl = ToCsvUrl(url);
            string csv = _http.GetStringAsync(csvUrl).GetAwaiter().GetResult();
            return ParseCsv(csv);
        }

        private static string ToCsvUrl(string url)
        {
            if (url.Contains("/export?format=csv")) return url;
            int idx = url.IndexOf("/edit");
            if (idx < 0) idx = url.IndexOf("/view");
            if (idx < 0) throw new ArgumentException("Не удалось определить ID таблицы.");
            return url.Substring(0, idx) + "/export?format=csv";
        }

        private static double[] ParseCsv(string csv)
        {
            var list = new List<double>();
            var lines = csv.Split(new char[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var cells = line.Split(new char[] { ',', ';', '\t' });
                foreach (var cell in cells)
                {
                    string raw = cell.Trim().Trim('"');
                    string s = raw.Replace('.', ',');
                    double v;
                    if (double.TryParse(s, NumberStyles.Any,
                            CultureInfo.CurrentCulture, out v)
                        || double.TryParse(raw, NumberStyles.Any,
                            CultureInfo.InvariantCulture, out v))
                    {
                        list.Add(v);
                    }
                }
            }

            if (list.Count == 0)
                throw new InvalidOperationException("В таблице нет числовых данных.");
            return list.ToArray();
        }
    }
}