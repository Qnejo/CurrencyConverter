using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Threading.Tasks;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        private List<string> currencies = new List<string>();
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await GetCurrencyCodesAsync();
            
            FromCurrencyComboBox.ItemsSource = currencies;
            ToCurrencyComboBox.ItemsSource = currencies;
        }

        private async Task<List<string>> GetCurrencyCodesAsync()
        {
            string url = "https://www.cbr-xml-daily.ru/daily_json.js";

            try
            {
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(url);


                var data = JsonSerializer.Deserialize<CbrResponse>(json);
                currencies.Add("RUB");
                foreach (var currency in data.Valute)
                {
                    currencies.Add(currency.Key);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить курсы валют. Ошибка: {ex.Message}. Проверьте подключение к интернету."); 
            }
            return currencies;
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(InputAmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Введите число!");
                return;
            }
            
            ConvertButton.IsEnabled = false;
            ResultTextBlock.Text = "Загружаю курсы...";
            
            string url = "https://www.cbr-xml-daily.ru/daily_json.js";

            try
            {
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(url);

                var data = JsonSerializer.Deserialize<CbrResponse>(json);

                string from = (FromCurrencyComboBox.SelectedItem as string) ?? "RUB";
                string to = (ToCurrencyComboBox.SelectedItem as string) ?? "RUB";

                decimal fromRate = from == "RUB" ? 1 : data.Valute[from].Value / data.Valute[from].Nominal;
                decimal toRate = to == "RUB" ? 1 : data.Valute[to].Value / data.Valute[to].Nominal;

                decimal result = amount * (fromRate / toRate);

                ResultTextBlock.Text = $"{amount} {from} = {result:F4} {to}";


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить курсы валют. Ошибка: {ex.Message}");
            }
            finally
            {
                ConvertButton.IsEnabled = true;
            }
        }
        
        public class CbrResponse
        {
            public Dictionary<string, CurrencyInfo> Valute { get; set; }
        }
        
        public class CurrencyInfo
        {
            public decimal Value { get; set; }
            public string CharCode { get; set; }   
            public int Nominal { get; set; }
        }

        private void ReverseCurrencyCode(object sender, RoutedEventArgs e)
        {
            object FromCurrencyComboBoxItem = FromCurrencyComboBox.SelectedItem;
            FromCurrencyComboBox.SelectedItem = ToCurrencyComboBox.SelectedItem;
            ToCurrencyComboBox.SelectedItem = FromCurrencyComboBoxItem;
        }
    }
}