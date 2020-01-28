using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Grasshopper.Kernel.Types;
using Simulacrum.Properties;

namespace Simulacrum
{
    public class NetstatMonitorComponent : GH_Component
    {
        Socket _clientSocket;

        public NetstatMonitorComponent()
          : base("Netstat Monitor", "Monitor",
              "Monitors active TCP connections. Checks every 5 seconds if the connection is still established.",
              "VirtualRobot", "KukaVarProxy")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket", "Socket", "Incoming object from TCP Client", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
           
            pManager.AddTextParameter("Output", "Output", "Active Connections", GH_ParamAccess.item);
            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string cmdOutput = "Not processed";
            //string paramMod = "";
            GH_ObjectWrapper _abstractSocket = new GH_ObjectWrapper();

            if (_clientSocket == null)
            {
                if (!DA.GetData(0, ref _abstractSocket)){ return;}
                _abstractSocket.CastTo(ref _clientSocket);
            }
            else if (_clientSocket != null && !DA.GetData(0, ref _abstractSocket))
            {
                try
                {
                    _clientSocket = null;
                    return;
                }
                catch
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Waiting For Connection...");
                    return;
                }
            }

            IPEndPoint remoteIpEndPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            cmdOutput = callFromCmd(remoteIpEndPoint.ToString() );
            DA.SetData(0, cmdOutput);

            GH_Document doc = OnPingDocument();
            if (doc != null)
            {
                doc.ScheduleSolution(500, ScheduleCallback);
            }
        }

        public string callFromCmd(string paramMod)
        {

            string netstatBase = "/c netstat -an";
            string stringFilter = "| FINDSTR";
            string netstatFull = netstatBase + " " + stringFilter + " " + paramMod;

            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd", netstatFull);
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.CreateNoWindow = true;


            System.Diagnostics.Process processObject = new System.Diagnostics.Process();
            processObject.StartInfo = processInfo;
            processObject.Start();
            string output = processObject.StandardOutput.ReadToEnd();
            processObject.WaitForExit();
            return output;
        }
        private void ScheduleCallback(GH_Document document)
        {
            ExpireSolution(false);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.monitor;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("4b848e95-c7ea-4902-b4ed-0a69d2245126"); }
        }
    }

}
