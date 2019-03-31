using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Image = System.Drawing.Image;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //api key c665da500cfb20637389a225a77ffa71
    // api call api.openweathermap.org/data/2.5/forecast?q={city name},{country code}


    public partial class MainWindow : Window
    {
        const string appid = "c665da500cfb20637389a225a77ffa71";

        public string TempSymbol { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource =
              new BitmapImage(new Uri("../../background.jpg", UriKind.Relative));
            this.Background = myBrush;
            TempSymbol = "°C";
            getUserLocation();
        }

        void getUserLocation()
        {
            using (WebClient web = new WebClient())
            {
                string url = string.Format("https://api.ipdata.co/?api-key=cc18d6b6ad30a29071dc75de15debef6e74093fad097d41b93f76e1c");
                var jsonLocation = web.DownloadString(url);

                var result = JsonConvert.DeserializeObject<WeatherInfo.IPObject>(jsonLocation);
                getWeather(string.Format("{0}", result.city));
            }
        }

        int GetMaximumTemp(int start, int end, WeatherInfo.List[] list)
        {
            int max = 0;
            for(int i = start; i < end; i++)
            {
                int value = (int)list[i].main.temp_max;
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        int GetMinimumTemp(int start, int end, WeatherInfo.List[] list)
        {
            int min = 0;
            for (int i = start; i < end; i++)
            {
                int value = (int)list[i].main.temp_min;
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }
 
        void getWeather(string city)
        {
            using(WebClient web = new WebClient())
            {
                string cityName = city;
                string url = string.Format("http://api.openweathermap.org/data/2.5/forecast?q={0}&appid={1}&units=" + (this.TempSymbol.Equals("°F") ? "imperial" : "metric"), cityName,appid);
                var json = web.DownloadString(url);

                var result = JsonConvert.DeserializeObject<WeatherInfo.Rootobject>(json);
                WeatherInfo.Rootobject output = result;
                
                //grad, drzava, trenutna temperatura
                label_CityName.Content = string.Format("{0}", output.city.name);
                label_CountryName.Content = string.Format("{0}", output.city.country);
                label_Temperature.Content = string.Format("{0} " + this.TempSymbol, Convert.ToInt64(Math.Floor(Convert.ToDouble(output.list[0].main.temp))));

                int hour = Int32.Parse(output.list[0].dt_txt.Split(' ')[1].Split(':')[0]);
     
                int secondDayStartIndex = (24 - hour) / 3 == 0 ? 8 : (24 - hour) / 3;

                // uzima vrednosti za datum
                label_1.Content = string.Format("{0}", output.list[0].dt_txt.Split(' ')[0]);
                label_2.Content = string.Format("{0}", output.list[secondDayStartIndex].dt_txt.Split(' ')[0]);
                label_3.Content = string.Format("{0}", output.list[secondDayStartIndex + 8].dt_txt.Split(' ')[0]);
                label_4.Content = string.Format("{0}", output.list[secondDayStartIndex + 16].dt_txt.Split(' ')[0]);
                label_5.Content = string.Format("{0}", output.list[secondDayStartIndex + 24].dt_txt.Split(' ')[0]);

                int firstDayMiddle = secondDayStartIndex / 2 >= 4 ? secondDayStartIndex / 2 : 0;
                int secondDayMiddle = secondDayStartIndex + 4;

                //Ikonice
                Img1.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[firstDayMiddle].weather[0].icon + ".png"));
                Img2.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[secondDayMiddle].weather[0].icon + ".png"));
                Img3.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[secondDayMiddle + 8].weather[0].icon + ".png"));
                Img4.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[secondDayMiddle + 16].weather[0].icon + ".png"));
                Img5.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[secondDayMiddle + 24].weather[0].icon + ".png"));
                
                //temperature
                label_1T.Content = string.Format("{0} " + this.TempSymbol, this.GetMaximumTemp(0, secondDayStartIndex, output.list));
                label_2T.Content = string.Format("{0} " + this.TempSymbol, this.GetMaximumTemp(secondDayStartIndex, secondDayStartIndex + 8, output.list));
                label_3T.Content = string.Format("{0} " + this.TempSymbol, this.GetMaximumTemp(secondDayStartIndex + 8, secondDayStartIndex + 16, output.list));
                label_4T.Content = string.Format("{0} " + this.TempSymbol, this.GetMaximumTemp(secondDayStartIndex + 16, secondDayStartIndex + 24, output.list));
                label_5T.Content = string.Format("{0} " + this.TempSymbol, this.GetMaximumTemp(secondDayStartIndex + 24, secondDayStartIndex + 32, output.list));
                
                //vetar
                label_1Wind.Content = string.Format("{0} " + (this.TempSymbol.Equals("°C") ? "m/s" : "mph"), output.list[firstDayMiddle].wind.speed); 
                label_2Wind.Content = string.Format("{0} " + (this.TempSymbol.Equals("°C") ? "m/s" : "mph"), output.list[secondDayMiddle].wind.speed); 
                label_3Wind.Content = string.Format("{0} " + (this.TempSymbol.Equals("°C") ? "m/s" : "mph"), output.list[secondDayMiddle + 8].wind.speed); 
                label_4Wind.Content = string.Format("{0} " + (this.TempSymbol.Equals("°C") ? "m/s" : "mph"), output.list[secondDayMiddle + 16].wind.speed); 
                label_5Wind.Content = string.Format("{0} " + (this.TempSymbol.Equals("°C") ? "m/s" : "mph"), output.list[secondDayMiddle + 24].wind.speed); 

                //pritisak
                label_1Pressure.Content = string.Format("{0} mbar", output.list[firstDayMiddle].main.pressure);
                label_2Pressure.Content = string.Format("{0} mbar", output.list[secondDayMiddle].main.pressure);
                label_3Pressure.Content = string.Format("{0} mbar", output.list[secondDayMiddle + 8].main.pressure);
                label_4Pressure.Content = string.Format("{0} mbar", output.list[secondDayMiddle + 16].main.pressure);
                label_5Pressure.Content = string.Format("{0} mbar", output.list[secondDayMiddle + 24].main.pressure);

            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            label_CityName.Content = countryText.Text;
            getWeather(countryText.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!this.TempSymbol.Equals("°C"))
            {
                this.TempSymbol = "°C";
                string chosenCity = countryText.Text;
                if (chosenCity.Equals(""))
                    getUserLocation();
                else
                    getWeather(chosenCity);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!this.TempSymbol.Equals("°F"))
            {
                this.TempSymbol = "°F";
                string chosenCity = countryText.Text;
                if (chosenCity.Equals(""))
                    getUserLocation();
                else
                    getWeather(chosenCity);
            }
        }
    }
}


