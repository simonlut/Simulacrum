using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Simulacrum
{
    public class WritePos : GH_Component
    {
        #region Fields
        private Socket _clientSocket;
        private string _writtenValues = "";
        #endregion

        #region gh_Methods
        /// <summary>
        /// Initializes a new instance of the VariableWrite class.
        /// </summary>
        public WritePos()
            : base("Write Position", "Write Pos",
                "Writes position values of the robot, axis values.",
                "VirtualRobot", "KukaVarProxy")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //[0] Socket
            pManager.AddGenericParameter("Socket", "Socket", "Incoming object from TCP Client", GH_ParamAccess.item);
            //[1] Variable To write to
            pManager.AddTextParameter("Variable Write", "Write", "Variable to write to in KRC", GH_ParamAccess.item, "MYPOS");
            //[2] Data to write
            pManager.AddNumberParameter("Axis Values", "Axis Values", "List of Axis values that can be written to the robot.", GH_ParamAccess.list);
            //[3] Run
            pManager.AddBooleanParameter("Run", "Run", "Write variable to KRC", GH_ParamAccess.item);
            //[4] Refresh rate.
            pManager.AddIntegerParameter("Refresh Rate", "Refresh Rate",
                "Time between updates in milliseconds (ms)", GH_ParamAccess.item, 20);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //[0] Written Variable
            pManager.AddTextParameter("Value", "Val", "Value written to VarWrite", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Fields
            E6AXIS e6Axis = new E6AXIS();
            GH_ObjectWrapper abstractSocket = new GH_ObjectWrapper();
            string writeVariable = "";
            List<double> axisValues = new List<double>();
            bool run = false;
            int refreshRate = 0;

            //Check input
            if (_clientSocket == null)
            {
                if (!DA.GetData(0, ref abstractSocket)) return;
                abstractSocket.CastTo(ref _clientSocket);
            }
            else if (_clientSocket != null && !DA.GetData(0, ref abstractSocket))
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
            if (!DA.GetData(1, ref writeVariable)) return;
            if (!DA.GetDataList(2, axisValues)) return;
            if (axisValues.Count != 6) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Make sure to only give 6 axis values.");
            if (!DA.GetData(3, ref run)) return;
            if (!DA.GetData(4, ref refreshRate)) return;

            if (axisValues.Count == 6)
            {

                e6Axis.SerializeE6AXIS(axisValues[0], axisValues[1], axisValues[2], axisValues[3], axisValues[4],
                    axisValues[5]);
            }

            if (run)
            {
                string response = Util.WriteVariable(ref _clientSocket, writeVariable, e6Axis.SerializedString, this);
                _writtenValues = response;

                if (this.Params.Input[3].Sources[0].GetType() == typeof(GH_BooleanToggle))
                {
                    GH_Document doc = OnPingDocument();
                    doc?.ScheduleSolution(refreshRate, ScheduleCallback);
                }
            }

            DA.SetData(0, _writtenValues);

        }
        #endregion

        #region callbacks
        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a7aa30dd-f2ae-4fc9-80f6-fe2dcbf043bf"); }
        }
        #endregion
    }
}