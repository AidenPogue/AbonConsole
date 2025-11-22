using System.Diagnostics;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    public static class ClipboardTools
    {
        public static void CopyToClipboard(string text)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    CopyToClipboardWindows(text);
                    break;

                default:
                    UnityEngine.Debug.LogWarning("Tried to copy to clipboard on an unsupported platform.");
                    break;
            }
        }

        private static void CopyToClipboardWindows(string text)
        {
            var procStart = new ProcessStartInfo()
            {
                FileName = @"clip",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                //StandardInputEncoding = System.Text.Encoding.UTF8,
            };
            var proc = Process.Start(procStart);
            proc.StandardInput.Write(text);
            proc.StandardInput.Close();
            proc.Close();
        }
    }
}
