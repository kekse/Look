﻿namespace look.common.Events
{
    using System;
    using System.Drawing;

    public class ScreenUpdateEventArgs : EventArgs
    {
        public string Ip { get; set; }
        public string WindowId { get; set; }
        public Image Screen { get; set; }
    }
}
