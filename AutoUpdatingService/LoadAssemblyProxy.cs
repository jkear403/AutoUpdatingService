using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Interfaces;
using System.Threading;

namespace AutoUpdatingService
{
    public class LoadAssemblyProxy : MarshalByRefObject
    {
        private string assemblyPath = "";

        public void LoadAssembly(string path)
        {
            assemblyPath = path;
            Assembly.LoadFrom(assemblyPath);
        }

        public void InvokePlugin(string interfaceType, string methodName)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.Location.CompareTo(assemblyPath) == 0);
            if (assembly != null)
            {
                foreach (Type pType in assembly.GetTypes())
                {
                    if (pType.IsPublic)
                    {
                        if (!pType.IsAbstract)
                        {
                            Type tInterface = pType.GetInterface(interfaceType, true);
                            if (tInterface != null)
                            {
                                MethodInfo mInfo = tInterface.GetMethod(methodName);
                                if (mInfo != null)
                                {
                                    mInfo.Invoke(Activator.CreateInstance(assembly.GetType(pType.ToString())), null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
