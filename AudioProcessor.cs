using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace Analyzer
{
    public class AudioProcessor
    {
        // Event Pattern
        public delegate void AudioAvailableEventHandler(object sender, AudioAvailableEventArgs e);

        public event AudioAvailableEventHandler AudioAvailable;



        private bool _enable;               //enabled status
        private DispatcherTimer _t;         //timer that refreshes the display
        private float[] _fft;               //buffer for fft data
        private double _l, _r;         //progressbars for left and right channel intensity
        private WASAPIPROC _process;        //callback function to obtain data
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter
        private List<byte> _spectrumdata;   //spectrum data buffer

        private bool _initialized;          //initialized flag
        private int devindex;               //used device index

        



        //ctor

        public AudioProcessor(int deviceIndex, bool trimEnd = true)
        {

            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            _t = new DispatcherTimer();
            _t.Tick += _t_Tick;
            _t.Interval = TimeSpan.FromMilliseconds(20); //40hz refresh rate
            _t.IsEnabled = false;
            _l = 0;
            _r = 0;
            _r = ushort.MaxValue;
            _l = ushort.MaxValue;
            _process = new WASAPIPROC(Process);
            _spectrumdata = new List<byte>();

            devindex = deviceIndex;
            _initialized = false;
        }

        public void SwitchDevice(int deviceIndex)
        {
            devindex = deviceIndex;
            _hanctr = 4;
        }

        //flag for enabling and disabling program functionality
        public bool Enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
                if (value)
                {
                    if (!_initialized)
                    {
                        bool result = BassWasapi.BASS_WASAPI_Init(devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                        if (!result)
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            //MessageBox.Show(error.ToString());
                        }
                        else
                        {
                            _initialized = true;
                        }
                    }
                    BassWasapi.BASS_WASAPI_Start();
                }
                else BassWasapi.BASS_WASAPI_Stop(true);
                System.Threading.Thread.Sleep(50);
                _t.IsEnabled = value;
            }
        }



        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {
            // get fft data. Return value is -1 on error
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048);
            if (ret < 0) return;
            //_spectrumdata = getSpectrumData(_fft, _lines);

            //computes the spectrum data, the code is taken from a bass_wasapi sample.


            //if (DisplayEnable) _spectrum.Set(_spectrumdata);
            //OnAudioAvailable(_spectrumdata);
            OnAudioAvailable(_fft);
            _spectrumdata.Clear();


            int level = BassWasapi.BASS_WASAPI_GetLevel();
            _l = Utils.LowWord32(level);
            _r = Utils.HighWord32(level);
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output
            //so it doesn't make a gliched sound for long.
            if (_hanctr > 3)
            {
                _hanctr = 0;
                _l = 0;
                _r = 0;
                Free();
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                _initialized = false;
                Enable = true;
            }


        }



        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }

        public static List<byte> getSpectrumData(float[] fftData, int bands, double r, double factor)
        {
            int retBands = bands;
            bands = Convert.ToInt32(((double)bands) / r);
            return getSpectrumData(fftData, bands, factor).GetRange(0, retBands);
        }

        public static List<byte> getSpectrumData(float[] fftData, int bands, double factor)
        {
            float max = fftData.Max();
            float min = fftData.Min();
            List<byte> result = new List<byte>();


            int x, y;
            int b0 = 0;
            for (x = 0; x < bands; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (bands - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < fftData[1 + b0]) peak = fftData[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;
                result.Add((byte)y);
            }

            return applyFactor(result, factor);
        }

        public static List<byte> applyFactor(List<byte> x, double f)
        {
            List<byte> ret = new List<byte>();
            foreach (byte b in x)
            {
                int t = (int)(((double)b) * f);
                if (t < 0) t = 0;
                else if (t > 255) t = 255;
                ret.Add((byte)t);
            }
            return ret;
        }


        protected void OnAudioAvailable(float[] _toConv)
        {
            AudioAvailableEventHandler audioAvailable = AudioAvailable;
            if (audioAvailable != null) audioAvailable(this, new AudioAvailableEventArgs(_toConv));
            else
            {
                //throw new NullReferenceException("No Handler!");
            }
        }
    }

    public class AudioAvailableEventArgs : EventArgs
    {
        private float[] data;
        public AudioAvailableEventArgs(float[] fftData)
        {
            this.data = fftData;
        }
        public float[] AudioAvailable { get { return data; } }
    }
}