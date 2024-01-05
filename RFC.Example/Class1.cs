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

namespace LinqPad1
{
    public class EventData
    {
        public string ButtonName { get; set; }
        public Dictionary<string, string> FieldValues { get; set; }
    }

    public class MyPlugin
    {
        public static string GetXaml()
        {
            return ConvertUserControlToXamlString(new UserControl1());
        }

        public static string GetControlName()   
        {
            return "Im a Plugin 2";
        }

        public static string Event(string myrot, EventData edata)
        {
            var dte = DteFinder.GetAllDtes(myrot);

            var fullresp = JsonConvert.SerializeObject(edata);

            MessageBox.Show(fullresp);

            var ucontrol = new Done();

            ucontrol.MyLable.Text = "You clicked button - " + edata.ButtonName;
            
            return ConvertUserControlToXamlString(ucontrol);
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