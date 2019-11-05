using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microcharts;

namespace StockApp
{
    public partial class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async private void GoButton_Clicked(object sender, EventArgs e)
        {
            var client = new HttpClient();
            if (string.IsNullOrWhiteSpace(StockName.Text))
            {
                await DisplayAlert("Error", "Invalid Stock Name", " ", "Okay");
                return;
            }
            var stockURL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=" + StockName.Text + "&apikey=[API_KEY]";
            var uri = new Uri(stockURL);

            StockInfo stockData = new StockInfo();
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                stockData = JsonConvert.DeserializeObject<StockInfo>(jsonContent);
            }

            if (string.IsNullOrEmpty(stockData.MetaData.The1Information))
            {
                await DisplayAlert("Error", "Invalid Stock Name", "Cancel");
            }
            StockListView.ItemsSource = new ObservableCollection<TimeSeriesDaily>(stockData.TSD.Values); // values
            var temp = stockData.TSD.Values.ToList();
            int count = 0;

            HighStock.Text = "0";
            LowStock.Text = "1000000";
            while (count < 100)
            {
                if (float.Parse(HighStock.Text) < float.Parse(temp[count].The2High))
                {
                    HighStock.Text = temp[count].The2High.ToString();
                }
                if (float.Parse(LowStock.Text) > float.Parse(temp[count].The3Low))
                {
                    LowStock.Text = temp[count].The3Low.ToString();
                }

                count++;
            }
            LowStock.Text = "Lowest: " + LowStock.Text;
            HighStock.Text = "Highest: " + HighStock.Text;
            CreateCharts(stockData.TSD);
        }
        private void CreateCharts(Dictionary<string, TimeSeriesDaily> s)
        {
            var temp = s.Values.ToList();
            int count = 0;
            var entries = new List<Microcharts.Entry>();
            while (count < 30) // not ideal to hard code but quick solution
            {
                entries.Add(new Microcharts.Entry(float.Parse(temp[count].The2High))
                {
                    Label = "",
                    ValueLabel = ""
                });
                if (count % 5 == 0)
                {
                    entries[count].ValueLabel = temp[count].The2High;
                }
                count++;
            }
            var linechartShort = new LineChart() { Entries = entries };
            Monthly.Chart = linechartShort;
            //var lineChart = new LineChart() { Entries =  entries};
            // Longterm.Chart = lineChart;
        }

    }
}
