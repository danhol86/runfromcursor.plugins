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
using System.Threading;
using EnvDTE80;
using System.Xml.Linq;
using System.IO;
using RFC.Example;
using static System.Net.Mime.MediaTypeNames;

namespace LinqPad1
{
    public class MyPlugin
    {
        //First screen xaml
        public static string GetXaml()
        {
            return Helpers.ConvertUserControlToXamlString(new UserControl1());
        }

        //Name of plugin
        public static string GetControlName()   
        {
            return "Simple Commit 5";
        }

        //Deals with button clicks, and must return a new screen xaml. or null. which leaves as is
        public static string Event(string myrot, string ButtonName, string FieldValuesAsJson)
        {
            var FieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldValuesAsJson); 

            var dte = Helpers.GetAllDtes(myrot);

            return RunPlugin(dte, FieldValues);
        }

        public static string EventNative(DTE dte, string ButtonName, string FieldValuesAsJson)
        {
            var FieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldValuesAsJson);

            return RunPlugin(dte, FieldValues);
        }

        static string RunPlugin(DTE dte, Dictionary<string, string> FieldValues)
        {
            try
            {
                var commitm = FieldValues["MyTextBox"];

                //GitPlugin.Run(commitm, dte);

                var ucontrol = new UserControl1();

                ucontrol.MyMessage.Visibility = Visibility.Visible;
                ucontrol.MyLabel.Text = "Commited - " + DateTime.Now.ToString();

                string result = Helpers.ConvertUserControlToXamlString(ucontrol);
                return result;
            }
            catch (Exception e)
            {
                var errorc = new UCError();

                errorc.MyLabel.Text = "Error - " + e.ToString();

                string result = Helpers.ConvertUserControlToXamlString(errorc);
                return result;
            }
        }
        
        
    }
}

public static class Helpers
{
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