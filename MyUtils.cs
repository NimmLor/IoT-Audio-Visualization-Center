using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using MahApps.Metro;
using System.Threading;
using System.Diagnostics;

namespace Analyzer
{
    public static class MyUtils
    {
        public static AudioProcessor ap;
        public static string audioDevice;
        public static List<UdpDevice> UdpDevices = new List<UdpDevice>();

        public static double sourceFactor = 1.0;

        public static int MapValue(double a0, double a1, double b0, double b1, byte a)
        {
            double x = b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
            if (x < b0) x = b0;
            else if (x > b1) x = b1;
            return (int)x;
        }

        public static int MapValue(double a0, double a1, double b0, double b1, double a)
        {
            double x = b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
            if (x < b0) x = b0;
            else if (x > b1) x = b1;
            return (int)x;
        }

        public static Tuple<byte, byte, byte> HSL2RGB(double h, double sl, double l)
        {
            double v;
            double r, g, b;
            r = l;   // default to gray
            g = l;
            b = l;
            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;
                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            Tuple<byte,byte,byte> rgb = new Tuple<byte, byte, byte>(Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f), Convert.ToByte(b * 255.0f));
            return rgb;
        }

        public static List<Byte> GetAverageSpectrum(Queue<List<Byte>> toSmooth, int Smoothing)
        {
            byte[] result = new byte[toSmooth.First().Count];
            int size = toSmooth.Count;
            foreach (List<Byte> measure in toSmooth)
            {
                for (int i = 0; i < measure.Count; i++)
                {
                    int x = Convert.ToByte(measure[i] / Smoothing);
                    if ((result[i] + ((int)x)) > 255) result[i] = 255; else result[i] += (byte)x;
                }
            }
            return result.ToList();
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static bool ValidateIp(string ip)
        {
            IPAddress ipd;
            return IPAddress.TryParse(ip, out ipd);
        }

        public static bool IpReachable(string ip)
        {
            Ping pingSender = new Ping();
            string data = "ping";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingOptions options = new PingOptions(64, true);
            PingReply reply = pingSender.Send(ip, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
                return true;
            else return false;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void SaveToProperties(SaveObject toSave)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SaveObject));
            StringWriter sw = new StringWriter();
            ser.Serialize(sw, toSave);
            string xml = sw.ToString();
            Properties.Settings.Default.Configuration = xml;
            Properties.Settings.Default.Save();
            
        }

        public static SaveObject RetrieveSettings()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.Configuration))
            {
                XmlSerializer ser = new XmlSerializer(typeof(SaveObject));
                try
                {
                    using (TextReader reader = new StringReader(Properties.Settings.Default.Configuration))
                    {
                        return (SaveObject)ser.Deserialize(reader);
                    }
                }
                catch { }
            }
            return null;
        }

        public static void EnableAll()
        {
            foreach (UdpDevice u in UdpDevices)
            {
                u.setPowerAsync(1);
                u.Start();
            }
        }

        public static void DisableAll()
        {
            foreach (UdpDevice u in UdpDevices)
            {
                u.setPowerAsync(0);
                u.Stop();
            }
        }

        public static void SetDefaultTheme()
        {
            Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent("Blue"),
                                            ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1

            //var x = ThemeManager.GetAccent("Blue");
            var g = new LinearGradientBrush();
            //g.StartPoint = new Point(0.5, 0);
            //g.EndPoint = new Point(0.5, 1);
            g.StartPoint = new Point(1, 1);
            g.EndPoint = new Point(0, 0);
            g.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
            g.GradientStops.Add(new GradientStop(Colors.Orange, 0.17));
            g.GradientStops.Add(new GradientStop(Colors.Yellow, 0.33));
            g.GradientStops.Add(new GradientStop(Colors.Green, 0.5));
            g.GradientStops.Add(new GradientStop(Colors.Blue, 0.67));
            g.GradientStops.Add(new GradientStop(Colors.Indigo, 0.83));
            g.GradientStops.Add(new GradientStop(Colors.Violet, 1.0));
            var x = new ResourceDictionary();
            x.Add("RainbowBorderBrush", g);
            ThemeManager.AddAccent("RainbowBorderBrush", x);
            Application.Current.Resources.Add("RainbowBorderBrush", g);
        }

        public static void SetTheme(string accent, string appTheme)
        {
            try
            {
                Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current,
                                            ThemeManager.GetAccent(accent),
                                                ThemeManager.GetAppTheme(appTheme)); // or appStyle.Item1
            }
            catch
            {
                SetDefaultTheme();
            }
        }

        public static void SwitchDeviceFromString(string s)
        {
            int d = 0;
            bool success = int.TryParse(s.Split(' ')[0], out d);
            if (success)
            {
                if (MyUtils.ap == null)
                {
                    MyUtils.ap = new AudioProcessor(d);
                    MyUtils.ap.Enable = true;
                }
                else MyUtils.ap.SwitchDevice(d);
                MyUtils.audioDevice = s;
            }
        }

        public static void OnProcessExit(object sender, EventArgs e)
        {
            SaveObject s = new SaveObject(MyUtils.UdpDevices);
            s.audioDevice = MyUtils.audioDevice;

            SaveToProperties(s);
        }


        #region NetworkScanner
        public static List<NetworkDevice> NetworkDevices = new List<NetworkDevice>();

        public class NetworkDevice
        {
            public int ping { get; set; }
            public string DisplayPing { get { if (ping >= 0) return ping.ToString(); else return ""; } set { DisplayPing = value; } }
            public bool isLedDevice { get; set; }
            public string IP { get; set; }
            public string Hostname { get; set; }
            public string allResponse { get; set; }

            private ManualResetEvent _doneEvent;

            public NetworkDevice() { }
            public NetworkDevice(string ipaddr, ManualResetEvent doneEvent)
            {
                IP = ipaddr;
                ping = -2;
                isLedDevice = false;
                _doneEvent = doneEvent;
            }

            public static string returnWebserverInfo(string ipaddr)
            {
                string addr = "http://" + ipaddr + "/all";
                string response = "";
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        response = webClient.DownloadString(addr);
                    }
                    if (String.IsNullOrEmpty(response)) return "";
                    Debug.WriteLine("Got Response from: " + ipaddr);
                    return response;
                }
                catch { return ""; }
            }

            public int PingDevice()
            {
                Ping p = new Ping();
                PingReply r;
                r = p.Send(this.IP);
                if (!(r.Status == IPStatus.Success))
                {
                    allResponse = "";
                    return -1;
                }
                return Convert.ToInt32(r.RoundtripTime);
            }

            public static Task<PingReply> PingAsync(string address)
            {
                var tcs = new TaskCompletionSource<PingReply>();
                Ping ping = new Ping();
                ping.PingCompleted += (obj, sender) =>
                {
                    tcs.SetResult(sender.Reply);
                };
                ping.SendAsync(address, new object());
                return tcs.Task;
            }

            public void ThreadPoolCallback(Object threadContext)
            {
                ping = PingDevice(); if (ping < 0) return;
                allResponse = returnWebserverInfo(this.IP);
                if (!String.IsNullOrEmpty(allResponse))
                {
                    if (allResponse.Contains("{\"name\":\"power\",\"label\":\"Power\",\"type\":\"Boolean\"")) isLedDevice = true;
                }
                else { }
                try
                {
                    var hr = Dns.GetHostEntry(IP);
                    Hostname = hr.HostName;
                }
                catch { 
                    Hostname = "";
                    try     // cheesy way to retrieve hostname without parsing the json
                    {
                        if(allResponse.Contains("hostname"))
                        {
                            int p = allResponse.LastIndexOf("hostname");
                            string h = "";
                            for(int i = p;i<allResponse.Length;i++)
                            {
                                if (allResponse.Substring(i - 4, 5) == "value")
                                {
                                    h = allResponse.Substring(i+3);
                                    h = h.Substring(0, h.IndexOf('"'));
                                }
                            }
                            if (!String.IsNullOrEmpty(h)) Hostname = h;
                        }
                    }
                    catch { 
                        Hostname = "";
                    }
                }
                _doneEvent.Set();
            }
        }
        #endregion
    }



    public class SaveObject
    {
        public List<UdpDevice> udps;
        public string audioDevice;

        public SaveObject(List<UdpDevice> udps)
        {
            this.udps = udps;
        }
        public SaveObject() { }
    }

    public partial class WebserverResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Value? Value { get; set; }

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public long? Min { get; set; }

        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public long? Max { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Options { get; set; }
    }

    public partial struct Value
    {
        public long? Integer;
        public string String;

        public static implicit operator Value(long Integer) => new Value { Integer = Integer };
        public static implicit operator Value(string String) => new Value { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Value) || t == typeof(Value?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new Value { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Value { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type Value");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Value)untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type Value");
        }

        public static readonly ValueConverter Singleton = new ValueConverter();
    }
}
