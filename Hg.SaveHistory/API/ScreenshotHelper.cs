using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Hg.SaveHistory.Types;
using Hg.SaveHistory.Utilities;
using NLua;

namespace Hg.SaveHistory.API
{
    public class ScreenshotHelper
    {
        #region Fields & Properties

        private readonly Engine _engine;

        private IntPtr _processPtr = IntPtr.Zero;

        [LuaHide] public static ScreenshotQuality ScreenshotFormat { get; set; } = ScreenshotQuality.Png;

        #endregion

        #region Members

        public ScreenshotHelper(Engine engine)
        {
            _engine = engine;
        }

        public bool Capture(Rectangle captureBounds, EngineSnapshot snapshot)
        {
            Logger.Information(MethodBase.GetCurrentMethod().DeclaringType.Name, ".", MethodBase.GetCurrentMethod().Name);

            try
            {
                snapshot.HasScreenshot = false;

                Bitmap bitmap = new Bitmap(captureBounds.Width, captureBounds.Height);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(captureBounds.Location, Point.Empty, captureBounds.Size);
                }

                string screenShotExtension;
                ImageFormat screenShotFormat;
                switch (ScreenshotFormat)
                {
                    case ScreenshotQuality.Gif:
                        screenShotExtension = ".gif";
                        screenShotFormat = ImageFormat.Gif;
                        break;
                    case ScreenshotQuality.Jpg:
                        screenShotExtension = ".jpg";
                        screenShotFormat = ImageFormat.Jpeg;
                        break;
                    default:
                        screenShotExtension = ".png";
                        screenShotFormat = ImageFormat.Png;
                        break;
                }

                string path = Path.Combine(_engine.SnapshotsFolder, snapshot.RelativePath, "screenshot" + screenShotExtension);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                bitmap.Save(path, screenShotFormat);

                snapshot.ScreenshotFilename = "screenshot" + screenShotExtension;
                snapshot.HasScreenshot = true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                Utilities.Logger.LogException(exception);
                return false;
            }

            return snapshot.HasScreenshot;
        }

        public Rectangle? GetClientBounds()
        {
            IntPtr processPtr = GetProcessPtr();
            if (processPtr == IntPtr.Zero)
            {
                return null;
            }

            return ScreenShots.GetClientBounds(processPtr);
        }

        public Rectangle? GetWindowBounds()
        {
            IntPtr processPtr = GetProcessPtr();
            if (processPtr == IntPtr.Zero)
            {
                return null;
            }

            return ScreenShots.GetWindowBounds(processPtr);
        }

        public bool HasTitleBar()
        {
            IntPtr processPtr = GetProcessPtr();
            if (processPtr == IntPtr.Zero)
            {
                return false;
            }

            return ScreenShots.HasTitlebar(processPtr);
        }

        public int TitleBarHeight()
        {
            return ScreenShots.GetTitleBarHeight();
        }

        private IntPtr GetProcessPtr()
        {
            if (_engine == null || _engine.ProcessNames.Count == 0)
            {
                _processPtr = IntPtr.Zero;
                return IntPtr.Zero;
            }

            var processPtr = IntPtr.Zero;
            foreach (var process in Process.GetProcesses())
            {
                if (_engine.ProcessNames.Contains(process.ProcessName))
                {
                    processPtr = process.MainWindowHandle;
                    break;
                }
            }

            _processPtr = processPtr;

            return _processPtr;
        }

        #endregion
    }
}