using System;
using Emgu.CV;
using System.Windows.Threading;

namespace configCamera
{
    class cameraControl
    {
        // =================================================================================
        // global variables
        // =================================================================================
        private Capture capture;
        private DispatcherTimer timer;
        public int cameraDevice = 0;

        private double paraBrightness;
        private double paraSharpness;
        private double paraContrast;

        // =================================================================================
        // constructor
        // =================================================================================
        public cameraControl(int webcamDevice, EventHandler myEventHandler)
        {
            cameraDevice = webcamDevice;
            capture = new Capture(webcamDevice);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            //paraBrightness = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness);
            //paraSharpness = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Sharpness);
            //paraContrast = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(myEventHandler);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        // =================================================================================
        // timer controller
        // =================================================================================
        public void stopTimer()
        {
            if (timer.IsEnabled)
                timer.Stop();
        }

        public void startTimer()
        {
            if (!timer.IsEnabled)
                timer.Start();
        }

        // =================================================================================
        // capture related function
        // =================================================================================
        public Mat frameCapture()
        {
            return capture.QueryFrame();
        }

        // =================================================================================
        // parameter settings
        // =================================================================================
        public void setDevice(int webcamDevice, EventHandler myEventHandler)
        {
            if (capture != null)
                capture.Dispose();

            capture = new Capture(webcamDevice);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            //capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(myEventHandler);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        public void setParameter(string parameter, double value)
        {
            if (capture != null)
            {
                switch (parameter)
                {
                    case "Brightness":
                        capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, value);
                        paraBrightness = value;
                        break;
                    case "Sharpness":
                        capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Sharpness, value);
                        paraSharpness = value;
                        break;
                    case "Contrast":
                        capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, value);
                        paraContrast = value;
                        break;
                }
            }
        }

        public double getParameter(string parameter)
        {
            if (capture != null)
            {
                switch (parameter)
                {
                    case "Brightness":
                        return capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness);
                    case "Sharpness":
                        return capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Sharpness);
                    case "Contrast":
                        return capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast);
                    default:
                        return 0;
                }
            }
            else
                return 0;
        }

        // =================================================================================
        // dispose
        // =================================================================================
        public void releaseCamera()
        {
            if (capture != null)
                capture.Dispose();
        }
    }
}
