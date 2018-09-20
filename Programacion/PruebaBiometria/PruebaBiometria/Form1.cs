using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GriauleFingerprintLibrary;
using GriauleFingerprintLibrary.Exceptions;

namespace PruebaBiometria
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            fingerPrint = new FingerprintCore();
            fingerPrint.onStatus += new StatusEventHandler(fingerPrint_onStatus);
            //fingerPrint.onFinger += new FingerEventHandler(fingerPrint_onFinger);
            fingerPrint.onImage += new ImageEventHandler(fingerPrint_onImage);
        }

        private FingerprintCore fingerPrint;
        private GriauleFingerprintLibrary.DataTypes.FingerprintRawImage rawImage;
        GriauleFingerprintLibrary.DataTypes.FingerprintTemplate _template;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void fingerPrint_onImage(object source, GriauleFingerprintLibrary.Events.ImageEventArgs ie)
        {
            rawImage = ie.RawImage;
            SetImage(ie.RawImage.Image);

            ExtractTemplate();
        }

        private void ExtractTemplate()
        {
            if (rawImage != null)
            {
                try
                {
                    _template = null;
                    fingerPrint.Extract(rawImage, ref _template);

                    SetQualityBar(_template.Quality);

                    DisplayImage(_template, false);
                }
                catch 
                {

                    SetQualityBar(-1);
                }
            }
        }

        private void DisplayImage(GriauleFingerprintLibrary.DataTypes.FingerprintTemplate template, bool identy)
        {
            IntPtr hdc = FingerprintCore.GetDC();
            IntPtr image = new IntPtr();

            if (identy)
            {
                fingerPrint.GetBiometricDisplay(template, rawImage, hdc, ref image, FingerprintConstants.GR_DEFAULT_CONTEXT);
                btnEliminar.Enabled = true;
            }
            else
            {
                fingerPrint.GetBiometricDisplay(template, rawImage, hdc, ref image, FingerprintConstants.GR_NO_CONTEXT);
                btnIdentificar.Enabled = true;
            }

            SetImage(Bitmap.FromHbitmap(image));

            FingerprintCore.ReleaseDC(hdc);
        }

        delegate void delSetQuality(int quality);

        private void SetQualityBar(int quality)
        {
            if (prgbQuality.InvokeRequired == true)
            {
                this.Invoke(new delSetQuality(SetQualityBar), new object[] { quality });
            }
            else
            {
               switch(quality)
                {
                    case 0:
                        {
                            //prgbQuality.ProgressBarColor = System.Drawing.Color.LightCoral;
                            prgbQuality.Value = prgbQuality.Maximum / 3;
                        }break;

                    case 1:
                        {
                            //prgbQuality.ProgressBarColor = System.Drawing.Color.YellowGreen;
                            prgbQuality.Value = (prgbQuality.Maximum / 3) * 2;
                        }break;

                    case 2:
                        {
                            //prgbQuality.ProgressBarColor = System.Drawing.Color.MediumSeaGreen;
                            prgbQuality.Value = prgbQuality.Maximum;
                        }break;

                    default:
                        {
                            //prgbQuality.ProgressBarColor = System.Drawing.Color.Gray;
                            prgbQuality.Value = 0;
                        }
                        break;
                }
            }
        }

        private delegate void delSetImage(Image img);
        void SetImage(Image img)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new delSetImage(SetImage), new object[] { img });
            }
            else
            {
                Bitmap bmp = new Bitmap(img, pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = bmp;
            }
        }

        void fingerPrint_onStatus(object source, GriauleFingerprintLibrary.Events.StatusEventArgs se)
        {
            if (se.StatusEventType == GriauleFingerprintLibrary.Events.StatusEventType.SENSOR_PLUG)
            {
                fingerPrint.StartCapture(source.ToString());
            }
            else
            {
                fingerPrint.StopCapture(source);
            }
        }
    }
}
