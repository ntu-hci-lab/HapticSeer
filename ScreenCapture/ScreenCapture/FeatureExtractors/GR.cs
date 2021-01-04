using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord;
using Emgu.CV; //or any C# opencv wrapper
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using ImageProcessModule.ProcessingClass;
using static ImageProcessModule.ImageProcessBase;

namespace ScreenCapture
{
    class GR : FeatureExtractors
    {
        private int cnt = 0;
        private Mat prevMat;
        public GR(string speedOutlet = null) : base()
        {

            prevMat = new Mat();
            ImageProcessesList.Add(new ImageProcess(0.2f, 0.8f, 0.2f, 0.8f, ImageScaleType.OriginalSize, 60));
            ImageProcessesList.Last().NewFrameArrivedEvent += OpticalFlowEvent;
        }

        private List<double> CalculateDirection(Mat angle, Mat magnitude)
        {
            float PI = Math.PI.To<float>();
            List<double> offsetSum = new List<double>();
            Mat matX = new Mat();
            Mat matY = new Mat();
            CvInvoke.PolarToCart(magnitude, angle, matX, matY, true);
            Emgu.CV.Structure.MCvScalar sumX = CvInvoke.Sum(matX);
            Emgu.CV.Structure.MCvScalar sumY = CvInvoke.Sum(matY);
            double x = sumX.V0;
            double y = sumY.V0;
            double scale = Math.Sqrt(x * x + y * y);
            x /= scale;
            y /= scale;
            offsetSum.Add(x);
            offsetSum.Add(y);
            return offsetSum;
        }
        private void OpticalFlowEvent(ImageProcess sender, Mat mat)
        {
            if (cnt == 0)
            {
                //cnt ==0, clone mat for previous mat

                prevMat = mat.Clone();
                cnt++;
                return;
            }


            Image<Emgu.CV.Structure.Gray, byte> prev_img = prevMat.ToImage<Emgu.CV.Structure.Gray, byte>();
            Image<Emgu.CV.Structure.Gray, byte> curr_img = mat.ToImage<Emgu.CV.Structure.Gray, byte>();

            Mat flow = new Mat(prev_img.Height,prev_img.Width,DepthType.Cv32F,2);

            CvInvoke.CalcOpticalFlowFarneback(prev_img, curr_img,flow, 0.5, 3, 15, 3, 6, 1.3, 0);

            Mat[] flow_parts = new Mat[2];
            flow_parts = flow.Split();         
            Mat magnitude = new Mat(), angle = new Mat(), magn_norm = new Mat();
            CvInvoke.CartToPolar(flow_parts[0], flow_parts[1], magnitude, angle,true);
            CvInvoke.Normalize(magnitude, magn_norm, 0.0, 1.0,NormType.MinMax);
            /*
            //start drawing
            float factor = (float)((1.0 / 360.0) * (180.0 / 255.0));
            Mat colorAngle = angle * factor;
            //angle *= ((1f / 360f) * (180f / 255f));
            //build hsv image
            Mat[]  _hsv= new Mat[3];
            Mat hsv = new Mat();
            Mat hsv8 = new Mat();
            Mat bgr = new Mat();
            _hsv[0] = colorAngle;
            _hsv[1] = Mat.Ones(colorAngle.Height,angle.Width,DepthType.Cv32F,1);
            _hsv[2] = magn_norm;
            VectorOfMat vm = new VectorOfMat(_hsv);
            CvInvoke.Merge(vm, hsv);
            hsv.ConvertTo(hsv8,DepthType.Cv8U, 255.0);
            CvInvoke.CvtColor(hsv8, bgr,ColorConversion.Hsv2Bgr);
            bgr.Save("opticalFlow.bmp");
            */
            List<double> offset =  CalculateDirection(angle, magn_norm);
            Console.WriteLine(offset[0] + " , " + offset[1]);


            //test direction
            /*if( Math.Abs(offset[0]) >Math.Abs(offset[1]))
            {

                if (offset[0] > 0 )
                {
                    Console.WriteLine("Right");
                }
                else
                {
                    Console.WriteLine("Left");
                }

            }
            else
            {

                if (offset[1] > 0)
                {
                    Console.WriteLine("Down");
                }
                else
                {
                    Console.WriteLine("Up");
                }

            }*/


            prevMat = mat.Clone();


        }
    }
}
