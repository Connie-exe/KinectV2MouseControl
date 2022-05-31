using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{
    public class KinectCursor
    {
        private KinectReader sensorReader;
        private CursorMapper cursorMapper;

        public enum ControlMode
        {
            Disabled = 0,
            MoveOnly,
            GripToPress,
            HoverToClick,
            MoveGripPressing,
            MoveLiftClicking
        }


        private ControlMode _mode = ControlMode.Disabled;
        public ControlMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (value == ControlMode.Disabled)
                {
                    ToggleHoverTimer(false);
                    sensorReader.Close();
                }
                else
                {
                    sensorReader.Open();
                }
            }
        }

        public double Smoothing
        {
            get
            {
                return cursorMapper.Smoothing;
            }
            set
            {
                cursorMapper.Smoothing = value;
            }
        }

        /// <summary>
        /// Rect value worked out by pointing to left, top, right, bottom spot with my hand as the ideal edges, and noting down the x, y values got from GetHandRelativePosition.
        /// This may only fit more for me, and you can test out your rect value. Approximately it works fine for most people.
        /// </summary>
        private MRect gestureRect = new MRect(-0.18, 1.65, 0.18, -1.65);//-0.18, 1.65, 0.18, -1.65

        private bool[] handGrips = new bool[2] { false, false };

        private double _hoverDuration = 2;//2

        /// <summary>
        /// Wait time for a hover gesture.
        /// </summary>
        public double HoverDuration {
            get
            {
                return _hoverDuration;
            }
            set
            {
                _hoverDuration = value;
                hoverTimer.Interval = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Hand moves further than distance will cause a hover fail.
        /// Default as 20, this needs to be modified according to usual movement distance/speed in your specific case.
        /// </summary>
        public double HoverRange { get; set; } = 20;//20
        
        public double MoveScale
        {
            get
            {
                return cursorMapper.MoveScale;
            }
            set
            {
                cursorMapper.MoveScale = value;
            }
        }

        public double FootLiftYForClick { get; set; } = 0.02f;//0.02f

        private MVector2 lastCursorPos = MVector2.Zero;

        /// <summary>
        /// Timer for hover detection.
        /// </summary>
        private DispatcherTimer hoverTimer = new DispatcherTimer();

        private const int NONE_USED = -1;

        /// <summary>
        /// Used to keep track of the controlling hand. So when another hand lift up forward, the cursor would still follow the first controlling hand.
        /// </summary>
        private int usedFootIndex = NONE_USED;
        private bool hoverClicked = false;

        public KinectCursor()
        {
            MRect screenRect = new MRect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            cursorMapper = new CursorMapper(gestureRect, screenRect, CursorMapper.ScaleAlignment.LongerRange);

            sensorReader = new KinectReader(false);
            sensorReader.OnTrackedBody += Kinect_OnTrackedBody;
            sensorReader.OnLostTracking += Kinect_OnLostTracking;
            hoverTimer.Interval = TimeSpan.FromSeconds(HoverDuration);
            hoverTimer.Tick += new EventHandler(HoverTimer_Tick);
        }

        private void Kinect_OnLostTracking(object sender, EventArgs e)
        {
            ToggleHoverTimer(false);
            ReleaseGrip(0);
            ReleaseGrip(1);
            usedFootIndex = NONE_USED;
        }

        private void Kinect_OnTrackedBody(object sender, BodyEventArgs e)
        {
            Body body = e.BodyData;

            if (Mode == ControlMode.Disabled)
            {
                return;
            }

            for(int i = 1; i >= 0; i--) // Starts looking from right hand.
            {
                bool isLeft = (i == 0);
                bool isLeft2 = (i == 0);
                if (body.IsFootLiftForward(isLeft, isLeft2))
                {
                    if (usedFootIndex == -1)
                    {
                        usedFootIndex = i;
                    } else if (usedFootIndex!= i)
                    {
                        // In two-hand control mode, non-used hand would be used for pressing/releasing mouse button.
                        if (Mode == ControlMode.MoveGripPressing)
                        {
                            DoMouseControlByHandState(i, body.GetFeetState(isLeft));
                        } 

                        continue;
                    }

                    //MVector2 handPos = body.GetHandRelativePosition(isLeft, isLeft2)

                    CameraSpacePoint feetPos = body.GetFeetRelativePosition(isLeft);                    
                    MVector2 targetPos = cursorMapper.GetSmoothedOutputPosition(feetPos.ToMVector2());

                    //System.Diagnostics.Trace.WriteLine(handPos.ToString());

                    MouseControl.MoveTo(targetPos.X, targetPos.Y);

                    MouseControl.MoveTo(targetPos.X, 500); //siempre en la misma posición en y

                    if (Mode == ControlMode.GripToPress)
                    {
                        DoMouseControlByHandState(i, body.GetFeetState(isLeft));
                    }
                    else if (Mode == ControlMode.HoverToClick)
                    {
                        if ((targetPos - lastCursorPos).Length() > HoverRange)
                        {
                            ToggleHoverTimer(false);
                            hoverClicked = false;
                        }

                        lastCursorPos = targetPos;
                    }
                }
                else
                {
                    if(usedFootIndex == i)
                    {
                        // Reset to none.
                        usedFootIndex = NONE_USED;
                        ReleaseGrip(i);
                    }
                    else  if (Mode == ControlMode.MoveLiftClicking)
                    {
                        //DoMouseClickByHandLifting(i, body.GetHandRelativePosition(isLeft, isLeft2));
                        //System.Diagnostics.Trace.WriteLine(body.GetHandRelativePosition(isLeft).Y);
                    }
                    else // Release mouse button when it's not regularly released, such as hand tracking lost.
                    {
                        ReleaseGrip(i);
                    }
                    
                }
                
            }

            ToggleHoverTimer(Mode == ControlMode.HoverToClick && usedFootIndex != -1);
        }

        private void DoMouseControlByHandState(int handIndex, HandState handState)
        {
            MouseControlState controlState;
            switch (handState)
            {
                case HandState.Closed:
                    controlState = MouseControlState.ShouldPress;
                    break;
                case HandState.Open:
                    controlState = MouseControlState.ShouldRelease;
                    break;
                default:
                    controlState = MouseControlState.None;
                    break;
            }
            UpdateHandMouseControl(handIndex, controlState);
        }

        private void DoMouseClickByHandLifting(int handIndex, MVector2 handRelativePos)
        {
            UpdateHandMouseControl(handIndex, handRelativePos.Y > FootLiftYForClick ? MouseControlState.ShouldClick : MouseControlState.ShouldRelease);
            
            //DoMouseControlByHandLifting(with press and releas rather than just a click):
            //MouseControlState controlState = handRelativePos.Y > HandLiftYForClick ? MouseControlState.ShouldPress : MouseControlState.ShouldRelease;
            //UpdateHandMouseControl(handIndex, controlState);
        }

        private enum MouseControlState
        {
            None,
            ShouldPress,
            ShouldRelease,
            ShouldClick,
        }

        private void UpdateHandMouseControl(int footIndex, MouseControlState controlState)
        {
            if (controlState == MouseControlState.ShouldClick)
            {
                if (!handGrips[footIndex])
                {
                    MouseControl.Click();
                    handGrips[footIndex] = true;
                }
            }else if (controlState == MouseControlState.ShouldPress)
            {
                if (!handGrips[footIndex])
                {
                    MouseControl.PressDown();
                    handGrips[footIndex] = true;
                }
            }
            else if (controlState == MouseControlState.ShouldRelease && handGrips[footIndex])
            {
                ReleaseGrip(footIndex);
            }
        }

        private void ReleaseGrip(int index)
        {
            if (handGrips[index])
            {
                MouseControl.PressUp();
                handGrips[index] = false;
            }
        }

        private void ToggleHoverTimer(bool isOn)
        {
            if(hoverTimer.IsEnabled != isOn)
            {
                if (isOn)
                {
                    hoverTimer.Start();
                }
                else
                {
                    hoverTimer.Stop();
                }
            }
        }

        void HoverTimer_Tick(object sender, EventArgs e)
        {
            if (!hoverClicked)
            {
                MouseControl.Click();
                hoverTimer.Stop();
                hoverClicked = true;
            }
        }

    }
    
}
