using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AutoUpdatingService
{
    public class AppDomainManager : IDisposable
    {
        Dictionary<string, AppDomain> appDomains = new Dictionary<string, AppDomain>();
        Dictionary<string, AppDomain> assemblies = new Dictionary<string, AppDomain>();
        Dictionary<string, LoadAssemblyProxy> assemblyProxies = new Dictionary<string, LoadAssemblyProxy>();

        public bool LoadAssembly(string path, string domainName)
        {
            if (File.Exists(path))
            {
                if (!assemblies.ContainsKey(path))
                {
                    AppDomain appDomain = null;
                    if (appDomains.ContainsKey(domainName))
                    { appDomain = appDomains[domainName]; }
                    else
                    {
                        appDomain = CreateDomain(AppDomain.CurrentDomain, domainName);
                        appDomains[domainName] = appDomain;
                    }

                    Type proxyType = typeof(LoadAssemblyProxy);
                    if (proxyType.Assembly != null)
                    {
                        var proxy = (LoadAssemblyProxy)appDomain.CreateInstanceFrom(proxyType.Assembly.Location, proxyType.FullName).Unwrap();

                        proxy.LoadAssembly(path);

                        assemblies[path] = appDomain;
                        assemblyProxies[path] = proxy;

                        return true;
                    }
                    else
                    { return false; }
                }
                else
                { return false; }
            }
            else
            { return false; }
            
        }

        public bool IsUnloaded(string path)
        {
            if (File.Exists(path))
            {
                if (!assemblies.ContainsKey(path))
                {
                    return true;
                }
                else
                { return false; }
            }
            else
            { return false; }
        }

        public bool UnloadAssembly(string path)
        {
            if (File.Exists(path))
            {
                if (assemblies.ContainsKey(path) && assemblyProxies.ContainsKey(path))
                {
                    AppDomain appDomain = assemblies[path];
                    int count = assemblies.Values.Count(a => a == appDomain);
                    if (count != 1)
                    { return false; }

                    appDomains.Remove(appDomain.FriendlyName);
                    AppDomain.Unload(appDomain);

                    assemblies.Remove(path);
                    assemblyProxies.Remove(path);

                    return true;
                }
                else
                { return false; }
            }
            else
            { return false; }
        }

        public bool UnloadDomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                if (appDomains.ContainsKey(domainName))
                {
                    AppDomain appDomain = appDomains[domainName];

                    List<string> tempAssemeblies = new List<string>();
                    foreach (var kvp in assemblies)
                    {
                        if (kvp.Value == appDomain)
                            tempAssemeblies.Add(kvp.Key);
                    }

                    foreach (var assemblyName in tempAssemeblies)
                    {
                        assemblies.Remove(assemblyName);
                        assemblyProxies.Remove(assemblyName);
                    }

                    appDomains.Remove(domainName);
                    AppDomain.Unload(appDomain);

                    return true;
                }
                else
                { return false; }
            }
            else
            { return false; }
        }

        private AppDomain CreateDomain(AppDomain currentDomain, string domainName)
        {
            Evidence evidence = new Evidence(currentDomain.Evidence);
            AppDomainSetup setup = currentDomain.SetupInformation;
            return AppDomain.CreateDomain(domainName, evidence, setup);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var appDomain in appDomains.Values)
                { 
                    AppDomain.Unload(appDomain); 
                }

                assemblies.Clear();
                assemblyProxies.Clear();
                appDomains.Clear();
            }
        }

        public void InvokePlugin(string assemblyPath, string interfaceType, string methodName)
        {
            if (assemblies.ContainsKey(assemblyPath) && assemblyProxies.ContainsKey(assemblyPath))
            {
                assemblyProxies[assemblyPath].InvokePlugin(interfaceType, methodName);
            }
        }
    }
}
