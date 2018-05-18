using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

using DirectShowLib;

using LeptonUVC;

using configCamera;
using funcFormat;
using funcEmguCV;

namespace Kisaragi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // system parameters
        private cameraControl webcam = null;
        private bool captureInProgress = false;
        private Mat currentFrame = new Mat();
        private cameraDevice[] webcams;
        private int webcamDevice = 0;
        private Stopwatch watch;
        private bool cameraFound = false;
        private int cameraWidth = 160;
        private int cameraHeight = 120;

        private Lepton lepton;
        private Lepton.Vid.LutBuffer userLut;

        // mouse event parameters
        private bool isClicked = false;
        private bool isRect = false;
        private System.Drawing.Point rectStart = new System.Drawing.Point();
        private System.Drawing.Point rectEnd = new System.Drawing.Point();

        public MainWindow()
        {
            InitializeComponent();

            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            webcams = new cameraDevice[systemCameras.Length];

            for (int i = 0; i < systemCameras.Length; i += 1)
            {
                webcams[i] = new cameraDevice(i, systemCameras[i].Name, systemCameras[i].ClassID);

                if (webcams[i].ToStringS() == "PureThermal 1")
                {
                    buttonCamera.IsEnabled = true;
                    webcamDevice = i;
                    cameraFound = true;
                    break;
                }
            }

            if (cameraFound)
            {
                buttonCamera.IsEnabled = false;
                textSystem.Text = "Thermal camera not found.";
            }

            watch = new Stopwatch();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            currentFrame = webcam.frameCapture();

            if (currentFrame != null)
            {
                watch.Reset();
                watch.Start();

                CvInvoke.CvtColor(currentFrame, currentFrame, Emgu.CV.CvEnum.ColorConversion.Rgb2Bgr);
                
                if (isRect)
                {
                    currentFrame = drawing.drawRect(currentFrame, rectStart, rectEnd);
                }

                imageBoxDisp.Source = formatTrans.ToBitmapSource(currentFrame);

                watch.Stop();
                textTime.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
            }
        }

        private void AssignColorMap(Lepton.Vid.LutBuffer userLut, string name)
        {
            userLut.bin = Palettes.Names[name];
        }

        private void buttonCamera_Click(object sender, RoutedEventArgs e)
        {
            if (webcam == null)
            {
                webcam = new cameraControl(webcamDevice, ProcessFrame);

                List<Lepton.Handle> devices = Lepton.GetDevices();
                Lepton.Handle device = devices[0];
                this.lepton = device.Open();
                this.lepton.vid.GetPcolorLut();
                foreach (object value in Enum.GetValues(typeof(Lepton.Vid.PcolorLut)))
                {
                    string[] parts = Enum.GetName(typeof(Lepton.Vid.PcolorLut), value).Split(new char[]
                    {
                    '_'
                    });
                    string result = "";
                    string sep = "";
                    for (int x = 0; x < parts.Length - 1; x++)
                    {
                        result = result + sep + parts[x][0].ToString() + parts[x].Substring(1).ToLower();
                        sep = " ";
                    }
                    this.comboBoxPalette.Items.Add(result);
                    this.comboBoxPalette.IsEnabled = true;
                }

                this.userLut = this.lepton.vid.GetUserLut();
                this.AssignColorMap(this.userLut, "Iron");
                this.lepton.vid.SetUserLut(this.userLut);
                this.lepton.vid.SetPcolorLut(Lepton.Vid.PcolorLut.USER_LUT);
            }

            if (captureInProgress)
            {
                buttonCamera.Content = "Camera Restart";
                webcam.stopTimer();
            }
            else
            {
                buttonCamera.Content = "Camera Stop";
                webcam.startTimer();
                buttonSave.IsEnabled = true;
            }
            captureInProgress = !captureInProgress;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (currentFrame == null)
            {
                textSystem.Text = "There is no image to save.";
            }
            else
            {
                if (captureInProgress)
                {
                    buttonCamera.Content = "Camera Restart";
                    webcam.stopTimer();
                    webcam.releaseCamera();
                    webcam = null;
                    captureInProgress = !captureInProgress;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "Cleffa_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                saveFileDialog.Filter = "PNG Image File (*.PNG)|*.PNG|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Interlace = PngInterlaceOption.Off;
                    encoder.Frames.Add(BitmapFrame.Create(formatTrans.ToBitmapSource(currentFrame)));
                    encoder.Save(stream);
                    stream.Close();
                    textSystem.Text = "The image was saved to your computer.";
                }
            }
        }

        private void comboBoxPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



        private void imageBoxDisp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isClicked = true;
            isRect = true;
            System.Windows.Point pointMouse = this.TranslatePoint(e.GetPosition(this), imageBoxDisp);
            double pointMouseDX = Math.Floor(pointMouse.X * cameraWidth / imageBoxDisp.ActualWidth);
            double pointMouseDY = Math.Floor(pointMouse.Y * cameraHeight / imageBoxDisp.ActualHeight);
            System.Drawing.Point pointMouseD = new System.Drawing.Point(Convert.ToInt32(pointMouseDX), Convert.ToInt32(pointMouseDY));

            rectStart = pointMouseD;
            rectEnd = pointMouseD;
            textRadioMin.Text = pointMouseD.X.ToString();
            textRadioMax.Text = pointMouseD.Y.ToString();
        }

        private void imageBoxDisp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;
        }

        private void imageBoxDisp_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isClicked)
            {
                System.Windows.Point pointMouse = this.TranslatePoint(e.GetPosition(this), imageBoxDisp);
                double pointMouseDX = Math.Floor(pointMouse.X * cameraWidth / imageBoxDisp.ActualWidth);
                double pointMouseDY = Math.Floor(pointMouse.Y * cameraHeight / imageBoxDisp.ActualHeight);
                rectEnd.X = Convert.ToInt32(pointMouseDX);
                rectEnd.Y = Convert.ToInt32(pointMouseDY);

                textRadioMin.Text = pointMouse.X.ToString();
                textRadioMax.Text = pointMouse.Y.ToString();
            }
        }
    }
}
