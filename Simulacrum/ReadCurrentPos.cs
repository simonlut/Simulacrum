using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Simulacrum
{
    public class ReadCurrentPos : GH_Component
    {
        #region Fields
        Socket _clientSocket;
        private E6POS CurrentPos;
        private E6AXIS CurrentAngles;
        #endregion

        #region gh_methods
        /// <summary>
        /// Initializes a new instance of the ReadCurrentPos class.
        /// </summary>
        public ReadCurrentPos()
          : base("Read ActPos", "ActPos",
              "Read the current position of the Kuka Robot, E6POS",
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
            //[1] Run the reading.
            pManager.AddBooleanParameter("Run", "Run", "Run to read, for continues reading, plug-in a boolean toggle.", GH_ParamAccess.item);
            //[2] Refresh rate.
            pManager.AddIntegerParameter("Refresh Rate", "Refresh Rate",
                "Time between updates in milliseconds (ms)", GH_ParamAccess.item, 50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //[0] Axis Values
            pManager.AddNumberParameter("Axis Values", "Axis Values", "Axis values of the current position of the KUKA", GH_ParamAccess.list);
            //[1] TCP Plane
            pManager.AddPlaneParameter("TCP Plane", "TCP Plane", "Get the current TCP plane of the Robot.",
                GH_ParamAccess.item);
            //[2] ABC 
            pManager.AddNumberParameter("XYZ", "XYZ", "XYZ coordinates of the plane.",
                GH_ParamAccess.list);
            //[3] XYZ 
            pManager.AddNumberParameter("ABC", "ABC", "Rotations around axis A = Z, B = Y, C = X",
                GH_ParamAccess.list);
            //[4] E6POS String
            pManager.AddTextParameter("E6POS", "E6POS", "E6POS String",
                GH_ParamAccess.item);
            //[5] E6AXIS String
            pManager.AddTextParameter("E6AXIS", "E6AXIS", "E6AXIS String",
                GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Fields
            GH_ObjectWrapper abstractSocket = new GH_ObjectWrapper();
            bool triggerRead = false;
            int refreshRate = 0;

            E6POS currentPos = new E6POS();
            E6AXIS currentAngles = new E6AXIS();

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
            if (!DA.GetData(1, ref triggerRead)) return;
            if (!DA.GetData(2, ref refreshRate)) return;
            if (refreshRate < 15)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "WARNING: Refresh rate too low, this can cause performance issues for grasshopper. The maximum robot read speed is 5ms (for all messages)");
            }
            if (refreshRate < 5)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Refresh Rate too low. Absolute maximum speed is 5ms. This is not recommended. Try more in the region of ~20-70 ms");
                return;
            }

            //If trigger is pressed, read data and output.
            if (triggerRead)
            {
                string response = Util.ReadVariable(ref _clientSocket, "$POS_ACT", this);
                    currentPos.DeserializeE6POS(response);
                    CurrentPos = currentPos;

                    string response2 = Util.ReadVariable(ref _clientSocket, "$AXIS_ACT", this);
                    currentAngles.DeserializeE6AXIS(response2);
                    CurrentAngles = currentAngles;


            }

            if (!CurrentAngles.IsNull() && !CurrentPos.IsNull())
            {
                DA.SetDataList(0, CurrentAngles.GetAxisValues());
                DA.SetData(1, new GH_Plane(CurrentPos.GetPlane()));
                DA.SetDataList(2, new List<double> {CurrentPos.X, CurrentPos.Y, CurrentPos.Z});
                DA.SetDataList(3, new List<double> { CurrentPos.A, CurrentPos.B, CurrentPos.C });
                DA.SetData(4, CurrentPos.SerializedString);
                DA.SetData(5, CurrentAngles.SerializedString);
            }

            if (this.Params.Input[2].Sources[0].GetType() == typeof(GH_BooleanToggle) && triggerRead)
            {
                GH_Document doc = OnPingDocument();
                doc?.ScheduleSolution(refreshRate, ScheduleCallback);
            }

        }
        #endregion

        #region callbacks
        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }
        #endregion

        #region properties
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
            get { return new Guid("915aaf41-9c22-4dd6-8d10-ed267ff587f3"); }
        }
        #endregion
    }
}