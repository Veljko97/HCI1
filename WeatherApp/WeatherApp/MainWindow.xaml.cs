﻿using System;
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
        

        public MainWindow()
        {
            InitializeComponent();
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource =
              new BitmapImage(new Uri("C:\\Users\\marko\\Desktop\\background.jpg", UriKind.Absolute));
            this.Background = myBrush;
           
        }

        void getWeather()
        {
            using(WebClient web = new WebClient())
            {
                string cityName = countryText.Text;
                string url = string.Format("http://api.openweathermap.org/data/2.5/forecast?q={0}&appid={1}&units=metric", cityName,appid);
                var json = web.DownloadString(url);

                var result = JsonConvert.DeserializeObject<WeatherInfo.Rootobject>(json);
                WeatherInfo.Rootobject output = result;
                

                //grad, drzava, trenutna temperatura
                label_CityName.Content = string.Format("{0}", output.city.name);
                label_CountryName.Content = string.Format("{0}", output.city.country);
                label_Temperature.Content = string.Format("{0} \u00B0 C", output.list[0].main.temp);

                // uzima vrednosti za datum i vreme
                label_3.Content = string.Format("{0}", output.list[0].dt_txt);
                label_6.Content = string.Format("{0}", output.list[1].dt_txt);
                label_9.Content = string.Format("{0}", output.list[2].dt_txt);
                label_12.Content = string.Format("{0}", output.list[3].dt_txt);
                label_15.Content = string.Format("{0}", output.list[4].dt_txt);
                label_18.Content = string.Format("{0}", output.list[5].dt_txt);
                label_21.Content = string.Format("{0}", output.list[6].dt_txt);
                label_24.Content = string.Format("{0}", output.list[7].dt_txt);
                
                //temperature na svaka tri sata
                label_3T.Content = string.Format("{0} \u00B0 C", output.list[1].main.temp); //3 hours from now
                label_6T.Content = string.Format("{0} \u00B0 C", output.list[2].main.temp); //6 hours from now
                label_9T.Content = string.Format("{0} \u00B0 C", output.list[3].main.temp); //9 hours from now
                label_12T.Content = string.Format("{0} \u00B0 C", output.list[4].main.temp); //12 hours from now
                label_15T.Content = string.Format("{0} \u00B0 C", output.list[5].main.temp); //15 hours from now
                label_18T.Content = string.Format("{0} \u00B0 C", output.list[6].main.temp); //18 hours from now
                label_21T.Content = string.Format("{0} \u00B0 C", output.list[7].main.temp); //21 hours from now
                label_24T.Content = string.Format("{0} \u00B0 C", output.list[8].main.temp); //24 hours from now

                //vetar
                label_3Wind.Content = string.Format("{0} km/h", output.list[1].wind.speed); 
                label_6Wind.Content = string.Format("{0} km/h", output.list[2].wind.speed); 
                label_9Wind.Content = string.Format("{0} km/h", output.list[3].wind.speed); 
                label_12Wind.Content = string.Format("{0} km/h", output.list[4].wind.speed); 
                label_15Wind.Content = string.Format("{0} km/h", output.list[5].wind.speed); 
                label_18Wind.Content = string.Format("{0} km/h", output.list[6].wind.speed); 
                label_21Wind.Content = string.Format("{0} km/h", output.list[7].wind.speed); 
                label_24Wind.Content = string.Format("{0} km/h", output.list[8].wind.speed); 

                //pritisak
                label_3Pressure.Content = string.Format("{0} mbar", output.list[1].main.pressure);
                label_6Pressure.Content = string.Format("{0} mbar", output.list[2].main.pressure);
                label_9Pressure.Content = string.Format("{0} mbar", output.list[3].main.pressure);
                label_12Pressure.Content = string.Format("{0} mbar", output.list[4].main.pressure);
                label_15Pressure.Content = string.Format("{0} mbar", output.list[5].main.pressure);
                label_18Pressure.Content = string.Format("{0} mbar", output.list[6].main.pressure);
                label_21Pressure.Content = string.Format("{0} mbar", output.list[7].main.pressure);
                label_24Pressure.Content = string.Format("{0} mbar", output.list[8].main.pressure);


            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            label_CityName.Content = countryText.Text;
            getWeather();
        }

     
      /*  private void Label_CityName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            label_monT.Visibility = Visibility.Visible;
            label_tueT.Visibility = Visibility.Visible;
            label_wedT.Visibility = Visibility.Visible;
            label_thuT.Visibility = Visibility.Visible;
            label_friT.Visibility = Visibility.Visible;
            label_satT.Visibility = Visibility.Visible;
            label_sunT.Visibility = Visibility.Visible;

            label_mon.Visibility = Visibility.Visible;
            label_tue.Visibility = Visibility.Visible;
            label_wed.Visibility = Visibility.Visible;
            label_thu.Visibility = Visibility.Visible;
            label_fri.Visibility = Visibility.Visible;
            label_sat.Visibility = Visibility.Visible;
            label_sun.Visibility = Visibility.Visible;


            monImg.Visibility = Visibility.Visible;
            tueImg.Visibility = Visibility.Visible;
            wedImg.Visibility = Visibility.Visible;
            thuImg.Visibility = Visibility.Visible;
            friImg.Visibility = Visibility.Visible;
            satImg.Visibility = Visibility.Visible;
            sunImg.Visibility = Visibility.Visible;

        }
        */
    }
}

