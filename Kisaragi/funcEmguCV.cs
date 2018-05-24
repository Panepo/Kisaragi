using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace funcEmguCV
{
    class imageProcess
    {
        static public Mat drawRect(Mat inputMat, Point pointStart, Point pointEnd)
        {
            Mat overlay = new Mat();
            Mat outputMat = new Mat();
            overlay = inputMat.Clone();
            outputMat = inputMat.Clone();

            Point rectPoint = new Point(pointStart.X < pointEnd.X ? pointStart.X : pointEnd.X, pointStart.Y < pointEnd.Y ? pointStart.Y : pointEnd.Y);
            Size rectSize = new Size( Math.Abs(pointStart.X - pointEnd.X), Math.Abs(pointStart.Y - pointEnd.Y));
            Rectangle rect = new Rectangle(rectPoint, rectSize);

            CvInvoke.Rectangle(overlay, rect, new Bgr(Color.Cyan).MCvScalar, 2);
            CvInvoke.AddWeighted(inputMat, 0.7, overlay, 0.3, 0, outputMat);
            CvInvoke.Rectangle(outputMat, rect, new Bgr(Color.Cyan).MCvScalar, 1);

            return outputMat;
        }

        static public Mat imgRotation(Mat inputMat, string angle)
        {
            Image<Bgr, Byte> temp = inputMat.ToImage<Bgr, Byte>();

            switch (angle)
            {
                case "Angle0":
                    break;
                case "Angle90":
                    temp = temp.Rotate(90, new Bgr(Color.Black));
                    break;
                case "Angle180":
                    temp = temp.Rotate(180, new Bgr(Color.Black));
                    break;
                case "Angle270":
                    temp = temp.Rotate(270, new Bgr(Color.Black));
                    break;
                default:
                    break;
            }

            return temp.Mat;
        }

        static public Mat imgResize(Mat inputMat, int width, int height)
        {
            Image<Bgr, Byte> temp = inputMat.ToImage<Bgr, Byte>();
            return temp.Resize(width, height, Emgu.CV.CvEnum.Inter.Linear).Mat;
        }
    }
}
