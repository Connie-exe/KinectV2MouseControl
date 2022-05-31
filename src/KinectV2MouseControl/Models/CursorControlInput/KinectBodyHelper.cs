using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Threading;

namespace KinectV2MouseControl
{
    public static class KinectBodyHelper
    {       
        const double FOOT_LIFT_Z_DISTANCE = 0.15f;//0.15f
        const double GESTURE_Y_OFFSET = -0.65f;
        const double GESTURE_X_OFFSET = 0.185f;
        const double GESTURE_Z_OFFSET = 0.185f;

        public static double[,] matrix_pos_FootLeft;
        public static double[,] matrix_pos_FootRight;

        static MVector2[] gestureOffsets = new MVector2[] {
           new MVector2(GESTURE_X_OFFSET, GESTURE_Y_OFFSET),
           new MVector2(-GESTURE_X_OFFSET, GESTURE_Y_OFFSET)
        };
        static MVector3[] depthOffset = new MVector3[] {
           new MVector3(GESTURE_X_OFFSET, GESTURE_Y_OFFSET, GESTURE_Z_OFFSET),
           new MVector3(-GESTURE_X_OFFSET, GESTURE_Y_OFFSET, GESTURE_Z_OFFSET)
        };

        public static bool IsFootLiftForward(this Body body, bool isLeft, bool isLeft2)
        {            
            return body.Joints[isLeft ? JointType.FootLeft : JointType.FootRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z < -FOOT_LIFT_Z_DISTANCE;            
        }
        
        //public static void ConsoleWrite()
        //{
        //    Console.WriteLine()
        //}
        

        public static HandState GetFeetState(this Body body, bool isLeft)
        {
            return isLeft ? body.HandLeftState : body.HandRightState;
        }

        //public static MVector2 GetHandRelativePosition(this Body body, bool isLeft, bool isLeft2)
        public static CameraSpacePoint GetFeetRelativePosition(this Body body, bool isLeft)
        {
            CameraSpacePoint footPosLeft = body.Joints[JointType.FootRight].Position;
            CameraSpacePoint footPosRight = body.Joints[JointType.FootRight].Position;
            //Console.WriteLine("LeftPos X: " + footPosLeft.X.ToString() + " LeftPos Y: " + footPosLeft.Y.ToString() + " LeftPos Z: " + footPosLeft.Z.ToString());
            //Console.WriteLine("RightPos X: " + footPosRight.X.ToString() + " RightPos Y: " + footPosRight.Y.ToString() + " RightPos Z: " + footPosRight.Z.ToString());

            int i = 0;
            int j = 0;
            //int fila;
            //double[,] matrix_pos_FootLeft;
            if (i <= 20)
            {
                matrix_pos_FootLeft[i, 0] = footPosLeft.X;
                matrix_pos_FootLeft[i, 1] = footPosLeft.Y;
                matrix_pos_FootLeft[i, 2] = footPosLeft.Z;
                i++;

                Console.WriteLine(matrix_pos_FootLeft[i, 0]);
                Console.WriteLine(matrix_pos_FootLeft[i, 1]);
                Console.WriteLine(matrix_pos_FootLeft[i, 2]);
            }
            else
            {
                i = 0;
            }
            if(j < 20)
            {
                matrix_pos_FootRight[j, 0] = footPosRight.X;
                matrix_pos_FootRight[j, 1] = footPosRight.Y;
                matrix_pos_FootRight[j, 2] = footPosRight.Z;
                j++;

                Console.WriteLine(matrix_pos_FootRight[i, 0]);
                Console.WriteLine(matrix_pos_FootRight[i, 1]);
                Console.WriteLine(matrix_pos_FootRight[i, 2]);
            }
            else
            {
                j = 0;
            }


            return isLeft ? body.Joints[JointType.FootLeft].Position : body.Joints[JointType.FootRight].Position;

            //CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;

        }

        public static MVector2 ToMVector2(this CameraSpacePoint jointPoint)
        {
            return new MVector2(jointPoint.X, jointPoint.Y);
        } 
        
        //public static void MatrixFiller()
        //{
            
        //}
    }
}
