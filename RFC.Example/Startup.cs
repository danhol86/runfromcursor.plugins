using LinqPad;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using EnvDTE;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using RFC.Example;
using System.Data.SqlTypes;

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
            return "Chat GPT";
        }

        //Deals with button clicks, and must return a new screen xaml. or null. which leaves as is
        public static string Event(string myrot, string ButtonName, string FieldValuesAsJson)
        {
            var FieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldValuesAsJson); 

            var dte = Helpers.GetAllDtes(myrot);

            return RunPlugin(dte, FieldValues, ButtonName);
        }

        public static string EventNative(DTE dte, string ButtonName, string FieldValuesAsJson)
        {
            var FieldValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldValuesAsJson);

            return RunPlugin(dte, FieldValues, ButtonName);
        }

        public static string KeyBindNative(DTE dte)
        {
            return Helpers.ConvertUserControlToXamlString(new UserControl1());
        }

        public static string KeyBind(string myrot)
        {
            var dte = Helpers.GetAllDtes(myrot);

            return Helpers.ConvertUserControlToXamlString(new UserControl1());
        }

        static void T()
        {
            var d = DoChat("What model are you?", "gpt-4", "");
        }

        public static string DoChat(string message, string model = null, string systemMessage = null)
        {
            try
            {
                return Task.Run(() =>
                {
                    var myret = GPTAPI.SendMessage(message, systemMessage, model).GetAwaiter().GetResult(); 
                    return myret;
                }).Result;
            } catch (Exception e)
            {
                return e.ToString();
            }
        }

        static string RunPlugin(DTE dte, Dictionary<string, string> FieldValues, string ButtonName)
        {
            try
            {
                var ucontrol = new UserControl1();
                var commitm = FieldValues["MyTextBox"];
                var use4 = FieldValues["Use4"];

                ucontrol.Use4.IsChecked = (use4.ToUpper() == "YES" || use4.ToUpper() == "TRUE" || use4.ToUpper() == "1");

                if (ButtonName == "Use4")
                {
                    string result1 = Helpers.ConvertUserControlToXamlString(ucontrol);
                    return result1;

                }
                else
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string model = null;
                    if (ucontrol.Use4.IsChecked.Value)
                    {
                        model = "gpt-4";
                    }

                    var myresp = DoChat(commitm, model);

                    ucontrol.MyMessage.Visibility = Visibility.Visible;
                    ucontrol.MyLabel.Text = myresp;

                    string result = Helpers.ConvertUserControlToXamlString(ucontrol);
                    return result;
                }
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