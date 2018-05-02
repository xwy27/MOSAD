using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;


// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace DailyQuery {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
            /// Set the titleBar to be transparent
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            /// Set the color style for the command button of titleBar 
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
        }

        private async Task<string> WeatherApi(string city) {
            string weather = "";

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            // Get the city code for the api
            string code = "101010100";
            XmlDocument cityCode = new XmlDocument();
            cityCode.LoadXml(System.IO.File.ReadAllText("Weather.xml"));
            foreach (IXmlNode node in cityCode.GetElementsByTagName("City")) {
                if ((string)node.Attributes[0].NodeValue == city) {
                    code = (string)node.Attributes[2].NodeValue;
                }
            }

            string uri = "http://www.weather.com.cn/data/sk/" + code + ".html";
            Uri requestUri = new Uri(uri);
            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                // Set encoding to 'UTF-8'
                Byte[] getByte1 = await httpResponse.Content.ReadAsByteArrayAsync();
                Encoding code1 = Encoding.GetEncoding("UTF-8");
                string result1 = code1.GetString(getByte1, 0, getByte1.Length);
                // Pharse the json data
                JsonReader reader = new JsonTextReader(new StringReader(result1));
                while (reader.Read()) {
                    if ((String)reader.Value == "city") {
                        weather += "城市: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "\n";
                    } else if ((String)reader.Value == "temp") {
                        weather += "温度: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "℃\n";
                    } else if ((String)reader.Value == "WD") {
                        weather += "风向: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "\n";
                    } else if ((String)reader.Value == "WS") {
                        weather += "风速: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "\n";
                    } else if ((String)reader.Value == "SD") {
                        weather += "相对湿度: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "\n";
                    } else if ((String)reader.Value == "time") {
                        weather += "时间: ";
                        reader.Read();
                        if (reader.Value != null)
                            weather += reader.Value + "\n";
                    }
                }
            } catch (Exception ex) {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return weather;
        }

        private async void GetWeather(object sender, RoutedEventArgs e) {
            Weather.Text = await WeatherApi(City.Text);
        }

        private void Rest(object sender, RoutedEventArgs e) {
            City.Text = "";
            Weather.Text = "";
        }

        private void IPRest(object sender, RoutedEventArgs e) {
            IP.Text = "";
            Location.Text = "";
        }

        private async Task<string> IPApi(string ip) {
            string location = "";

            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            string uri = "http://restapi.amap.com/v3/ip?ip=" + ip + "&output=xml&key=f16e5db9a637b16c70f9b2a410e2033b";
            Uri requestUri = new Uri(uri);
            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                // Set encoding to 'UTF-8'
                Byte[] getByte = await httpResponse.Content.ReadAsByteArrayAsync();
                Encoding code = Encoding.GetEncoding("UTF-8");
                string result = code.GetString(getByte, 0, getByte.Length);
                // Pharse the xml data
                XmlDocument document = new XmlDocument();
                document.LoadXml(result);

                var province = document.GetElementsByTagName("province");
                location += "省份: " + province[0].InnerText + "\n";

                var city = document.GetElementsByTagName("city");
                location += "城市: " + city[0].InnerText + "\n";

                var rectangle = document.GetElementsByTagName("rectangle");
                var temp = rectangle[0].InnerText.Split(';');
                location += "区域:\n\t" + temp[0] + "\n\t" + temp[1] + "\n";
            } catch (Exception ex) {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return location;
        }

        private async void GetIP(object sender, RoutedEventArgs e) {
            Location.Text = await IPApi(IP.Text);
        }
    }
}
