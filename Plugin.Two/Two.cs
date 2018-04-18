using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Interfaces;
using System.IO;

namespace Plugin.Two
{
    public class Two : IPlugin
    {
        public void Start()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            //File.AppendAllText(@"C:\Users\JonKear\Documents\visual studio 2013\Projects\AutoUpdatingService\Output\Plugins\Plugin2.txt", "I am Plugin Two running version " + version);
            Console.WriteLine("I am Plugin Two running version " + version);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }
    }
}
