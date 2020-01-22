using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Grasshopper.Kernel.Types;
using Simulacrum.Properties;

namespace Simulacrum
{
    public class SocketClientComponent : GH_Component
    {

        public SocketClientComponent()
          : base("TCP Client", "Client",
              "Initializes TCP socket to specified server address and port. Outputs handle to socket.",
              "VirtualRobot", "KukaVarProxy")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Server IP", "IP", "IP address of the KRC", GH_ParamAccess.item, "172.31.1.147");
            pManager.AddIntegerParameter("Server Port", "Port", "Port for connecting to the KRC", GH_ParamAccess.item, 7000);
            pManager.AddBooleanParameter("Connection Trigger", "Connect", "Trigger for connection to KRC", GH_ParamAccess.item, false);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket", "Socket", "Handle to Socket object", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int serverPort = 0;
            string serverIp = "NA";
            bool connectTrigger = false;
            bool connectToServer = false;
            if (!DA.GetData(0, ref serverIp)) return; 
            if (!DA.GetData(1, ref serverPort)) return;
            DA.GetData(2, ref connectTrigger);

            
            if (connectTrigger)
            {
                connectToServer = startClient(serverIp, serverPort);
                DA.SetData(0, new GH_ObjectWrapper(m_clientSocket));
            }
            else if(m_clientSocket != null)
            {
                m_clientSocket.Close();
                this.Message = "Socket Closed.";
            }
            else
            {
                
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Waiting for connection trigger...");
            }

        }

        // Global Variables
        Socket m_clientSocket;
        private bool startClient(string serverIP, int serverSocket)
        {
            m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            System.Net.IPAddress ip = System.Net.IPAddress.Parse(serverIP);
            IPEndPoint clientEndPoint = new IPEndPoint(ip, serverSocket);
            //m_clientSocket.SendTimeout = 25;
            //m_clientSocket.ReceiveTimeout = 25;
            Stopwatch connectionTimeout = new Stopwatch();
            connectionTimeout.Start();
            try
            {
                IAsyncResult result = m_clientSocket.BeginConnect(clientEndPoint, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                if (m_clientSocket.Connected)
                {
                    m_clientSocket.EndConnect(result);
                }else
                {
                    m_clientSocket.Close();
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Connection Timeout!");
                }
                
            }
            catch (ArgumentNullException ae)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ArgumentNullException :{0}" + ae.ToString());
                return false;
            }
            catch (SocketException se)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "SocketException :{0}" + se.ToString());
                return false;
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "UnexpectedException :{0}" + e.ToString());
                return false;
            }

            return true;
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
                return Resources.talk;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("2760acf2-53b8-4e68-9d6d-65ee6529fe35"); }
        }
    }
}
