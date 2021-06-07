using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace toolstray
{
    /// <summary>
    /// URL をブラウザで表示 
    /// </summary>
    public class OpenURL : IShortcut
    {
        string _url;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="url">URL</param>
        public OpenURL(string url)
        {
            _url = url;
        }


        /// <summary>
        /// 実行
        /// </summary>
        public void Run()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd";
            info.Arguments = $"/C start " + _url;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Start();
        }
    }
}
