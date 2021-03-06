﻿namespace look.communication.Services
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using look.common.Command;
    using look.common.Events;
    using look.common.Helper;
    using look.common.Model;
    using look.communication.Contracts;
    using look.communication.Model;

    public class ViewService : IViewService
    {

        private string GetIp() {
            try {
                return (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;
            } catch {
                return null;
            }
        }

        public bool Connect() {
            var ip = this.GetIp();
            if (string.IsNullOrEmpty(ip))
                return false;

            var e = new HostConnectedEventArgs { Ip = ip };
            this.OnHostConnected(e);

            return e.Accepted;
        }

        public void Disconnect()
        {
            var e = new HostDisconnectedEventArgs { Ip = this.GetIp() };
            this.OnHostDisconnected(e);
        }

        public void PushAvailableWindows(List<Window> windows) {
            var e = new WindowsSharedEventArgs { Ip = this.GetIp(), Windows = windows };
            this.OnWindowsShared(e);
        }

        public void PushScreenUpdate(byte[] data)
        {
            if (data == null)
                return;

            Image partial;
            Rectangle bounds;
            Guid id;
            Utils.UnpackScreenCaptureData(data, out partial, out bounds, out id);

            ViewSession viewSession;
            if (!_sessions.ContainsKey(id))
            {
                viewSession = new ViewSession { Id = id, Ip = this.GetIp() };
                _sessions[id] = viewSession;
            }
            else
            {
                viewSession = _sessions[id];
            }

            Utils.UpdateScreenImage(ref viewSession.Screen, partial, bounds);

            UpdateScreenImage(id);
        }

        public string PushCursorUpdate(byte[] data)
        {
            if (data != null)
            {
                Image cursor;
                int cursorX, cursorY;
                Guid id;
                Utils.UnpackCursorCaptureData(data, out cursor, out cursorX, out cursorY, out id);

                ViewSession viewSession;
                if (!_sessions.ContainsKey(id))
                {
                    viewSession = new ViewSession { Id = id, Ip = this.GetIp() };
                    _sessions[id] = viewSession;
                }
                else
                {
                    viewSession = _sessions[id];
                }

                viewSession.Cursor = cursor;
                viewSession.CursorX = cursorX;
                viewSession.CursorY = cursorY;
                UpdateScreenImage(id);
            }

            return Commands.SerializeCommandStack();
        }

        public void RequestWindowTransfer(List<Window> windows)
        {
            var e = new WindowsRequestedEventArgs { Ip = this.GetIp(), Windows = windows };
            this.OnWindowsRequested(e);
        }

        #region Events

        public delegate void HostConnectedHandler(object sender, HostConnectedEventArgs e);
        public static event HostConnectedHandler HostConnected;
        
        public delegate void WindowsSharedHandler(object sender, WindowsSharedEventArgs e);
        public static event WindowsSharedHandler WindowsShared;

        public delegate void WindowsRequestedHandler(object sender, WindowsRequestedEventArgs e);
        public static event WindowsRequestedHandler WindowsRequested;

        public delegate void ImageChangedHandler(Image display, string id, string ip);
        public static event ImageChangedHandler ImageChanged;

        public delegate void HostDisconnectedHandler(object sender, HostDisconnectedEventArgs e);
        public static event HostDisconnectedHandler HostDisconnected;

        #endregion

        private static readonly Dictionary<Guid, ViewSession> _sessions = new Dictionary<Guid, ViewSession>();        
        public static CommandInfoCollection Commands = new CommandInfoCollection();

        private static void UpdateScreenImage(Guid id)
        {
            var viewSession = _sessions[id];
            if (viewSession == null)
            {
                return;
            }
            if (viewSession.Screen == null)
            {
                return;
            }

            if (viewSession.Cursor != null)
            {
                viewSession.Display = Utils.MergeScreenAndCursor(viewSession.Screen, viewSession.Cursor, viewSession.CursorX, viewSession.CursorY);
            }
            else
            {
                viewSession.Display = viewSession.Screen;
            }

            if (ImageChanged != null)
            {
                ImageChanged(viewSession.Display, viewSession.Id.ToString(), viewSession.Ip);
            }
        }


        private void OnHostConnected(HostConnectedEventArgs e)
        {
            if (HostConnected != null)
            {
                HostConnected(this, e);
            }
        }

        private void OnWindowsShared(WindowsSharedEventArgs e)
        {
            if (WindowsShared != null)
            {
                WindowsShared(this, e);
            }
        }

        private void OnWindowsRequested(WindowsRequestedEventArgs e)
        {
            if (WindowsRequested != null)
            {
                WindowsRequested(this, e);
            }
        }

        private void OnHostDisconnected(HostDisconnectedEventArgs e)
        {
            if (HostDisconnected != null)
            {
                HostDisconnected(this, e);
            }
        }
    }

}
