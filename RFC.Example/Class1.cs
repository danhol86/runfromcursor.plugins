using LinqPad;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace LinqPad1
{
    public class EventData
    {
        public string ButtonName { get; set; }
        public Dictionary<string, string> FieldValues { get; set; }
    }

    public class MyPlugin
    {
        //First screen xaml
        public static string GetXaml()
        {
            return ConvertUserControlToXamlString(new UserControl1());
        }

        //Name of plugin
        public static string GetControlName()   
        {
            return "Test Plugin 2";
        }

        //Deals with button clicks, and must return a new screen xaml. or null. which leaves as is
        public static string Event(string myrot, EventData edata)
        {
            var dte = DteFinder.GetAllDtes(myrot);

            GitTest(dte);

            var mainproj = dte.Solution.Projects.Cast<Project>().FirstOrDefault().Name;
            var fullresp = JsonConvert.SerializeObject(edata);

            var message = "";
            message += "Button Name - " + edata.ButtonName + "\n";
            message += "Rot - " + myrot + "\n";
            message += "Main Project - " + mainproj + "\n";
            message += "RESP - " + fullresp + "\n";

            var ucontrol = new Done();

            ucontrol.MyLable.Text = message;

            return ConvertUserControlToXamlString(ucontrol);
        }

        static void GitTest(DTE dte)
        {
            var fname = dte.Solution.FullName;
            var _solutionDirectory = System.IO.Path.GetDirectoryName(fname);



            ExecuteGitCommand("add .", _solutionDirectory);
            ExecuteGitCommand("commit -m \"Auto commit\"", _solutionDirectory);
            ExecuteGitCommand("push", _solutionDirectory);
        }


        private static void ExecuteGitCommand(string command, string folder)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = folder,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Handle the output/error as needed
                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error: " + error);
                }
            }
        }
        public static string ConvertUserControlToXamlString(UserControl userControl)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                XamlWriter.Save(userControl, xmlWriter);
            }

            return sb.ToString();
        }
    }

    public static class DteFinder
    {
        public static DTE GetAllDtes(string RotName)
        {
            IntPtr numFetched = Marshal.AllocHGlobal(sizeof(int));

            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            IBindCtx bindCtx;
            Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));

            bindCtx.GetRunningObjectTable(out runningObjectTable);

            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                if (runningObjectName.Contains("VisualStudio.DTE.") && runningObjectName == RotName)
                {
                    object runningObjectVal;
                    runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                    DTE dte = (DTE)runningObjectVal;
                    return dte;
                }
            }
            return null;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
    }
}