/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;

namespace Chummer.Backend
{
    public static class CrashHandler
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private sealed class DumpData : ISerializable, IDeserializationCallback
        {
            public DumpData(Exception ex)
            {
                _dicPretendFiles = new Dictionary<string, string> { { "exception.txt", ex?.ToString().Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]") ?? "No Exception Specified" } };

                _dicAttributes = new Dictionary<string, string>
                {
                    {"visible-crash-id", Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo)},
#if DEBUG
                    {"visible-build-type", "DEBUG"},
#else
                    {"visible-build-type", "RELEASE"},
#endif
                    {"commandline", Environment.CommandLine.Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]")},
                    {"visible-version", Application.ProductVersion},
                    {"chummer-version", Utils.CurrentChummerVersion.ToString()},
                    {"os-type", Environment.OSVersion.VersionString},
                    {"human-readable-os-version", Utils.HumanReadableOSVersion},
                    {"visible-error-friendly", ex?.Message.Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]") ?? "No description available"},
                    {"visible-stacktrace", ex?.StackTrace.Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]") ?? "No stack trace available"},
                    {"installation-id", Properties.Settings.Default.UploadClientId.ToString() },
                    {"option-upload-logs-set", GlobalSettings.UseLoggingApplicationInsights.ToString() }
                };

                try
                {
                    _dicAttributes.Add("chummer-ui-language", GlobalSettings.Language);
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("chummer-ui-language", e.ToString());
                }
                try
                {
                    _dicAttributes.Add("chummer-cultureinfo", GlobalSettings.CultureInfo.ToString());
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("chummer-cultureinfo", e.ToString());
                }
                try
                {
                    _dicAttributes.Add("system-cultureinfo", GlobalSettings.SystemCultureInfo.ToString());
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("system-cultureinfo", e.ToString());
                }

                //Crash handler will make visible-{whatever} visible in the upload while the rest will exists in a file named attributes.txt
                if (Registry.LocalMachine != null)
                {
                    RegistryKey obj64BitRegistryKey = null;
                    RegistryKey objCurrentVersionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                    if (objCurrentVersionKey?.GetValueNames().Contains("ProductId") == false)
                    {
                        //On 32 bit builds? get 64 bit registry
                        objCurrentVersionKey.Close();
                        objCurrentVersionKey.Dispose();
                        obj64BitRegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        objCurrentVersionKey = obj64BitRegistryKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    }

                    if (objCurrentVersionKey != null)
                    {
                        try
                        {
                            try
                            {
                                _dicAttributes.Add("machine-id", objCurrentVersionKey.GetValue("ProductId").ToString());
                            }
                            catch (Exception e)
                            {
                                _dicAttributes.Add("machine-id", e.ToString());
                            }

                            try
                            {
                                _dicAttributes.Add("os-name", objCurrentVersionKey.GetValue("ProductName").ToString());
                            }
                            catch (Exception e)
                            {
                                _dicAttributes.Add("os-name", e.ToString());
                            }
                        }
                        finally
                        {
                            objCurrentVersionKey.Close();
                        }
                    }
                    obj64BitRegistryKey?.Close();
                }

                PropertyInfo[] systemInformation = typeof(SystemInformation).GetProperties();
                foreach (PropertyInfo propertyInfo in systemInformation)
                {
                    _dicAttributes.Add("system-info-" + propertyInfo.Name, propertyInfo.GetValue(null).ToString());
                }
            }

            // JavaScriptSerializer requires that all properties it accesses be public.
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Dictionary<string, string> _dicCapturedFiles = new Dictionary<string, string>();

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Dictionary<string, string> _dicPretendFiles;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Dictionary<string, string> _dicAttributes;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly int _intProcessId = Program.MyProcess.Id;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly uint _uintThreadId = NativeMethods.GetCurrentThreadId();

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly IntPtr _ptrExceptionInfo = Marshal.GetExceptionPointers();

            public void SerializeJson(JsonWriter objWriter)
            {
                JsonSerializer.CreateDefault().Serialize(objWriter, this);
            }

            internal void AddFile(string strFileName)
            {
                string strKey = strFileName.Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]");
                if (_dicCapturedFiles.ContainsKey(strKey))
                    return;
                try
                {
                    _dicCapturedFiles.Add(strKey, string.Empty);
                }
                catch (ArgumentException)
                {
                    return;
                }
                string strContents;
                try
                {
                    strContents = File.ReadAllText(strFileName, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    strContents = e.ToString();
                }
                _dicCapturedFiles[strKey] = strContents.Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("_intProcessId", _intProcessId);
                info.AddValue("_uintThreadId", _uintThreadId);
                info.AddValue("_ptrExceptionInfo", _ptrExceptionInfo.ToInt64());
                info.AddValue("_dicAttributes", _dicAttributes);
                info.AddValue("_dicPretendFiles", _dicPretendFiles);
                info.AddValue("_dicCapturedFiles", _dicCapturedFiles);
            }

            /// <inheritdoc />
            public void OnDeserialization(object sender)
            {
                _dicAttributes.OnDeserialization(sender);
                _dicPretendFiles.OnDeserialization(sender);
            }
        }

        public static void WebMiniDumpHandler(Exception ex, DateTime datCrashDateTime)
        {
            try
            {
                DumpData dump = new DumpData(ex);
                foreach (string strSettingFile in Directory.EnumerateFiles(Utils.GetSettingsFolderPath, "*.xml"))
                {
                    dump.AddFile(strSettingFile);
                }

                string strChummerLog = Path.Combine(Utils.GetStartupPath, "chummerlog.txt");
                if (File.Exists(strChummerLog))
                    dump.AddFile(strChummerLog);

                string strJsonPath = Path.Combine(Utils.GetStartupPath, "chummer_crash_" + datCrashDateTime.ToFileTimeUtc() + ".json");
                using (FileStream objFileStream = new FileStream(strJsonPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter objStreamWriter = new StreamWriter(objFileStream))
                using (JsonTextWriter objJsonWriter = new JsonTextWriter(objStreamWriter))
                {
                    objJsonWriter.Formatting = Formatting.Indented;
                    dump.SerializeJson(objJsonWriter);
                }

                string strCrashHandler = Path.Combine(Utils.GetStartupPath, "CrashHandler.exe");
                if (File.Exists(strCrashHandler))
                {
                    string strArgs = "crash \"" + strJsonPath + "\" \"" + datCrashDateTime.ToFileTimeUtc()
#if DEBUG
                        + "\" --debug";
#else
                        + "\"";
#endif
                    using (Process prcCrashHandler = Process.Start(strCrashHandler, strArgs))
                    {
                        if (prcCrashHandler == null)
                            return;
                        prcCrashHandler.WaitForExit();
                        if (prcCrashHandler.ExitCode != 0)
                        {
                            Program.ShowScrollableMessageBox(
                                "Failed to create crash report because of an issue with the crash handler."
                                + Environment.NewLine + "Chummer crashed with version: " + Utils.CurrentChummerVersion
                                + Environment.NewLine + "Crash Handler crashed with exit code: " + prcCrashHandler.ExitCode
                                + Environment.NewLine + "Crash information:"
                                + Environment.NewLine + ex.ToString().Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]"),
                                "Failed to Create Crash Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Program.ShowScrollableMessageBox(
                                "Failed to create crash report because the crash handler was not found."
                                + Environment.NewLine + "Chummer crashed with version: " + Utils.CurrentChummerVersion
                                + Environment.NewLine + "Crash information:"
                                + Environment.NewLine + ex.ToString().Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]"),
                                "Failed to Create Crash Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception nex)
            {
                Program.ShowScrollableMessageBox(
                    "Failed to create crash report."
                    + Environment.NewLine + "Chummer crashed with version: " + Utils.CurrentChummerVersion
                    + Environment.NewLine + "Here is some information to help the developers figure out why:"
                    + Environment.NewLine + nex.Demystify().ToString().Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]")
                    + Environment.NewLine + "Crash information:" + Environment.NewLine
                    + ex.ToString().Replace(Utils.GetStartupPath, "[Chummer Path]").Replace(Utils.GetEscapedStartupPath, "[Chummer Path]"),
                    "Failed to Create Crash Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
