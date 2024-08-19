using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WeatherCheck
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiKey = "YOUR-API-HERE";

        public MainWindow()
        {
            InitializeComponent();
            VisibilitaPlaceHolder();

            CasellaDiRicerca.KeyDown += CityTextBox_KeyDown;

            string cittaIniziale = "Roma";
            CasellaDiRicerca.Text = cittaIniziale;
            _ = OttieniMeteo(cittaIniziale);
            CasellaDiRicerca.Text = "";
        }

        private async Task OttieniMeteo()
        {
            string citta = CasellaDiRicerca.Text;
            await OttieniMeteo(citta);
        }

        private async Task OttieniMeteo(string citta)
        {
            iconaUmidita.Source = new BitmapImage(new Uri($"pack://application:,,,/{"immagini/humidity.png"}"));
            
            // in caso non venga messa nessuna città nella casella di ricerca

            if (string.IsNullOrWhiteSpace(citta))
            {
                iconaMeteo.Source = new BitmapImage(new Uri($"pack://application:,,,/{"immagini/info.png"}"));
                MostraMessaggio("Inserisci il nome di una città.");
                PulisciInterfacciaUtente();
                return;
            }

            //////////////////////////////////////////////////////////////////

            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={citta}&appid={apiKey}";
                var risposta = await client.GetAsync(url);

                if (!risposta.IsSuccessStatusCode)
                {
                    PulisciInterfacciaUtente();

                    // in caso la città non venga trovata

                    if (risposta.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        iconaMeteo.Source = new BitmapImage(new Uri($"pack://application:,,,/{"immagini/info.png"}"));
                        MostraMessaggio("La città non è stata trovata.");
                    }

                    //////////////////////////////////////////////////////////////////

                    // in caso di errori

                    else
                    {
                        iconaMeteo.Source = new BitmapImage(new Uri($"pack://application:,,,/{"immagini/info.png"}"));
                        MostraMessaggio("Si è verificato un errore durante il recupero dei dati meteo.");
                    }
                    return;
                }

                    //////////////////////////////////////////////////////////////////

                var jsonResponse = await risposta.Content.ReadAsStringAsync();
                var datiMeteo = JObject.Parse(jsonResponse);

                ImpostaIconaMeteo(datiMeteo);

                var umidità = (int)datiMeteo["main"]["humidity"];
                testoUmidita.Text = $"Umidità: {umidità}%";

                MostraMessaggio("");

                var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
                var textInfo = cultureInfo.TextInfo;
                testoCitta.Text = textInfo.ToTitleCase(citta.ToLower());
            }
            catch (Exception ex)
            {
                PulisciInterfacciaUtente();
                MostraMessaggio("Si è verificato un errore durante il recupero dei dati meteo: " + ex.Message);
            }
        }

        private void ImpostaIconaMeteo(JObject datiMeteo)
        {
            string meteo = datiMeteo["weather"][0]["description"].ToString();
            string iconaPath;

            meteo = meteo.ToLower();

            if (meteo.Contains("clear sky"))
            {
                iconaPath = "immagini/clear.png";
            }
            else if (meteo.Contains("few clouds"))
            {
                iconaPath = "immagini/few_clouds_day.png";
            }
            else if (meteo.Contains("scattered clouds") || meteo.Contains("broken clouds"))
            {
                iconaPath = "immagini/cloudy.png";
            }
            else if (meteo.Contains("shower rain"))
            {
                iconaPath = "immagini/hail.png";
            }
            else if (meteo.Contains("rain"))
            {
                iconaPath = "immagini/rain.png";
            }
            else if (meteo.Contains("thunderstorm"))
            {
                iconaPath = "immagini/storm.png";
            }
            else if (meteo.Contains("snow"))
            {
                iconaPath = "immagini/snow.png";
            }
            else if (meteo.Contains("overcast clouds"))
            {
                iconaPath = "immagini/overcast_clouds.png";
            }
            else if (meteo.Contains("mist") || meteo.Contains("smoke") || meteo.Contains("dust") || meteo.Contains("haze") || meteo.Contains("sand") || meteo.Contains("ash"))
            {
                iconaPath = "immagini/if-weather-30-2682821_90800.png";
            }
            else if (meteo.Contains("squall") || meteo.Contains("tornado"))
            {
                iconaPath = "immagini/windy.png";
            }
            else
            {
                iconaPath = "immagini/info.png";
            }

            if (!string.IsNullOrEmpty(iconaPath))
            {
                iconaMeteo.Source = new BitmapImage(new Uri($"pack://application:,,,/{iconaPath}"));
            }
            else
            {
                iconaMeteo.Source = null;
            }

            double temperatura = (double)datiMeteo["main"]["temp"];
            int temperaturaArrotondata = (int)Math.Round(temperatura);
            testoTemperatura.Text = $"{temperaturaArrotondata - 273}°C";
        }

        private void CasellaDiRicerca_TestoCambiato(object sender, TextChangedEventArgs e)
        {
            VisibilitaPlaceHolder();
        }

        private void VisibilitaPlaceHolder()
        {
            PlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(CasellaDiRicerca.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MostraMessaggio(string messaggio)
        {
            testoMeteo.Text = messaggio;
        }

        private void PulisciInterfacciaUtente()
        {
            testoTemperatura.Text = string.Empty;
            testoCitta.Text = string.Empty;
            iconaUmidita.Source = null;
            testoUmidita.Text = string.Empty;
        }

        private async void OttieniMeteo_premuto(object sender, RoutedEventArgs e)
        {
            await OttieniMeteo();
        }

        private async void CityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await OttieniMeteo();
                e.Handled = true;
            }
        }
    }
}