Requirements
- .net 4.7.2
- Class called MyPlugin
- 3 Functions
    - public static string GetXaml()
    - public static string GetControlName()
    - public static string Event(string myrot, EventData edata)
 
public class EventData
{
    public string ButtonName { get; set; }
    public Dictionary<string, string> FieldValues { get; set; }
}

To use myrot and get handle of DTE object you can use following function

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
