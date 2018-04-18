using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using System.Threading;
using System.Security.Policy;

namespace AutoUpdatingService
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exitTest = false;
            Console.WriteLine("Welcome to Auto Updating Service");
            AppDomainManager adm = new AppDomainManager();

            while (!exitTest)
            {
                string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Plugins\\";

                foreach (string plugin in Directory.GetFiles(assemblyPath, "*.dll"))
                {
                    
                    if (adm.IsUnloaded(plugin))
                    {
                        string updatePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Updates\\";
                        if (File.Exists(updatePath + Path.GetFileName(plugin)))
                        {
                            File.Copy(updatePath + Path.GetFileName(plugin), plugin, true);
                            File.Delete(updatePath + Path.GetFileName(plugin));
                        }

                        bool success = adm.LoadAssembly(plugin, Path.GetFileNameWithoutExtension(plugin));
                        if (success)
                        {
                            Action invokePlugin = () =>
                            {
                                adm.InvokePlugin(plugin, "Interfaces.IPlugin", "Start");
                                adm.UnloadAssembly(plugin);
                            };

                            Task invokeTask = new Task(invokePlugin);
                            invokeTask.Start();
                            Console.WriteLine(Path.GetFileName(plugin));
                        }
                    }


                }

                Thread.Sleep(20000);

            }
        }
    }
}