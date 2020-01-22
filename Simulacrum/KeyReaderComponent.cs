using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Simulacrum.Properties;

namespace Simulacrum
{
    public class KeyReaderComponent : GH_Component
    {

        public KeyReaderComponent()
          : base("Keyboard Reader", "KeyReader",
              "Monitors global keyboard events",
              "VirtualRobot", "KukaVarProxy")
        {
        }

        
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static int shift = 0;
        public static List<string> keyPressedHistory = new List<string>() { "0" };
        bool isBackgroundWorkerActive = false;
        BackgroundWorker keyMonitorThread;


        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static void deviateToOutput(string keyPressed)
        {
            keyPressedHistory.Insert(0, keyPressed);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (Control.ModifierKeys == (Keys.Shift|Keys.Control))
                {
                    KeysConverter kc = new KeysConverter();
                    deviateToOutput(kc.ConvertToString(vkCode));
                }
            }
            else
            {
                deviateToOutput("None");
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void keyMonitorThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }
        private void keyMonitorThread_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddBooleanParameter("Start Reading Trigger", "Read", "Start reading keyboard input.", GH_ParamAccess.item, true);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Key Pressed", "Key", "Current Key", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, keyPressedHistory[0]);
            if (!isBackgroundWorkerActive)
            {
                keyMonitorThread = new BackgroundWorker();
                keyMonitorThread.RunWorkerAsync();
                isBackgroundWorkerActive = true;
            }
            keyMonitorThread.DoWork += keyMonitorThread_DoWork;
            keyMonitorThread.ProgressChanged += keyMonitorThread_ProgressChanged;
            keyMonitorThread.WorkerReportsProgress = true;

            ExpireSolution(true);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.keyreader;
                
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("67735de4-6bd9-4a17-918a-0b8fa67771af"); }
        }
    }
}