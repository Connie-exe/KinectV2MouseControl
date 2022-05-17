﻿using Microsoft.Kinect;

namespace KinectV2MouseControl
{
    public static class KinectBodyHelper
    {
        const double HAND_LIFT_Z_DISTANCE = 0.15f;//0.15f
        const double GESTURE_Y_OFFSET = -0.65f;
        const double GESTURE_X_OFFSET = 0.185f;
        
        static MVector2[] gestureOffsets = new MVector2[] {
           new MVector2(GESTURE_X_OFFSET, GESTURE_Y_OFFSET),
           new MVector2(-GESTURE_X_OFFSET, GESTURE_Y_OFFSET)
        };

        public static bool IsHandLiftForward(this Body body, bool isLeft, bool isLeft2)
        {
            return body.Joints[isLeft ? JointType.FootLeft : JointType.FootRight].Position.Z - body.Joints[isLeft2 ? JointType.HipLeft : JointType.HipRight].Position.Z < -HAND_LIFT_Z_DISTANCE;
        }

        public static HandState GetHandState(this Body body, bool isLeft)
        {
            return isLeft ? body.HandLeftState : body.HandRightState;
        }

        public static MVector2 GetHandRelativePosition(this Body body, bool isLeft, bool isLeft2)
        {
            CameraSpacePoint handPos = body.Joints[isLeft ? JointType.FootLeft : JointType.FootRight].Position;
            CameraSpacePoint spineBase = body.Joints[isLeft2 ? JointType.HipLeft : JointType.HipRight].Position;
            
            return handPos.ToMVector2() - spineBase.ToMVector2() + gestureOffsets[isLeft ? 0 : 1];
        }

        public static MVector2 ToMVector2(this CameraSpacePoint jointPoint)
        {
            return new MVector2(jointPoint.X, jointPoint.Y);
        }

    }
}
