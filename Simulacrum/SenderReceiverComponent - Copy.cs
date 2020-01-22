using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Net.Sockets;
using Grasshopper.Kernel.Types;
using System.Text;

namespace Simulacrum
{
    public class SenderReceiverComponent : GH_Component
    {

        public SenderReceiverComponent()
          : base("SenderReceiver", "InOut",
              "Sends variable values to KUKAVARPROXY",
              "COM", "")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket Object", "Client", "Incoming object from SocketClient", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Target Plane", "Target", "Current position and orientation of TCP", GH_ParamAccess.item, new Plane());
            pManager.AddPlaneParameter("World Plane", "World", "Reference plane for A,B,C calculation", GH_ParamAccess.item, new Plane());

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("X", "X", "X", GH_ParamAccess.item);
            pManager.AddTextParameter("Y", "Y", "Y", GH_ParamAccess.item);
            pManager.AddTextParameter("Z", "Z", "Z", GH_ParamAccess.item);
            pManager.AddTextParameter("A", "A", "A", GH_ParamAccess.item);
            pManager.AddTextParameter("B", "B", "B", GH_ParamAccess.item);
            pManager.AddTextParameter("C", "C", "C", GH_ParamAccess.item);
            
        }

       
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Plane _targetPlane = new Plane();
            Plane _worldPlane = new Plane();
            GH_ObjectWrapper _abstractSocket = new GH_ObjectWrapper();
            Socket _clientSocket;

            if (!DA.GetData(0, ref _abstractSocket)) return;
            if (!DA.GetData(1, ref _targetPlane)) return;
            if (!DA.GetData(2, ref _worldPlane)) return;

            _abstractSocket.CastTo(out _clientSocket);

            double _targetX = _targetPlane.OriginX;
            double _targetY = _targetPlane.OriginY;
            double _targetZ = _targetPlane.OriginZ;

            Transform rotMatrix = Transform.ChangeBasis(_targetPlane, _worldPlane);
            double _targetA = Rhino.RhinoMath.ToDegrees(Math.Atan2(rotMatrix[2, 1], rotMatrix[2, 2]));
            double _targetB = Rhino.RhinoMath.ToDegrees(-Math.Atan2(rotMatrix[2, 0], Math.Sqrt(rotMatrix[2, 1] * rotMatrix[2, 1] + rotMatrix[2, 2] * rotMatrix[2, 2])));
            double _targetC = Rhino.RhinoMath.ToDegrees(Math.Atan2(rotMatrix[1, 0], rotMatrix[0, 0]));





            // -----------------------------------------------------

            string req = readMessageRequest("MYPOS");

            byte[] messageReq = Encoding.UTF8.GetBytes(req);

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, BitConverter.ToString(messageReq));
            byte[] bytes = new byte[256];
            int sentBytes = 0;
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "reqLength: " + req.Length.ToString() + "  messageReqLength: " + messageReq.Length.ToString());

                try
                {
                    sentBytes = _clientSocket.Send(messageReq);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Sent " + sentBytes.ToString() + " bytes as " + req);
                    //i = _clientSocket.Receive(bytes);
                    //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Received :{0} bytes." + i.ToString());
                }
                catch (SocketException e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "SocketException :{0}" + e.ToString());
                }


            DA.SetData(0, _targetX);
            DA.SetData(1, _targetY);
            DA.SetData(2, _targetZ);
            DA.SetData(3, _targetA);
            DA.SetData(4, _targetB);
            DA.SetData(5, _targetC);
        }

        string readMessageRequest(string varName)
        {
            Random rnd = new Random();
            string messageId = (rnd.Next(10, 100)).ToString();
            string functionType = "0";
            string varLengthHex = varName.Length.ToString();
            //string reqLength = (varName.Length + functionType.Length + varLengthHex.Length).ToString("X2");
            string reqLength = (varName.Length + 3).ToString();
            string messageReq = messageId + reqLength + functionType + varLengthHex + varName;

            return messageReq;

        }

        //string readMessageRequest(string varName)
        //{
        //    Random rnd = new Random();
        //    string messageId = (rnd.Next(10, 100)).ToString();
        //    string functionType = "0";
        //    string varLengthHex = varName.Length.ToString("X2");
        //    //string reqLength = (varName.Length + functionType.Length + varLengthHex.Length).ToString("X2");
        //    string reqLength = (varName.Length + 3).ToString("X2");
        //    string messageReq = messageId + reqLength + functionType + varLengthHex + varName;

        //    return messageReq;

        //}
        //void writeMessageStructure(string varName, string varValue, bool debugInfo)
        // {
        //     int varNameLength = varName.Length;
        //     int flag = 1;
        //     int varValueLength = varValue.Length;
        //     int reqLength = varNameLength + 3 + 2 + varValueLength;
        //     Random rnd = new Random();
        //     int messageId = rnd.Next(1, 100); 
        //     //string message = "!HHBH'"+varNameLength.ToString()+"s"+"H"+varValueLength.ToString()+"s", messageId, 

        // }


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
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5e564858-c34b-45ee-b728-b8209b9ad7fe"); }
        }
    }
}
