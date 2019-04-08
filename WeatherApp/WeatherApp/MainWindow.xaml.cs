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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using LiveCharts;
using LiveCharts.Wpf;
using Image = System.Drawing.Image;
using System.IO;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //api key c665da500cfb20637389a225a77ffa71
    // api call api.openweathermap.org/data/2.5/forecast?q={city name},{country code}


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const string appid = "c665da500cfb20637389a225a77ffa71";

        const string savedFile = "../../saved.bin";
        const string historyFile = "../../history.bin";

        #region PropertyChangedNotifier
        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public SeriesCollection SeriesCollection { get; set; }

        public string TempSymbol { get; set; }
       
        public Func<double, string> YFormatter { get; set; }

        private Border Selected { get; set; }

        public string[] _labels;

        public string[] Labels
        {
            get
            {
                return _labels;
            }
            set
            {
                if (value != _labels)
                {
                    _labels = value;
                    OnPropertyChanged("labels");
                }
            }
        }

        public ObservableCollection<string> Saved { get; set; }

        public ObservableCollection<string> History { get; set; }

        private WeatherInfo.Rootobject Data { get; set; }

        private int SecondDay { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource =
              new BitmapImage(new Uri("../../background.jpg", UriKind.Relative));
            this.Background = myBrush;
            readFromFile();
            readHistory();
            TempSymbol = "°C";
            Selected = null;
            YFormatter = value => string.Format("{0:0.00}" + TempSymbol, value);
            getUserLocation();
            Grid_MouseDown(boredr_0, new RoutedEventArgs());
            
            
        }

        public void saveToFile()
        {
            using(Stream stream = File.Open(savedFile,FileMode.Create))
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Saved);
            }
        }

        public void historyToFile()
        {
            using (Stream stream = File.Open(historyFile, FileMode.Create))
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, History);
            }
        }
        
        public void readFromFile()
        {
            try
            {
                using (Stream stream = File.Open(savedFile, FileMode.Open))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    Saved = (ObservableCollection<string>)formatter.Deserialize(stream);
                }
            }
            catch
            {
                Stream stream = File.Open(savedFile, FileMode.Create);
                stream.Close();
                Saved = new ObservableCollection<string>();
            }
        }

        public void readHistory()
        {
            try
            {
                using (Stream stream = File.Open(historyFile, FileMode.Open))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    History = (ObservableCollection<string>)formatter.Deserialize(stream);
                }
            }
            catch
            {
                Stream stream = File.Open(historyFile, FileMode.Create);
                stream.Close();
                History = new ObservableCollection<string>();
            }
        }

        void getUserLocation()
        {
            using (WebClient web = new WebClient())
            {
                string url = string.Format("https://api.ipdata.co/?api-key=cc18d6b6ad30a29071dc75de15debef6e74093fad097d41b93f76e1c");
                string jsonLocation;
                try
                {
                    jsonLocation = web.DownloadString(url);
                }
                catch
                {
                    return;
                }
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
        
        private List<double> getDayTemps(int begin, int end, WeatherInfo.List[] list)
        {
            List<double> temps = new List<double>();
            List<string> times = new List<string>();
            for (int i = begin; i < end; i++)
            {
                temps.Add(list[i].main.temp);
                times.Add(list[i].dt_txt.Split(' ')[1]);
            }
            Labels = times.ToArray();
            DataContext = this;
            return temps;
        }

        void getWeather(string city)
        {
            using(WebClient web = new WebClient())
            {
                string cityName = city;
                string url = string.Format("http://api.openweathermap.org/data/2.5/forecast?q={0}&appid={1}&units=" + (this.TempSymbol.Equals("°F") ? "imperial" : "metric"), cityName,appid);
                string json;
                try
                {
                    json = web.DownloadString(url);
                }
                catch
                {
                    label_error.Content = "City wasn't fount: " + cityName;
                    return;
                }
                History.Add(cityName);
                historyToFile();
                label_CityName.Content = cityName;
                var result = JsonConvert.DeserializeObject<WeatherInfo.Rootobject>(json);
                WeatherInfo.Rootobject output = result;
                Data = output;
                //grad, drzava, trenutna temperatura
                label_CityName.Content = string.Format("{0}", output.city.name);
                label_CountryName.Content = string.Format("{0}", output.city.country);
                label_Temperature.Content = string.Format("Temp: {0} " + this.TempSymbol, Convert.ToInt64(Math.Floor(Convert.ToDouble(output.list[0].main.temp))));

                int hour = Int32.Parse(output.list[0].dt_txt.Split(' ')[1].Split(':')[0]);
     
                int secondDayStartIndex = (24 - hour) / 3 == 0 ? 8 : (24 - hour) / 3;
                SecondDay = secondDayStartIndex;
                // uzima vrednosti za datum
                label_1.Content = string.Format("{0}", output.list[0].dt_txt.Split(' ')[0]);
                label_2.Content = string.Format("{0}", output.list[secondDayStartIndex].dt_txt.Split(' ')[0]);
                label_3.Content = string.Format("{0}", output.list[secondDayStartIndex + 8].dt_txt.Split(' ')[0]);
                label_4.Content = string.Format("{0}", output.list[secondDayStartIndex + 16].dt_txt.Split(' ')[0]);
                label_5.Content = string.Format("{0}", output.list[secondDayStartIndex + 24].dt_txt.Split(' ')[0]);

                int firstDayMiddle = secondDayStartIndex / 2 >= 4 ? secondDayStartIndex / 2 : 0;
                int secondDayMiddle = secondDayStartIndex + 4;

                //Ikonice
                Img0.Source = new BitmapImage(new Uri("http://openweathermap.org/img/w/" + output.list[0].weather[0].icon + ".png"));
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

        public Boolean findInSaved(string label)
        {
            Boolean flag = true;
            int i = 0;
            foreach (String item in Saved)
            {
                if (item.Equals(label))
                {
                    ListBox_saved.SelectedIndex = i;
                    flag = false;
                    break;
                }
                i++;
            }

            return flag;
        }

        public Boolean addItem(string label)
        {
            Boolean flag = findInSaved(label);
            if (flag)
            {
                Saved.Add(label);
            }
            getWeather(label);
            return flag;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            label_error.Content = "";
            if (countryText.Text.Equals(""))
            {
                getUserLocation();
            }
            else
            {
                getWeather(countryText.Text);
            }
            if (label_error.Content.ToString().Equals(""))
            {
                if (findInSaved(label_CityName.Content.ToString()))
                {
                    ListBox_saved.UnselectAll();
                }
            }
            Grid_MouseDown(boredr_0, new RoutedEventArgs());
            countryText.Text = "";
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            label_error.Content = "";
            Boolean toSave = false;
            if (countryText.Text.Equals(""))
            {
                getUserLocation();
                toSave = addItem(label_CityName.Content.ToString());
            }
            else
            {
                getWeather(countryText.Text);
                if (label_error.Content.ToString().Equals(""))
                {
                    toSave = addItem(countryText.Text);
                }
            }
            if (toSave)
            {
                saveToFile();
            }
            countryText.Text = "";
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {   
            if(ListBox_saved.SelectedValue == null)
            {
                return;
            }
            string item = ListBox_saved.SelectedValue.ToString();
            Saved.Remove(item);
            ListBox_saved.UnselectAll();
            saveToFile();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!this.TempSymbol.Equals("°C"))
            {
                this.TempSymbol = "°C";
                getWeather(label_CityName.Content.ToString());
            }
            if (Selected.BorderBrush != null)
            {
                setChart();
                chart_day.Update();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!this.TempSymbol.Equals("°F"))
            {
                this.TempSymbol = "°F";
                getWeather(label_CityName.Content.ToString());
            }
            if(Selected.BorderBrush != null)
            {
                setChart();
                chart_day.Update();
            }
        }

        private void setChart()
        {
            switch (Selected.Name)
            {
                case "boredr_0":
                    {
                        chart_day.Series = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Temp",
                                Values = new ChartValues<double>(getDayTemps(0,SecondDay,Data.list))
                            }
                        };
                        break;
                    }
                case "boredr_1":
                    {
                        chart_day.Series = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Temp",
                                Values = new ChartValues<double>(getDayTemps(SecondDay, SecondDay + 8, Data.list))
                            }
                        };
                        break;
                    }
                case "boredr_2":
                    {
                        chart_day.Series = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Temp",
                                Values = new ChartValues<double>(getDayTemps(SecondDay + 8, SecondDay + 16, Data.list))
                            }
                        };
                        break;
                    }
                case "boredr_3":
                    {
                        chart_day.Series = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Temp",
                                Values = new ChartValues<double>(getDayTemps(SecondDay + 16, SecondDay + 24, Data.list))
                            }
                        };
                        break;
                    }
                case "boredr_4":
                    {
                        chart_day.Series = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Temp",
                                Values = new ChartValues<double>(getDayTemps(SecondDay + 24, SecondDay + 32, Data.list))
                            }
                        };
                        break;
                    }
            }
        }

        private void Grid_MouseDown(object sender, RoutedEventArgs e )
        {
            Border b = (Border)sender;
            if (b.Name.Equals(Selected?.Name))
            {
                return;
            }
            System.Windows.Media.Brush brush = null;
            if (Selected?.BorderBrush != null)
            {
                brush = Selected.BorderBrush.Clone();
                brush.Opacity = 0;
                Selected.BorderBrush = brush;
            }
            Selected = b;
            brush = Selected.BorderBrush.Clone();
            brush.Opacity = 1;
            Selected.BorderBrush = brush;
            setChart();
            chart_day.Visibility = Visibility.Visible;
            chart_day.Update();
        }

        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = (Border)sender;
            if (b.Name.Equals(Selected.Name))
            {
                return;
            }
            var brush = b.BorderBrush.Clone();
            brush.Opacity = 0.5;
            b.BorderBrush = brush;
        }

        private void grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = (Border)sender;
            if (b.Name.Equals(Selected.Name))
            {
                return;
            }
            var brush = b.BorderBrush.Clone();
            brush.Opacity = 0;
            b.BorderBrush = brush;
        }

        private void ListBox_saved_Selected(object sender, RoutedEventArgs e)
        {
            if (ListBox_saved.SelectedItem == null)
            {
                button_remove.IsEnabled = false;
            }
            else
            {
                getWeather(ListBox_saved.SelectedValue.ToString());
                button_remove.IsEnabled = true;
            }
        }
    }
}


