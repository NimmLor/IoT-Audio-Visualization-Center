using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Analyzer
{
    public class UdpDevice : ICommunicate
    {
        private string ip;
        private string deviceName;
        private bool enable;
        private int port;
        private int lines;
        private int smoothing;
        private bool smooth;
        public double range = 0.7;  // just take 50% of the fft data

        private UdpClient client;
        private static readonly HttpClient httpClient = new HttpClient();
        private Queue<List<Byte>> lastVals = new Queue<List<byte>>();

        private List<WebserverResponse> info;
        private List<Pattern> allPatterns = new List<Pattern>();
        private List<Pattern> visualizationPatterns = new List<Pattern>();
        private List<Pattern> regularPatterns = new List<Pattern>();
        private List<Pattern> twinklePatterns = new List<Pattern>();

        #region WebserverStatus
        public int power;
        public int brightness;
        public int currentPattern;
        public int animationSpeed;
        public int autoplay;
        public int autoplayDuration;
        string ColorString;
        #endregion

        #region Constructor
        public UdpDevice(string name, string ipaddress, int port, int bandCount, int smoothing)
        {
            deviceName = name;
            client = new UdpClient(ipaddress, port);
            ip = ipaddress;
            this.port = port;
            lines = bandCount;
            getWebserverInfo();
            this.smoothing = smoothing;
        }

        public UdpDevice(string name, string ipaddress, int port, int bandCount, int smoothing, string webResponse)
        {
            deviceName = name;
            client = new UdpClient(ipaddress, port);
            ip = ipaddress;
            this.port = port;
            lines = bandCount;
            try
            {
                Info = JsonConvert.DeserializeObject<WebserverResponse[]>(webResponse).ToList();
                if (info != null) EvaluateWebserverInfo(Info);
            }
            catch { }
            this.smoothing = smoothing;
        }

        public UdpDevice() { }
        #endregion

        #region Properties
        public bool Enable { get => enable; set => enable = value; }
        public string DeviceName { get => deviceName; set => deviceName = value; }
        public int Lines { get => lines; set => lines = value; }
        public bool Smooth { get { return smooth; } set { smooth = value; } }
        public int Smoothing { get => smoothing; set => smoothing = value; }
        public int Port { get => port; set => port = value; }
        public string Ip { get => ip; set => ip = value; }
        public string Hostname
        {
            get
            {
                return "";
                try
                {
                    IPHostEntry entry = Dns.GetHostEntry(ip);
                    if (entry != null)
                    {
                        return entry.HostName;
                    }
                }
                catch { }
                return null;
            }
        }
        public int BandCount { get { return lines; } }
        public List<WebserverResponse> Info { get => info; set => info = value; }
        public List<Pattern> AllPatterns { get => allPatterns; set => allPatterns = value; }
        public List<Pattern> VisualizationPatterns { get => visualizationPatterns; set => visualizationPatterns = value; }
        public List<Pattern> RegularPatterns { get => regularPatterns; set => regularPatterns = value; }
        public List<Pattern> TwinklePatterns { get => twinklePatterns; set => twinklePatterns = value; }
        #endregion

        #region Methods
        public void setBrightnessAsync(int b)
        {
            int brit = (int)(b * 2.55);
            if (brit >= 254) brit = 255;
            var values = new Dictionary<string, string> { { "value", (brit.ToString() )} };
            var content = new FormUrlEncodedContent(values);
            string addr = "http://" + ip + "/brightness";
            httpClient.PostAsync(addr, content);
        }

        public void setSpeedAsync(int b)
        {
            int brit = (int)(b * 2.55);
            if (brit >= 254) brit = 255;
            var values = new Dictionary<string, string> { { "value", (brit.ToString()) } };
            var content = new FormUrlEncodedContent(values);
            string addr = "http://" + ip + "/speed";
            httpClient.PostAsync(addr, content);
        }

        public void setPatternAsync(int patternNum)
        {
            var values = new Dictionary<string, string> { { "value", patternNum.ToString() } };
            var content = new FormUrlEncodedContent(values);
            string addr = "http://" + ip + "/pattern";
            httpClient.PostAsync(addr, content);
        }

        public void setPowerAsync(int val)
        {
            var values = new Dictionary<string, string> { { "value", val.ToString() } };
            var content = new FormUrlEncodedContent(values);
            string addr = "http://" + ip + "/power";
            httpClient.PostAsync(addr, content);
        }

        public bool getWebserverInfo()
        {
            if (deviceName.ToLower() == "meta") return false;
            string addr = "http://" + ip + "/all";
            string response = "";
            try
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        if (!this.Ready()) return false;
                        var bytes = webClient.DownloadData(addr);
                        UTF8Encoding utf8 = new UTF8Encoding();
                        response = utf8.GetString(bytes);
                    }
                }
                catch
                {
                    response = "";
                }

                if (String.IsNullOrEmpty(response)) return false;

                Info = JsonConvert.DeserializeObject<WebserverResponse[]>(response).ToList();
                if (info != null) EvaluateWebserverInfo(Info);
            }
            catch { Info = null; return false; }
            return true;
        }

        public bool modeOTA()
        {
            return switchMode("ota");
        }

        public bool modeAlexa()
        {
            return switchMode("alexa");
        }

        public bool modeReboot()
        {
            return switchMode("reboot");
        }

        public bool switchMode(string path)
        {
            string response = "";
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    response = webClient.DownloadString("http://" + ip + "/" + path);
                }
            }
            catch
            {
                response = "";
            }

            if (String.IsNullOrEmpty(response)) return false;
            return true;
        }

        public void EvaluateWebserverInfo(List<WebserverResponse> w)
        {
            power = (int)w[0].Value.Value.Integer;
            brightness = (int)w[1].Value.Value.Integer;
            currentPattern = (int)w[2].Value.Value.Integer;
            animationSpeed = (int)w[4].Value.Value.Integer;
            autoplay = (int)w[6].Value.Value.Integer;
            autoplayDuration = (int)w[7].Value.Value.Integer;
            ColorString = (string)w[9].Value.Value.String;
            try
            {
                if (w.Count >= 17) lines = (int)w[16].Value.Value.Integer;
            }
            catch (Exception ex)
            {
                try
                {
                    lines = Convert.ToInt32(w[16].Value.Value.String);
                }
                catch  { }
            }

            allPatterns.Clear();
            regularPatterns.Clear();
            visualizationPatterns.Clear();
            twinklePatterns.Clear();

            int idex = 0;
            foreach (string s in w[2].Options)
            {
                AllPatterns.Add(new Pattern
                {
                    id = idex,
                    name = s
                });
                if (s.ToLower().Contains("visual")) visualizationPatterns.Add(new Pattern
                {
                    id = idex,
                    name = s
                });
                else if (s.ToLower().Contains("twinkle")) twinklePatterns.Add(new Pattern
                {
                    id = idex,
                    name = s
                });
                else regularPatterns.Add(new Pattern { id = idex, name = s });
                idex++;
            }
        }
        #endregion

        #region ICommunication
        public bool Ready()
        {
            return MyUtils.IpReachable(ip);
        }

        public bool Send(List<byte> arr)
        {
            if (!enable) return true;
            var x = new byte[lines];
            for (int i = 0; i < lines; i++)
            {
                x[i] = arr[i];
                if (x[i] <= 0) x[i] = 1;
            }
            return client.Send(x, lines) > 0;
        }

        public bool Send(string s)
        {
            if (!enable) return true;
            return client.Send(Encoding.ASCII.GetBytes(s), Encoding.ASCII.GetBytes(s).Length) > 0;
        }

        public bool Start()
        {
            if (!enable) MyUtils.ap.AudioAvailable += new AudioProcessor.AudioAvailableEventHandler(this.UpdateValues);
            enable = true;
            return true;
        }

        public bool Stop()
        {
            if (enable) MyUtils.ap.AudioAvailable -= new AudioProcessor.AudioAvailableEventHandler(this.UpdateValues);
            enable = false;
            return true;
        }

        public void UpdateValues(object sender, AudioAvailableEventArgs e)
        {
            var newData = AudioProcessor.getSpectrumData(e.AudioAvailable, lines, range, MyUtils.sourceFactor);
            lastVals.Enqueue(newData);
            while (lastVals.Count > Smoothing)
            {
                lastVals.Dequeue();
            }
            if (!Smooth) Send(newData);
            else
            {
                Send(MyUtils.GetAverageSpectrum(lastVals, Smoothing));
            }
        }
        #endregion

        public UdpDevice DeepCopy()
        {
            UdpDevice deep = new UdpDevice(this.deviceName, this.ip, this.port, this.lines, this.smoothing, "");
            deep.smooth = this.smooth;
            return deep;
        }

        public struct Pattern
        {
            public int id;
            public string name;
        }
    }




    public class SerialComDevice : ICommunicate
    {
        public SerialPort Serial { get; set; }
        public int Lines { get => lines; set => lines = value; }

        private int lines;

        public SerialComDevice(SerialPort s)
        {
            Serial = s;
        }

        public SerialComDevice(string port, int baud, int bands)
        {
            Serial = new SerialPort(port, baud);
            Lines = bands;
        }

        public bool Send(List<byte> data)
        {
            if (Serial != null)
            {
                Serial.Write(data.ToArray(), 0, data.Count);
                return true;
            }
            return false;
        }

        public bool Send(string s)
        {
            if (Serial != null)
            {
                Serial.Write(s);
                return true;
            }
            return false;
        }

        public bool Start()
        {
            if (Serial == null) return false;
            if (!Serial.IsOpen)
            {
                Serial.Open();
            }
            return true;
        }

        public bool Stop()
        {
            if (Serial != null && Serial.IsOpen) Serial.Close();
            return true;
        }

        public void UpdateValues(object sender, AudioAvailableEventArgs e)
        {
            if (this.Ready()) Send(AudioProcessor.getSpectrumData(e.AudioAvailable, lines, MyUtils.sourceFactor));
        }

        public bool Ready()
        {
            return Serial != null && Serial.IsOpen;
        }
    }

    public class WpfUserControlDevice : ICommunicate
    {
        private int lines;
        private bool enable;
        private Spectrum spec;
        private Queue<List<Byte>> lastVals = new Queue<List<byte>>();
        private int smoothing;
        public double range = 0.7;
        public string name;

        public WpfUserControlDevice(int lines, Spectrum spec, string n)
        {
            this.lines = lines;
            this.spec = spec;
            enable = false;
            Smoothing = 10;
            name = n;
        }

        public bool Smooth { get { return (Smoothing > 0); } set { if (!value) smoothing = 0; } }
        public int Smoothing { get => smoothing; set => smoothing = value; }

        public bool Ready()
        {
            return enable;
        }

        public bool Send(List<byte> arr)
        {
            foreach (ProgressBar p in spec.bars)
            {
                int num = Convert.ToInt32(p.Name.Substring(1));
                //if (num <= arr.Count) p.Value = MyUtils.MapValue(0,255,0,100,arr[num - 1]);
                if (num <= arr.Count) p.Value = arr[num - 1];
            }
            return true;
        }

        public bool Send(string s)
        {
            return true;
        }

        public bool Start()
        {
            if (!enable && MyUtils.ap != null)
            {
                MyUtils.ap.AudioAvailable += new AudioProcessor.AudioAvailableEventHandler(this.UpdateValues);
                enable = true;
            }
            return true;
        }

        public bool Stop()
        {
            if (enable && MyUtils.ap != null)
            {
                try
                {
                    enable = false;
                    MyUtils.ap.AudioAvailable -= new AudioProcessor.AudioAvailableEventHandler(this.UpdateValues);
                    Send(new byte[lines].ToList());
                }
                catch { }
            }
            enable = false;
            return true;
        }

        public void UpdateValues(object sender, AudioAvailableEventArgs e)
        {
            var newData = AudioProcessor.getSpectrumData(e.AudioAvailable, lines, range, MyUtils.sourceFactor);
            lastVals.Enqueue(newData);
            while (lastVals.Count > Smoothing)
            {
                lastVals.Dequeue();
            }
            if (!Smooth) Send(newData);
            else
            {
                Send(MyUtils.GetAverageSpectrum(lastVals, Smoothing));
            }
        }
    }

    public class WpfDisplay : ICommunicate
    {
        private WpfVisualizer w;
        private double scale;
        private bool open = false;

        public WpfDisplay() { scale = 1; w = new WpfVisualizer(scale); }
        public WpfDisplay(double scale) { this.scale = scale; w = new WpfVisualizer(scale); }

        public WpfDisplay(WpfVisualizer w)
        {
            this.w = w;
            scale = 1;
        }
        public WpfDisplay(double scale, WpfVisualizer w)
        {
            this.w = w;
            this.scale = scale;
        }

        public double Scale { get => scale; set => scale = value; }

        public bool Ready()
        {
            return open;
            //return w != null && w.IsActive;
            //return MyUtils.IsWindowOpen<WpfVisualizer>();
        }

        public bool Send(List<byte> arr)
        {
            if (!this.Ready()) return false;
            List<int> n = new List<int>();
            foreach (byte b in arr) n.Add((byte)(MyUtils.MapValue(0, 255, 0, 100, ((double)b) * w.sldScale.Value)));
            if (n.Count >= 32)
            {
                #region PROGRESS_BAR
                w.c1.Value = n[0];
                w.c2.Value = n[1];
                w.c3.Value = n[2];
                w.c4.Value = n[3];
                w.c5.Value = n[4];
                w.c6.Value = n[5];
                w.c7.Value = n[6];
                w.c8.Value = n[7];
                w.c9.Value = n[8];
                w.c10.Value = n[9];
                w.c11.Value = n[10];
                w.c12.Value = n[11];
                w.c13.Value = n[12];
                w.c14.Value = n[13];
                w.c15.Value = n[14];
                w.c16.Value = n[15];
                w.c17.Value = n[16];
                w.c18.Value = n[17];
                w.c19.Value = n[18];
                w.c20.Value = n[19];
                w.c21.Value = n[20];
                w.c22.Value = n[21];
                w.c23.Value = n[22];
                w.c24.Value = n[23];
                w.c25.Value = n[24];
                w.c26.Value = n[25];
                w.c27.Value = n[26];
                w.c28.Value = n[27];
                w.c29.Value = n[28];
                w.c30.Value = n[29];
                w.c31.Value = n[30];
                w.c32.Value = n[31];
                #endregion
            }
            return true;
        }

        public bool Send(string s)
        {
            throw new NotImplementedException();
        }

        public bool Start()
        {
            if (!Ready()) w.Show();
            open = true;
            return true;
        }

        public bool Stop()
        {
            w.Close();
            open = false;
            return true;
        }

        public void UpdateValues(object sender, AudioAvailableEventArgs e)
        {
            if (this.Ready()) Send(AudioProcessor.getSpectrumData(e.AudioAvailable, 32, MyUtils.sourceFactor));
        }
    }
}
