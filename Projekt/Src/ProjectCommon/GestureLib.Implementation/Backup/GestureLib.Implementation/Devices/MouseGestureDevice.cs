using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kennedy.ManagedHooks;
using System.Drawing;

namespace GestureLib
{
    public class MouseGestureDevice : AbstractGestureDevice, IPointerGestureDevice, IDisposable
    {
        public enum MouseButton
        { 
            LeftButton,
            RightButton
        }
        
        private MouseHook _mouseHook;
        private Size _screenSize;

        public MouseGestureDevice(Size screenSize)
            : this(screenSize, false) { }

        public MouseGestureDevice(Size screenSize, bool autoHook)
        {
            _mouseHook = new MouseHook();
            _mouseHook.MouseEvent += new MouseHook.MouseEventHandler(MouseHook_MouseEvent);

            MatchingMouseButton = MouseButton.RightButton;

            _screenSize = screenSize;

            if (autoHook)
            {
                InstallHook();
            }
        }

        private void MouseHook_MouseEvent(MouseEvents mEvent, System.Drawing.Point point)
        {
            PointerGestureState = new PointerGestureState(
                (float)point.X / (float)_screenSize.Width,
                (float)point.Y / (float)_screenSize.Height);

            OnGestureDeviceParametersChanged();

            if (MatchingMouseButton == MouseButton.LeftButton)
            {
                if (mEvent == MouseEvents.LeftButtonDown)
                {
                    OnRecordingStart();
                }
                else if (mEvent == MouseEvents.LeftButtonUp)
                {
                    OnRecordingFinish();
                }
            }

            if (MatchingMouseButton == MouseButton.RightButton)
            {
                if (mEvent == MouseEvents.RightButtonDown)
                {
                    OnRecordingStart();
                }
                else if (mEvent == MouseEvents.RightButtonUp)
                {
                    OnRecordingFinish();
                }
            }
        }

        public MouseButton MatchingMouseButton { get; set; }

        public void InstallHook()
        {
            _mouseHook.InstallHook();
        }

        public void UninstallHook()
        {
            _mouseHook.UninstallHook();
        }

        #region IPointerGestureDevice Members

        public PointerGestureState PointerGestureState { get; private set; }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UninstallHook();
                _mouseHook.Dispose();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
