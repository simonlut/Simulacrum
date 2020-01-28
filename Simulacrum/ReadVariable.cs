using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Simulacrum
{
    public class ReadVariable : GH_Component
    {
        #region Fields
        Socket _clientSocket;
        #endregion

        #region gh_methods
        /// <summary>
        /// Initializes a new instance of the VariableRead class.
        /// </summary>
        public ReadVariable()
          : base("KRC Read", "KRC Read",
              "Reads Variables from KRC",
              "VirtualRobot", "KukaVarProxy")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //[0] Refresh rate.
            pManager.AddGenericParameter("Socket", "Socket", "Incoming object from TCP Client", GH_ParamAccess.item);
            //[1] Refresh rate.
            pManager.AddTextParameter("Variable to Read", "Var Read", "Variable to read in KRC", GH_ParamAccess.item, "$POS_ACT");
            //[2] Refresh rate.
            pManager.AddBooleanParameter("Read Trigger", "Read Trigger", "Read variable from KRC", GH_ParamAccess.item);
            //[3] Refresh rate.
            pManager.AddIntegerParameter("Refresh Rate", "Refresh Rate",
                "Time between updates in milliseconds (ms)", GH_ParamAccess.item, 20);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Value Read", "Val Read", "Value obtained from VarRead", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Fields
            GH_ObjectWrapper abstractSocket = new GH_ObjectWrapper();
            string varRead = "";
            bool triggerRead = false;
            int refreshRate = 0;

            bool getSocket = DA.GetData(0, ref abstractSocket);

            //Check input
            if (_clientSocket == null)
            {
                if (!getSocket) return;
                abstractSocket.CastTo(ref _clientSocket);
            }
            else if (_clientSocket != null && !getSocket)
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
            if (!DA.GetData(1, ref varRead)) return;
            if (!DA.GetData(2, ref triggerRead)) return;
            if (!DA.GetData(3, ref refreshRate)) return;

            //If trigger is pressed, read data and output.
            if (triggerRead)
            {
                string response = Util.ReadVariable(ref _clientSocket, varRead, this);
                DA.SetData(0, response);
            }

            GH_Document doc = OnPingDocument();
            doc?.ScheduleSolution(refreshRate, ScheduleCallback);
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
            get { return new Guid("a2bdb476-ceb8-4f76-b231-45be72bacddb"); }
        }
        #endregion

    }
}