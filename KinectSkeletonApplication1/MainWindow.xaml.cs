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

                    ourFunction(currentSkeleton.Joints[JointType.HandLeft], currentSkeleton.Joints[JointType.HandRight], currentSkeleton.Joints[JointType.Head]);

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

        private void ourFunction(Joint first, Joint second, Joint head)
        {
            //first == Left Hand
            //second == Right Hand

            writeProperLetterOrNumber(first, second, head);

            /*
                        if (second.Position.Y > head.Position.Y && first.Position.Y > head.Position.Y) textbox.Text = "Both hands up!";
                        else if (first.Position.Y > head.Position.Y) textbox.Text = "Left hands up!";
                        else if (second.Position.Y > head.Position.Y) textbox.Text = "Right hands up!";
                        else textbox.Text = "No hands up!";
             */


        }

        private void writeProperLetterOrNumber(Joint left, Joint right, Joint head)
        {
            //Height of the triangle
            float height = head.Position.Y - right.Position.Y;
          
            //Length of the triangle
            float length = right.Position.X - head.Position.X;

            //Get the angle from the head to hand
            double angle = Math.Atan((double)length / (double)height);
          

            //If the angle is more than 40
            if (angle > 0 && angle < 0.40)
            {

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

               


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "Space/Rest";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    if (letters)
                        textbox.Text = "A";
                    else
                        textbox.Text = "1";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    if (letters)
                        textbox.Text = "B";
                    else
                        textbox.Text = "2";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    if (letters)
                        textbox.Text = "C";
                    else
                        textbox.Text = "3";
                }
                else
                {
                    if (letters)
                        textbox.Text = "D";
                    else
                        textbox.Text = "4";
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

               


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    if (letters)
                        textbox.Text = "G";
                    else
                        textbox.Text = "7";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    if (letters)
                        textbox.Text = "N";
                    else
                        textbox.Text = "N/A";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    if(letters)
                    textbox.Text = "S";
                    else
                        textbox.Text = "N/A";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    if(letters)
                    textbox.Text = "Cancel";
                    else
                        textbox.Text = "N/A";
                }
                else
                {
                    if(letters)
                    textbox.Text = "V";
                    else
                        textbox.Text = "N/A";
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

              


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    if(letters)
                    textbox.Text = "F";
                    else
                        textbox.Text = "6";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    if(letters)
                    textbox.Text = "M";
                    else
                        textbox.Text = "N/A";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    if(letters)
                    textbox.Text = "R";
                    else
                        textbox.Text = "N/A";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    if(letters)
                    textbox.Text = "Y";
                    else
                        textbox.Text = "N/A";
                 
                }
                else
                {     
                    textbox.Text = "J";
                    numeralonoff.Fill = Brushes.Red;
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

              


                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    if(letters)
                    textbox.Text = "E";
                    else
                        textbox.Text = "5";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    if(letters)
                    textbox.Text = "L";
                    else
                        textbox.Text = "N/A";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    if(letters)
                    textbox.Text = "Q";
                    else
                        textbox.Text = "N/A";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    if(letters)
                    textbox.Text = "U";
                    else
                        textbox.Text = "N/A";
                }
                else
                {
                    textbox.Text = "Numerals";
                    numeralonoff.Fill = Brushes.Green;
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

                //If angle is between 0 and 40, we have a space/rest
                if (oangle >= 0 && oangle < 0.40)
                {
                    textbox.Text = "Not a symbol";

                }
                else if (oangle >= 0.40 && oangle < 0.8)
                {
                    if(letters)
                    textbox.Text = "K";
                    else
                        textbox.Text = "0";
                }
                else if (oangle >= 0.8 && oangle < 1.4)
                {
                    if(letters)
                    textbox.Text = "P";
                    else
                        textbox.Text = "N/A";
                }
                else if ((oangle >= 1.4) || (oangle <= -1.2))
                {
                    if(letters)
                    textbox.Text = "T";
                    else
                        textbox.Text = "N/A";
                }
                else
                {
                    textbox.Text = "Not a symbol";
                }


            }



        }
    }
}
