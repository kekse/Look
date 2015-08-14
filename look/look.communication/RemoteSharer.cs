﻿namespace look.communication
{
    using System;
    using System.Drawing;
    using System.ServiceModel;
    using System.Threading;

    using look.communication.Clients;
    using look.communication.Helper;
    using look.communication.Helper.Command;
    using look.utils;

    public class RemoteSharer : IDisposable
    {
        private static readonly ScreenCapture capture = new ScreenCapture();
        private ViewServiceClient proxy;
        private Thread threadCursor;
        private Thread threadScreen;
        private bool running;
        private int _numByteFullScreen = 1;

        public RemoteSharer(string host)
        {
            proxy = RemoteContext.Instance.GetProxy(host);
        }

        public void Start()
        {
            running = true;

            threadScreen = new Thread(ScreenThread);
            threadScreen.Start();

            threadCursor = new Thread(CursorThread);
            threadCursor.Start();
        }

        public void Stop()
        {
            if (proxy.State == CommunicationState.Opened)
                proxy.Abort();
            running = false;
            threadCursor.Join();
            threadScreen.Join();
        }


        private void ScreenThread()
        {
            Rectangle bounds = Rectangle.Empty;

            // Run until we are asked to stop.
            while (running)
            {
                try
                {
                    // Capture a bitmap of the changed pixels.
                    Bitmap image = capture.Screen(ref bounds);
                    if (_numByteFullScreen == 1)
                    {
                        // Initialize the screen size (used for performance metrics)
                        _numByteFullScreen = bounds.Width * bounds.Height * 4;
                    }

                    if (bounds != Rectangle.Empty && image != null)
                    {
                        // We have data...pack it and send it.
                        byte[] data = Utils.PackScreenCaptureData(image, bounds);
                        if (data != null)
                        {
                            // Thread safety on the proxy.
                            lock (proxy)
                            {
                                try
                                {
                                    // Push the data.
                                    proxy.PushScreenUpdate(data);
                                }
                                catch (Exception ex)
                                {
                                    // TODO
                                }
                            }
                        }
                        else
                        {
                            // Show performance metrics.
                            Console.WriteLine(DateTime.Now + ": Screen - no data bytes");
                        }
                    }
                    else
                    {
                        // Show performance metrics.
                        Console.WriteLine(DateTime.Now + ": Screen - no new image data");
                    }
                }
                catch (Exception ex)
                {
                    // Unhandled exception...log it.
                    Console.WriteLine("Unhandled: ************");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private void CursorThread()
        {
            // Run until we are asked to stop.
            while (running)
            {
                try
                {
                    // Get an update for the cursor.
                    int cursorX = 0;
                    int cursorY = 0;
                    Bitmap image = capture.Cursor(ref cursorX, ref cursorY);
                    if (image != null)
                    {
                        // We have valid data...pack and push it.
                        var data = Utils.PackCursorCaptureData(image, cursorX, cursorY);
                        if (data != null)
                        {
                            try
                            {
                                // Push the data.
                                var commandStack = proxy.PushCursorUpdate(data);

                                // Process command stack
                                ProcessCommands(commandStack);
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Unhandled exception...log it. TODO
                    Console.WriteLine("Unhandled: ************");
                    Console.WriteLine(ex.ToString());
                }

                // Throttle this thread a bit.
                Thread.Sleep(10);
            }
        }

        private static void ProcessCommands(string commandStack)
        {
            var cmds = new CommandInfoCollection();
            cmds.DeserializeCommandStack(commandStack);

            CommandInfo cmd;
            while ((cmd = cmds.GetNextCommand()) != null)
            {
                Command.Execute(cmd);
            }
        }

        public void Dispose()
        {
            ((IDisposable)this.proxy).Dispose();
        }
    }
}
