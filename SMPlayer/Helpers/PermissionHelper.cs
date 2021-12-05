using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Popups;

namespace SMPlayer.Helpers
{
    // https://docs.microsoft.com/en-us/windows/uwp/design/input/speech-recognition
    public static class PermissionHelper
    {
        // If no microphone is present, an exception is thrown with the following HResult value.
        private static int NoCaptureDevicesHResult = -1072845856;

        /// <summary>
        /// Note that this method only checks the Settings->Privacy->Microphone setting, it does not handle
        /// the Cortana/Dictation privacy check.
        ///
        /// You should perform this check every time the app gets focus, in case the user has changed
        /// the setting while the app was suspended or not in focus.
        /// </summary>
        /// <returns>True, if the microphone is available.</returns>
        public async static Task<bool> RequestMicrophonePermission()
        {
            try
            {
                // Request access to the audio capture device.
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio,
                    MediaCategory = MediaCategory.Speech
                };
                MediaCapture capture = new MediaCapture();
                await capture.InitializeAsync(settings);
            }
            catch (TypeLoadException)
            {
                // Thrown when a media player is not available.
                var messageDialog = new MessageDialog("Media player components are unavailable.");
                await messageDialog.ShowAsync();
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // Thrown when permission to use the audio capture device is denied.
                // If this occurs, show an error or disable recognition functionality.
                return false;
            }
            catch (Exception exception)
            {
                // Thrown when an audio capture device is not present.
                if (exception.HResult == NoCaptureDevicesHResult)
                {
                    var messageDialog = new MessageDialog("No Audio Capture devices are present on this system.");
                    await messageDialog.ShowAsync();
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }
    }
}
