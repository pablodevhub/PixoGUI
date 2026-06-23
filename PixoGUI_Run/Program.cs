using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixoGUI_Run
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = System.IO.Path.Combine(dir, "publish", "PixoGUI.exe");
            p.StartInfo.Arguments = "/secondary /minimized";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            //System.Threading.Thread.Sleep(10);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
