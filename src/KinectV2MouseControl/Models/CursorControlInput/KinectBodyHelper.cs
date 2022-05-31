using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Threading;

namespace KinectV2MouseControl
{
    public static class KinectBodyHelper
    {       
        const double HAND_LIFT_Z_DISTANCE = 0.15f;//0.15f
        const double GESTURE_Y_OFFSET = -0.65f;
        const double GESTURE_X_OFFSET = 0.185f;
        const double GESTURE_Z_OFFSET = 0.185f;

        static MVector2[] gestureOffsets = new MVector2[] {
           new MVector2(GESTURE_X_OFFSET, GESTURE_Y_OFFSET),
           new MVector2(-GESTURE_X_OFFSET, GESTURE_Y_OFFSET)
        };
        static MVector3[] depthOffset = new MVector3[] {
           new MVector3(GESTURE_X_OFFSET, GESTURE_Y_OFFSET, GESTURE_Z_OFFSET),
           new MVector3(-GESTURE_X_OFFSET, GESTURE_Y_OFFSET, GESTURE_Z_OFFSET)
        };

        public static bool IsHandLiftForward(this Body body, bool isLeft, bool isLeft2)
        {            
            return body.Joints[isLeft ? JointType.FootLeft : JointType.FootRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z < -HAND_LIFT_Z_DISTANCE;            
        }
        
        //public static void ConsoleWrite()
        //{
        //    Console.WriteLine()
        //}
        

        public static HandState GetHandState(this Body body, bool isLeft)
        {
            return isLeft ? body.HandLeftState : body.HandRightState;
        }

        //public static MVector2 GetHandRelativePosition(this Body body, bool isLeft, bool isLeft2)
        public static CameraSpacePoint GetFeetRelativePosition(this Body body, bool isLeft)
        {
            CameraSpacePoint footPosLeft = body.Joints[JointType.FootRight].Position;
            CameraSpacePoint footPosRight = body.Joints[JointType.FootRight].Position;
            Console.WriteLine("LeftPos X: " + footPosLeft.X.ToString() + " LeftPos Y: " + footPosLeft.Y.ToString() + " LeftPos Z: " + footPosLeft.Z.ToString());
            Console.WriteLine("RightPos X: " + footPosRight.X.ToString() + " RightPos Y: " + footPosRight.Y.ToString() + " RightPos Z: " + footPosRight.Z.ToString());
            return isLeft ? body.Joints[JointType.FootLeft].Position : body.Joints[JointType.FootRight].Position;

            //CameraSpacePoint footPosRight = body.Joints[JointType.FootRight].Position;
            //CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;


            //Console.WriteLine("LeftPos X: " + footPosLeft.X.ToString() + " LeftPos Y: " + footPosLeft.Y.ToString() + " LeftPos Z: " + footPosLeft.Z.ToString());
            //Console.WriteLine("RightPos X: " + footPosRight.X.ToString() + " RightPos Y: " + footPosRight.Y.ToString() + " RightPos Z: " + footPosRight.Z.ToString());
            //return footPosLeft.X - spineBase.X; /*+ gestureOffsets[isLeft ? 0 : 1];*/
            //return footPosLeft;

        }

        //public static CameraSpacePoint GetFootRightRelativePosition(this Body body, bool isLeft, bool isLeft2)
        //{            
        //    CameraSpacePoint footPosRight = body.Joints[JointType.FootRight].Position;
        //    //CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;           
        //    Console.WriteLine("RightPos X: " + footPosRight.X.ToString() + " RightPos Y: " + footPosRight.Y.ToString() + " RightPos Z: " + footPosRight.Z.ToString());
        //    //return footPosLeft.X - spineBase.X; /*+ gestureOffsets[isLeft ? 0 : 1];*/
        //    return footPosRight;

        //}

        public static MVector2 ToMVector2(this CameraSpacePoint jointPoint)
        {
            return new MVector2(jointPoint.X, jointPoint.Y);
        }       
        
        //public static bool WhichFoot(this Body body, bool isLeft)
        //{
        //    return body.Joints[isLeft ? JointType.FootLeft : JointType.FootRight].Position.Z - body.Joints[JointType.SpineBase].Position.Z;
        //}

    }
}
