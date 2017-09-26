using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Project4
{
    /// <summary>
    /// Simple Template application using SharpDX.Toolkit.
    /// </summary>
    class Program
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string Cmd, StringBuilder StrReturn, int ReturnLength, IntPtr HwndCallback);
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
#if NETFX_CORE
        [MTAThread]
#else
        [STAThread]
#endif
        static void Main()
        {
            //directory commands
            //path class
            //environment class

            string FileName = Directory.GetCurrentDirectory().ToString() + "/Content/Song.mp3";
                //@"J:\SP2014-2015\CS361A\wilsonl\Project4\Template\Content\Song.mp3";
            mciSendString("open \"" + FileName + "\" type mpegvideo alias thisIsMyTag", null, 0, IntPtr.Zero);
            mciSendString("play thisIsMyTag from 0", null, 0, IntPtr.Zero);
            using (var program = new Asteroids())
                program.Run();
        }
    }
}