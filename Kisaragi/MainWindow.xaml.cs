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
        //private Stopwatch watch;
        private bool cameraFound = false;
        private int cameraWidth = 160;
        private int cameraHeight = 120;

        private Lepton.Handle leptonDevice;
        private Lepton lepton;
        private Lepton.Rad.Roi leptonRoi;
        private Lepton.Rad.SpotmeterObjKelvin leptonSpotInfo;

        // mouse event parameters
        private bool isClicked = false;
        private System.Drawing.Point rectStart = new System.Drawing.Point();
        private System.Drawing.Point rectEnd = new System.Drawing.Point();

        public MainWindow()
        {
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = 20;

            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            webcams = new cameraDevice[systemCameras.Length];

            for (int i = 0; i < systemCameras.Length; i += 1)
            {
                webcams[i] = new cameraDevice(i, systemCameras[i].Name, systemCameras[i].ClassID);

                if (webcams[i].ToStringName() == "PureThermal 1")
                {
                    buttonCamera.IsEnabled = true;
                    webcamDevice = i;
                    cameraFound = true;
                    break;
                }
            }

            if (cameraFound == false)
            {
                buttonCamera.IsEnabled = false;
                buttonCamera.Content = "Thermal camera not found";
            }

            //watch = new Stopwatch();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            currentFrame = webcam.frameCapture();

            if (currentFrame != null)
            {
                //watch.Reset();
                //watch.Start();
                currentFrame = imageProcess.imgRotation(currentFrame, SystemInformation.ScreenOrientation.ToString());

                Mat frameDisp = imageProcess.drawRect(currentFrame, rectStart, rectEnd);
                imageBoxDisp.Source = formatTrans.ToBitmapSource(frameDisp);

                if (!isClicked)
                {
                    leptonSpotInfo = lepton.rad.GetSpotmeterObjInKelvinX100Checked();
                    textRadioMin.Text = cameraLepton.convertTemp(leptonSpotInfo.radSpotmeterMinValue);
                    textRadioMax.Text = cameraLepton.convertTemp(leptonSpotInfo.radSpotmeterMaxValue);
                    textRadioAvg.Text = cameraLepton.convertTemp(leptonSpotInfo.radSpotmeterValue);
                }
                
                //watch.Stop();
                //textTime.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
            }
        }

        private void buttonCamera_Click(object sender, RoutedEventArgs e)
        {
            if (webcam == null)
            {
                webcam = new cameraControl(webcamDevice, ProcessFrame);

                // lepton initial
                List<Lepton.Handle> devices = Lepton.GetDevices();
                leptonDevice = devices[0];
                lepton = leptonDevice.Open();

                // get color palette
                lepton.vid.GetPcolorLut();
                foreach (object value in Enum.GetValues(typeof(Lepton.Vid.PcolorLut)))
                {
                    string[] parts = Enum.GetName(typeof(Lepton.Vid.PcolorLut), value).Split(new char[] {'_'});
                    string result = "";
                    for (int i = 0; i < parts.Length - 1; i+= 1)
                    {
                        result += " " + parts[i][0].ToString() + parts[i].Substring(1).ToLower();
                    }

                    if (result != " User")
                        comboBoxPalette.Items.Add(result);
                }
                comboBoxPalette.IsEnabled = true;

                // set default color palette to fusion
                comboBoxPalette.SelectedIndex = 1;
                lepton.vid.SetPcolorLut((Lepton.Vid.PcolorLut)1);

                // radiometer initial
                leptonRoi = lepton.rad.GetSpotmeterRoi();
 
                rectStart.X = leptonRoi.startCol;
                rectStart.Y = leptonRoi.startRow;
                rectEnd.X = leptonRoi.endCol;
                rectEnd.Y = leptonRoi.endRow;
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
                //textSystem.Text = "There is no image to save.";
            }
            else
            {
                if (captureInProgress)
                {
                    buttonCamera.Content = "Camera Restart";
                    webcam.stopTimer();
                    captureInProgress = !captureInProgress;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "Anemone_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                saveFileDialog.Filter = "PNG Image File (*.PNG)|*.PNG|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Interlace = PngInterlaceOption.Off;
                    encoder.Frames.Add(BitmapFrame.Create(formatTrans.ToBitmapSource(imageProcess.imgResize(currentFrame,640,480))));
                    encoder.Save(stream);
                    stream.Close();
                    //textSystem.Text = "The image was saved to your computer.";
                }
            }
        }

        private void comboBoxPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = comboBoxPalette.SelectedIndex;
            lepton.vid.SetPcolorLut((Lepton.Vid.PcolorLut)idx);
        }

        private void imageBoxDisp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isClicked = true;
            System.Windows.Point pointMouse = this.TranslatePoint(e.GetPosition(this), imageBoxDisp);
            double pointMouseDX = Math.Floor(pointMouse.X * cameraWidth / imageBoxDisp.ActualWidth);
            double pointMouseDY = Math.Floor(pointMouse.Y * cameraHeight / imageBoxDisp.ActualHeight);
            System.Drawing.Point pointMouseD = new System.Drawing.Point(Convert.ToInt32(pointMouseDX), Convert.ToInt32(pointMouseDY));

            rectStart = pointMouseD;
            rectEnd = pointMouseD;
        }

        private void imageBoxDisp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;

            if (rectStart.X < rectEnd.X)
            {
                leptonRoi.startCol = (ushort)rectStart.X;
                leptonRoi.endCol = (ushort)rectEnd.X;
            }
            else
            {
                leptonRoi.startCol = (ushort)rectEnd.X;
                leptonRoi.endCol = (ushort)rectStart.X;
            }

            if (rectStart.Y < rectEnd.Y)
            {
                leptonRoi.startRow = (ushort)rectStart.Y;
                leptonRoi.endRow = (ushort)rectEnd.Y;
            }
            else
            {
                leptonRoi.startRow = (ushort)rectEnd.Y;
                leptonRoi.endRow = (ushort)rectStart.Y;
            }

            lepton.rad.SetSpotmeterRoi(leptonRoi);
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
            }
        }
    }
}
