using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Net.Sockets;
using Grasshopper.Kernel.Types;
using System.Text;
using Simulacrum.Properties;

namespace Simulacrum
{
    public class SenderReceiverExplicit : GH_Component
    {

        public SenderReceiverExplicit()
          : base("IO:Explicit", "IO:Explicit", "Reads and writes to KRC", "VirtualRobot", "KukaVarProxy")
        {
        }
        Socket _clientSocket;
        Util _messenger;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket Object", "Socket In", "Incoming object from TCP Client", GH_ParamAccess.item);
            //pManager.AddPlaneParameter("Target Plane", "Target Plane", "Current position and orientation of TCP", GH_ParamAccess.item);
            //pManager[1].Optional = true;
            //pManager.AddPlaneParameter("World Plane", "World Plane", "Reference plane for A,B,C calculation", GH_ParamAccess.item, Plane.WorldXY);
            //pManager[2].Optional = true;
            pManager.AddTextParameter("Variable to Read", "Var Read", "Variable to read in KRC", GH_ParamAccess.item, "$POS_ACT");
            pManager.AddTextParameter("Variable to Write", "Var Write", "Variable to write to in KRC", GH_ParamAccess.item, "MYPOS");
            pManager.AddBooleanParameter("Read Trigger", "Read Trigger", "Read variable from KRC", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write Trigger", "Write Trigger", "Write variable to KRC", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Value Read", "Val Read", "Value obtained from VarRead", GH_ParamAccess.item);
            pManager.AddTextParameter("Value Write", "Val Write", "Value written to VarWrite", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Plane _targetPlane = new Plane();
            Plane _worldPlane = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), new Vector3d(0, 1, 0));
            GH_ObjectWrapper _abstractSocket = new GH_ObjectWrapper();
            string _variableRead = "NA";
            string _variableWrite = "NA";
            bool _triggerRead = false;
            bool _triggerWrite = false;

            if(_clientSocket == null)
            {
                if (!DA.GetData(0, ref _abstractSocket)) return;
                    _abstractSocket.CastTo(ref _clientSocket);
            }

            if (!DA.GetData(1, ref _targetPlane)) return;
            if (!DA.GetData(2, ref _worldPlane)) return;
            if (!DA.GetData(3, ref _variableRead)) return;
            if (!DA.GetData(4, ref _variableWrite)) return;
            if (!DA.GetData(5, ref _triggerRead)) return;
            if (!DA.GetData(6, ref _triggerWrite)) return;


            if (_triggerRead)
            {
                string response = Util.ReadVariable(ref _clientSocket, _variableRead, this);
                DA.SetData(0, response);
            }

            if (_triggerWrite)
            {
                string targetE6 = Util.calculateTargetE6Pos(_targetPlane, _worldPlane);
                string response = Util.WriteVariable(ref _clientSocket, _variableWrite,"hi",this );
                DA.SetData(1, response);
            }

            GH_Document doc = OnPingDocument();
            if (doc != null)
            {
                // Schedule loop for every 20ms
                doc.ScheduleSolution(20, ScheduleCallback);
            }

        }

        #region callbacks
        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }
        #endregion

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.control;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5e564858-c34b-45ee-b728-b8209b9ad7fe"); }
        }
    }

    public class SenderReceiverKeyboard : GH_Component
    {

        public SenderReceiverKeyboard()
          : base("IO:Keys", "IO:Keys", "Reads and writes to KUKAVARPROXY through Keyboard", "COM", "")
        {
        }

        Socket _clientSocket;
        Util _messenger;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket Object", "Client", "Incoming object from SocketClient", GH_ParamAccess.item);
            pManager.AddTextParameter("Keyboard Input", "Key", "Output from KeyRead component", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write Trigger", "Write", "Write variable to KRC", GH_ParamAccess.item);
            pManager.AddNumberParameter("Movement Resolution", "Step", "Step size for keyboard movement in MM", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Keyboard Output", "KeyOut", "Processed command", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper _abstractSocket = new GH_ObjectWrapper();
            string _keyInput = "None";
            bool _triggerWrite = false;
            long _stepResolution = 0;


            if (_clientSocket == null)
            {
                if (!DA.GetData(0, ref _abstractSocket)) return;
                _abstractSocket.CastTo(ref _clientSocket);
            }
            if (!DA.GetData(1, ref _keyInput)) return;
            if (!DA.GetData(2, ref _triggerWrite)) return;

            if (!DA.GetData(3, ref _stepResolution)) return;


            if (_triggerWrite)
            {
                _messenger.keyboardLoop(_keyInput, _stepResolution,  ref _clientSocket, this);
            }
            DA.SetData(0, _keyInput);


        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.keysend;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("530c3157-8996-40ec-a310-d167fedd01a7"); }
        }
    }

}
