using System;
using System.Linq;
using Emgu.CV; //or any C# opencv wrapper
using Emgu.CV.CvEnum;
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
            ImageProcessesList.Add(new ImageProcess(0.2f, 0.8f, 0.2f, 0.8f, ImageScaleType.OriginalSize, 30));
            ImageProcessesList.Last().NewFrameArrivedEvent += OpticalFlowEvent;
        }

        private void OpticalFlowEvent(ImageProcess sender, Mat mat)
        {
            if (cnt == 0)
            {

                prevMat = mat.Clone();
                cnt++;
                return;
            }
            if (cnt == 1)
            {
                cnt++;
                Image<Emgu.CV.Structure.Gray, byte> a = prevMat.ToImage<Emgu.CV.Structure.Gray, byte>();
                Image<Emgu.CV.Structure.Gray, byte> b = mat.ToImage<Emgu.CV.Structure.Gray, byte>();
                a.Save("a.bmp");
                b.Save("b.bmp");
                //prevMat = mat;
                return;
            }
            
            
            
            //Console.WriteLine("One loop begin");

            Image<Emgu.CV.Structure.Gray, byte> prev_img = prevMat.ToImage<Emgu.CV.Structure.Gray, byte>();
            Image<Emgu.CV.Structure.Gray, byte> curr_img = mat.ToImage<Emgu.CV.Structure.Gray, byte>();
            
            // one image array for each direction, which is x and y
            Image<Emgu.CV.Structure.Gray, float> flow_x;
            Image<Emgu.CV.Structure.Gray, float> flow_y;
            flow_x = new Image<Emgu.CV.Structure.Gray, float>(mat.Width, mat.Height);
            flow_y = new Image<Emgu.CV.Structure.Gray, float>(mat.Width, mat.Height);
            CvInvoke.CalcOpticalFlowFarneback(prev_img, curr_img, flow_x,flow_y, 0.5, 3, 15, 3, 6, 1.3, 0);
            

            long  sumX = 0;
            long  sumY = 0;
            for(int i=0;i<flow_x.Rows;++i)
            {
                for(int j=0;j<flow_x.Cols;++j)
                {
                    sumX += (int)flow_x.Data[i,j,0];
                    sumY += (int)flow_y.Data[i, j, 0];
                }
            }
            /*sumX /= mat.Width;
            sumY /= mat.Height;*/
            if(sumX==0&&sumY==0)
            {
                return;
                Console.WriteLine("No");
            }
            else if( Math.Abs(sumX)>Math.Abs(sumY))
            {
                Console.WriteLine("( " + sumX + ", " + sumY + " )");
                if ( sumX > 0 )
                {
                    Console.WriteLine("Right");
                }
                else
                {
                    Console.WriteLine("Left");
                }
                flow_x *= 50;
                flow_y *= 50;
                flow_x.Save("a.bmp");
                flow_y.Save("b.bmp");
            }
            else
            {
                Console.WriteLine("( " + sumX + ", " + sumY + " )");
                if (sumY > 0)
                {
                    Console.WriteLine("Down");
                }
                else
                {
                    Console.WriteLine("Up");
                }
                flow_x *= 50;
                flow_y *= 50;
                flow_x.Save("a.bmp");
                flow_y.Save("b.bmp");
            }

            
            //Console.WriteLine("( "+ sumX+", " + sumY+" )");



            prevMat = mat.Clone();


        }
    }
}
