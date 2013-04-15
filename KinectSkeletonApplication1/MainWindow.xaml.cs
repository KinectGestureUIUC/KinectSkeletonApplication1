using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Linq;

namespace KinectSkeletonApplication1
{
    public partial class MainWindow : Window
    {
        bool letters = true;
        //Instantiate the Kinect runtime. Required to initialize the device.
        //IMPORTANT NOTE: You can pass the device ID here, in case more than one Kinect device is connected.
        KinectSensor sensor = KinectSensor.KinectSensors[0];
        byte[] pixelData;
        Skeleton[] skeletons;

        public MainWindow()
        {
            InitializeComponent();

            //Runtime initialization is handled when the window is opened. When the window
            //is closed, the runtime MUST be unitialized.
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

            sensor.ColorStream.Enable();
            sensor.SkeletonStream.Enable();
        }

        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (SkeletonFrame SFrame = e.OpenSkeletonFrame())
            {
                if (SFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    skeletons = new Skeleton[SFrame.SkeletonArrayLength];
                    SFrame.CopySkeletonDataTo(skeletons);
                    receivedData = true;
                }
            }

            if (receivedData)
            {

                Skeleton currentSkeleton = (from s in skeletons
                                            where s.TrackingState == SkeletonTrackingState.Tracked
                                            select s).FirstOrDefault();

                if (currentSkeleton != null)
                {

                    textbox.IsEnabled = true;



                    /*SetEllipsePosition(head, currentSkeleton.Joints[JointType.Head]);
                    SetEllipsePosition(leftHand, currentSkeleton.Joints[JointType.HandLeft]);
                    SetEllipsePosition(rightHand, currentSkeleton.Joints[JointType.HandRight]);
                    */

                    inRange(currentSkeleton.Joints[JointType.HandLeft], currentSkeleton.Joints[JointType.HandRight], currentSkeleton.Joints[JointType.Head]);

                }
            }
        }


        //This method is used to position the ellipses on the canvas
        //according to correct movements of the tracked joints.

        //IMPORTANT NOTE: Code for vector scaling was imported from the Coding4Fun Kinect Toolkit
        //available here: http://c4fkinect.codeplex.com/
        //I only used this part to avoid adding an extra reference.
        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {
            Canvas.SetLeft(ellipse, joint.Position.X + 10);
            Canvas.SetTop(ellipse, joint.Position.Y + 10);
        }

        private float ScaleVector(int length, float position)
        {
            float value = (((((float)length) / 1f) / 2f) * position) + (length / 2);
            if (value > length)
            {
                return (float)length;
            }
            if (value < 0f)
            {
                return 0f;
            }
            return value;
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            sensor.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            sensor.SkeletonFrameReady += runtime_SkeletonFrameReady;
            sensor.ColorFrameReady += runtime_VideoFrameReady;
            sensor.Start();
        }

        void runtime_VideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (ColorImageFrame CFrame = e.OpenColorImageFrame())
            {
                if (CFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    pixelData = new byte[CFrame.PixelDataLength];
                    CFrame.CopyPixelDataTo(pixelData);
                    receivedData = true;
                }
            }

            if (receivedData)
            {
                BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Bgr32, null, pixelData, 640 * 4);

                videoImage.Source = source;
            }
        }

        private void inRange(Joint first, Joint second, Joint head)
        {
            //first == Left Hand
            //second == Right Hand

            checkABCD(first, second, head);
            //checkFMRYJ(first, second, head);
            //checkELQUnums(first, second, head);
            //checkKPT(first, second, head);
            /*
                        if (second.Position.Y > head.Position.Y && first.Position.Y > head.Position.Y) textbox.Text = "Both hands up!";
                        else if (first.Position.Y > head.Position.Y) textbox.Text = "Left hands up!";
                        else if (second.Position.Y > head.Position.Y) textbox.Text = "Right hands up!";
                        else textbox.Text = "No hands up!";
             */


        }

        private void checkABCD(Joint left, Joint right, Joint head)
        {
            //Height of the triangle
            float height = head.Position.Y - right.Position.Y;
            heightbox.Text = "Height: " + height;
            //Length of the triangle
            float length = right.Position.X - head.Position.X;
            lengthbox.Text = "Length: " + length;

            //Hypotenuse length
            float hypotenuse = (float)Math.Sqrt(Math.Pow(height, 2) + Math.Pow(length, 2));



            //Get the angle between from the head to hand
            double angle = Math.Atan((double)length / (double)height);
            righthandanglebox.Text = "Right: " + angle;

            //If the angle is more than 40
            if (angle > 0 && angle < 0.40)
            {

                //See where the left hand is at


                //Height of the triangle
                height = head.Position.Y - left.Position.Y;

                //Length of the triangle
                length = head.Position.X - left.Position.X;

                //If the left hand is on the right side of the body, return that the letter is not A


                double oangle;
                //Get the angle between from the head to hand
                if (height != 0)
                {
                    oangle = Math.Atan((double)length / (double)height);
                }
                else
                {
                    oangle = -1;
                }

                lefthandanglebox.Text = "Left: " + oangle;


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "Space/Rest";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    textbox.Text = "A";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    textbox.Text = "B";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    textbox.Text = "C";
                }
                else
                {
                    textbox.Text = "D";
                }

            }//End if statement
            else if (angle >= 0.40 && angle <= 0.80)
            {
                //See where the left hand is at


                //Height of the triangle
                height = head.Position.Y - left.Position.Y;

                //Length of the triangle
                length = head.Position.X - left.Position.X;

                //If the left hand is on the right side of the body, return that the letter is not A


                double oangle;
                //Get the angle between from the head to hand
                if (height != 0)
                {
                    oangle = Math.Atan((double)length / (double)height);
                }
                else
                {
                    oangle = -1;
                }

                lefthandanglebox.Text = "Left: " + oangle;


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "G";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    textbox.Text = "N";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    textbox.Text = "S";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    textbox.Text = "Cancel";
                }
                else
                {
                    textbox.Text = "V";
                }

            }
            else if (angle >= 0.80 && angle < 1.4)
            {
                //See where the left hand is at


                //Height of the triangle
                height = head.Position.Y - left.Position.Y;

                //Length of the triangle
                length = head.Position.X - left.Position.X;

                //If the left hand is on the right side of the body, return that the letter is not A


                double oangle;
                //Get the angle between from the head to hand
                if (height != 0)
                {
                    oangle = Math.Atan((double)length / (double)height);
                }
                else
                {
                    oangle = -1;
                }

                lefthandanglebox.Text = "Left: " + oangle;


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "F";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    textbox.Text = "M";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    textbox.Text = "R";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    textbox.Text = "Y";
                 
                }
                else
                {
                    textbox.Text = "J";
                    letters = true;
                }
            }
            else if((angle >= 1.4) || (angle <= -1.2))
            {
                //See where the left hand is at


                //Height of the triangle
                height = head.Position.Y - left.Position.Y;

                //Length of the triangle
                length = head.Position.X - left.Position.X;

                //If the left hand is on the right side of the body, return that the letter is not A


                double oangle = 0;
                //Get the angle between from the head to hand
                if (height != 0)
                {
                    oangle = Math.Atan((double)length / (double)height);
                }
                else
                {
                    oangle = -1;
                }

                lefthandanglebox.Text = "Left: " + oangle;


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "E";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    textbox.Text = "L";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    textbox.Text = "Q";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    textbox.Text = "U";
                }
                else
                {
                    textbox.Text = "Numerals";
                    letters = false;
                }


            }

            else
            {
                //See where the left hand is at


                //Height of the triangle
                height = head.Position.Y - left.Position.Y;

                //Length of the triangle
                length = head.Position.X - left.Position.X;

                //If the left hand is on the right side of the body, return that the letter is not A


                double oangle = 0;
                //Get the angle between from the head to hand
                if (height != 0)
                {
                    oangle = Math.Atan((double)length / (double)height);
                }
                else
                {
                    oangle = -1;
                }

                lefthandanglebox.Text = "Left: " + oangle;


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "Not a symbol";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    textbox.Text = "K";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    textbox.Text = "P";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    textbox.Text = "T";
                }
                else
                {
                    textbox.Text = "Not a symbol";
                }


            }



        }
    }
}
