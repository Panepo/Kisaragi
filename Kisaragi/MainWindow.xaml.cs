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

namespace Kisaragi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private cameraControl webcam = null;
        private bool captureInProgress = false;
        private Mat currentFrame = new Mat();
        private cameraDevice[] webcams;
        private int webcamDevice = 0;
        private Stopwatch watch;

        public MainWindow()
        {
            InitializeComponent();

            DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            webcams = new cameraDevice[systemCameras.Length];

            for (int i = 0; i < systemCameras.Length; i += 1)
            {
                webcams[i] = new cameraDevice(i, systemCameras[i].Name, systemCameras[i].ClassID);
                comboBoxPalette.Items.Add(webcams[i].ToStringS());
            }

            if (comboBoxPalette.Items.Count > 0)
            {
                comboBoxPalette.SelectedIndex = 0;
                buttonCamera.IsEnabled = true;
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
                imageBoxDisp.Source = funcFormat.ToBitmapSource(currentFrame);

 

                watch.Stop();
                textTime.Text = watch.Elapsed.TotalMilliseconds.ToString() + "ms";
            }
        }

        private void buttonCamera_Click(object sender, RoutedEventArgs e)
        {
            if (webcam == null)
            {
                webcam = new cameraControl(webcamDevice, ProcessFrame);

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
                    encoder.Frames.Add(BitmapFrame.Create(funcFormat.ToBitmapSource(currentFrame)));
                    encoder.Save(stream);
                    stream.Close();
                    textSystem.Text = "The image was saved to your computer.";
                }
            }
        }

        private void comboBoxPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            webcamDevice = comboBoxPalette.SelectedIndex;

            buttonCamera.Content = "Video Streaming";
            buttonSave.IsEnabled = false;
            imageBoxDisp.Source = null;

            if (captureInProgress)
            {
                webcam.stopTimer();
                captureInProgress = !captureInProgress;
            }

            if (webcam != null)
            {
                webcam.releaseCamera();
                webcam = null;
            }
        }
    }
}
