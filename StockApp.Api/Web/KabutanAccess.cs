using AngleSharp;
using AngleSharp.Dom;
using StockApp.Api.Models;

namespace StockApp.Api.Web
{
    public class KabutanAccess
    {
        public static async Task<List<StockData>> GetStockDataAsync(string code, DateTime latest)
        {
            // 引数チェック
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("codeがnullです。");
            if (code.Count() < 4) throw new ArgumentException("codeが不正です。");

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            var data = new List<StockData>();
            for (int page = 1; page <= 3; page++)
            {
                var address = $"https://kabutan.jp/stock/kabuka?code={code}&ashi=day&page={page}";

                Console.WriteLine(address);

                // サイトアクセス
                var document = await context.OpenAsync(address);

                // 株価データ
                var selector = string.Empty;
                if (page == 1)
                {
                    selector = "#stock_kabuka_table > table.stock_kabuka0 > tbody > tr";
                    var kabutanData = GetStockData(code, document, selector);
                    var saveData = kabutanData.Where(d => d.Date > latest);
                    data.AddRange(saveData);

                    selector = "#stock_kabuka_table > table.stock_kabuka_dwm > tbody > tr";
                    kabutanData = GetStockData(code, document, selector);
                    saveData = kabutanData.Where(d => d.Date > latest);
                    data.AddRange(saveData);
                }
                else
                {
                    selector = "#stock_kabuka_table > table.stock_kabuka_dwm > tbody > tr";
                    var kabutanData = GetStockData(code, document, selector);
                    var saveData = kabutanData.Where(d => d.Date > latest);

                    if (saveData.Count() == 0) break;

                    data.AddRange(saveData);
                }

                Thread.Sleep(10 * 1000);
            }

            return data.OrderBy(c => c.Date).ToList();
        }

        private static List<StockData> GetStockData(string code, IDocument document, string selector)
        {
            var data = new List<StockData>();

            var trs = document.QuerySelectorAll(selector);
            foreach (var tr in trs)
            {
                var th = tr.QuerySelector("th");
                var tds = tr.QuerySelectorAll("td");

                if (th == null) continue;
                if (tds?.Count() != 7) continue;

                if (DateTime.TryParse(th.TextContent, out DateTime date))
                {
                    if (double.TryParse(tds[0].TextContent, out double open) &&
                        double.TryParse(tds[1].TextContent, out double high) &&
                        double.TryParse(tds[2].TextContent, out double low) &&
                        double.TryParse(tds[3].TextContent, out double close))
                    {
                        data.Add(new StockData { Date = date, CountryCompanyCode = $"{Country.JPN}/{code}".ToLower(),
                            Open = open, High = high, Low = low, Close = close });
                    }
                }
            }

            return data;
        }
    }
}
