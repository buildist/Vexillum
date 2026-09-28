using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace Vexillum.Acceptance.toolsconfig
{
    /// <summary>
    /// Serial collection for the tools/config tests that touch process-wide
    /// state the harness collections do not cover: environment variables
    /// environment, the
    /// Steamworks shim statics, ControlSystem/Settings dictionaries, the
    /// Util debug buffer and Console.Out. DisableParallelization keeps it
    /// from running alongside any other collection.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ToolsConfigCollection
    {
        public const string Name = "ToolsConfig";
    }

    /// <summary>A fresh empty temp directory, deleted on dispose.</summary>
    public sealed class TempDir : IDisposable
    {
        public string Path { get; private set; }

        public TempDir()
        {
            string baseDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "vexillum-acceptance", "tools");
            Directory.CreateDirectory(baseDir);
            Path = System.IO.Path.Combine(baseDir, Guid.NewGuid().ToString("N").Substring(0, 12));
            Directory.CreateDirectory(Path);
        }

        public string File(string name)
        {
            return System.IO.Path.Combine(Path, name);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(Path)) Directory.Delete(Path, true); } catch (Exception) { }
        }
    }

    /// <summary>Changes the current directory and restores it on dispose.</summary>
    public sealed class CwdScope : IDisposable
    {
        private readonly string previous;

        public CwdScope(string dir)
        {
            previous = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dir);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(previous);
        }
    }

    /// <summary>Sets environment variables for the scope and restores the old values on dispose.</summary>
    public sealed class ConsoleCapture : IDisposable
    {
        private readonly TextWriter previous;
        private readonly StringWriter writer = new StringWriter();

        public ConsoleCapture()
        {
            previous = Console.Out;
            Console.SetOut(writer);
        }

        public string Text
        {
            get { return writer.ToString(); }
        }

        public void Dispose()
        {
            Console.SetOut(previous);
        }
    }

    /// <summary>One recorded HTTP request of <see cref="FakeRegistry"/>.</summary>
    public static class Reflect
    {
        public static object GetStatic(Type t, string field)
        {
            FieldInfo f = t.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (f == null) throw new MissingFieldException(t.FullName, field);
            return f.GetValue(null);
        }

        public static void SetStatic(Type t, string field, object value)
        {
            FieldInfo f = t.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (f == null) throw new MissingFieldException(t.FullName, field);
            f.SetValue(null, value);
        }

        public static object CallStatic(Type t, string method, params object[] args)
        {
            MethodInfo m = t.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (m == null) throw new MissingMethodException(t.FullName, method);
            try
            {
                return m.Invoke(null, args);
            }
            catch (TargetInvocationException ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                throw;
            }
        }
    }

    /// <summary>
    /// The C# map tools (MapTool/MapTools.csproj: MapTools.MapExtractor,
    /// MapTools.MapCreator, MapTools.MapUtil) loaded from their build output;
    /// the acceptance project does not reference the project and the two
    /// interactive wrappers end in Console.ReadKey, so the static methods are
    /// driven directly.
    /// </summary>
    public static class MapToolsDriver
    {
        private static Assembly assembly;

        public static Assembly Assembly
        {
            get
            {
                if (assembly == null)
                {
                    string dll = Path.Combine(Repo.Root, "MapTool", "bin", Repo.Configuration, Repo.Tfm, "MapTools.dll");
                    if (!File.Exists(dll))
                        throw new FileNotFoundException("MapTools not built; run `dotnet build Vexillum.sln` first", dll);
                    assembly = Assembly.LoadFrom(dll);
                }
                return assembly;
            }
        }

        private static Type T(string name)
        {
            Type t = Assembly.GetType(name);
            if (t == null) throw new TypeLoadException(name + " not found in " + Assembly.Location);
            return t;
        }

        /// <summary>MapTools.MapExtractor.ExtractMap(path); returns what it printed.</summary>
        public static string ExtractMap(string path)
        {
            using (ConsoleCapture c = new ConsoleCapture())
            {
                Reflect.CallStatic(T("MapTools.MapExtractor"), "ExtractMap", path);
                return c.Text;
            }
        }

        /// <summary>MapTools.MapCreator.createMap(folder, friendlyName); returns what it printed.</summary>
        public static string CreateMap(string folder, string friendlyName)
        {
            using (ConsoleCapture c = new ConsoleCapture())
            {
                Reflect.CallStatic(T("MapTools.MapCreator"), "createMap", folder, friendlyName);
                return c.Text;
            }
        }

        /// <summary>MapTools.MapUtil.GetFileNames() (reads mapfile.list from the cwd, cached in a static).</summary>
        public static string[] GetFileNames()
        {
            return (string[])Reflect.CallStatic(T("MapTools.MapUtil"), "GetFileNames");
        }

        /// <summary>Clears MapUtil's cached file list so the next call reads mapfile.list again.</summary>
        public static void ResetFileNames()
        {
            Reflect.SetStatic(T("MapTools.MapUtil"), "files", null);
        }

        /// <summary>MapCreator.GetJpgEncoder() (private, never called at runtime).</summary>
        public static object GetJpgEncoder()
        {
            return Reflect.CallStatic(T("MapTools.MapCreator"), "GetJpgEncoder");
        }
    }

    public static class Poll
    {
        /// <summary>Polls until the condition holds or the timeout passes; returns whether it held.</summary>
        public static bool Until(Func<bool> condition, TimeSpan timeout, int intervalMs = 50)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (true)
            {
                if (condition())
                    return true;
                if (DateTime.UtcNow >= deadline)
                    return condition();
                Thread.Sleep(intervalMs);
            }
        }
    }

    public static class Net
    {
        /// <summary>127.0.0.1 plus every IPv4 unicast address of this machine (what a LAN beacon can be heard from).</summary>
        public static HashSet<string> LocalAddresses()
        {
            HashSet<string> r = new HashSet<string> { "127.0.0.1" };
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    foreach (UnicastIPAddressInformation u in ni.GetIPProperties().UnicastAddresses)
                        if (u.Address.AddressFamily == AddressFamily.InterNetwork)
                            r.Add(u.Address.ToString());
            }
            catch (Exception) { }
            return r;
        }

        /// <summary>A non-loopback IPv4 address of an interface that is up, or null.</summary>
        public static string LanAddress()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;
                    foreach (UnicastIPAddressInformation u in ni.GetIPProperties().UnicastAddresses)
                        if (u.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(u.Address))
                            return u.Address.ToString();
                }
            }
            catch (Exception) { }
            return null;
        }
    }

    /// <summary>
    /// The one UDP port the in-process LAN beacon uses. LanDiscovery.Beacon
    /// starts its thread once per process and binds the port it was first
    /// given (later calls only replace the payload), so every test that
    /// starts or listens for the in-process beacon must use this port.
    /// </summary>
}
