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

           /* if (second.Position.Y > head.Position.Y && first.Position.Y > head.Position.Y) textbox.Text = "Both hands up!";
            else if (first.Position.Y > head.Position.Y) textbox.Text = "Left hands up!";
            else if (second.Position.Y > head.Position.Y) textbox.Text = "Right hands up!";
            else textbox.Text = "No hands up!";
            */

/*
            if ((first.Position.Y < second.Position.Y+30 && first.Position.Y > second.Position.Y - 30))
            {
                if (first.Position.Y >= head.Position.Y)
                {
                    textbox.Text = "YOLO!";

                }
                else
                {
                    textbox.Text = "Not YOLO";
                }

            }
            else
            {
               textbox.Text = "Hands not close";
            } 
            */
        }

        private void checkABCD(Joint left, Joint right, Joint head)
        {
            //Height of the triangle
            float height = head.Position.Y - right.Position.Y;

            //Length of the triangle
            float length = right.Position.X - head.Position.X;

            //Hypotenuse length
            float hypotenuse = (float) Math.Sqrt(Math.Pow(height, 2) + Math.Pow(length, 2));

            //If the right hand is on the left side of the body, return that the letter is not A
            if (length < 0) return;

            //Get the angle between from the head to hand
            double angle = Math.Atan((double)length / (double)height);

            //If the angle is more than 40
            if (!(angle > 0 && angle < 30))
            {
                return;
            }


            //See where the left hand is at


            //Height of the triangle
            height = head.Position.Y - left.Position.Y;

            //Length of the triangle
            length = head.Position.X - left.Position.X;

            //Hypotenuse length
            hypotenuse = (float)Math.Sqrt(Math.Pow(height, 2) + Math.Pow(length, 2));

            //If the left hand is on the right side of the body, return that the letter is not A
            if (length < 0) return;

            //Get the angle between from the head to hand
            if(height != 0)
            {
            angle = Math.Atan((double)length / (double)height);
            }
            else
            {
            angle = -1;
            }

            //If angle is between 0 and 40, we have a space/rest
            if (angle >= 0 && angle < 30)
            {
                textbox.Text = "Space/Rest";
            }
            else if (angle >= 30 && angle < 65)
            {
                textbox.Text = "A";
            }
            else
            {
                //If the left hand is near the same height as the head, it's a B
                if (left.Position.Y > head.Position.Y - 25 && left.Position.Y < head.Position.Y + 20)
                {
                    textbox.Text = "B";
                }
                 //Otherwise get the new angles
                else
                {
                    height = left.Position.Y - head.Position.Y;
                    length = head.Position.X - left.Position.X;

                    angle = Math.Atan((double)length / (double)height);

                    if (angle >= 30 && angle < 60)
                    {
                        textbox.Text = "C";
                    }
                    else
                    {
                        textbox.Text = "D";
                    }

                }





            }




           
        }
    }
}
