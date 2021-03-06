﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Simulacrum
{
    public class WriteVariable : GH_Component
    {
        private Socket _clientSocket;
        private string _oResponse;

        /// <summary>
        /// Initializes a new instance of the VariableWrite class.
        /// </summary>
        public WriteVariable()
          : base("KRC Write", "KRC Write",
              "Writes data to a global KRC variable.",
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
            pManager.AddTextParameter("Input", "Input", "Data that needs to be written to the KRC variable.", GH_ParamAccess.item);
            //[3] Run
            pManager.AddBooleanParameter("Run", "Run", "Write variable to KRC", GH_ParamAccess.item);
            //[4] Refresh Rate
            pManager.AddIntegerParameter("Refresh Rate", "Refresh Rate",
                "Time between updates in milliseconds (ms)", GH_ParamAccess.item, 100);
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
            GH_ObjectWrapper abstractSocket = new GH_ObjectWrapper();
            string varWrite = "";
            string varData = "";
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
            if (!DA.GetData(1, ref varWrite)) return;
            if (!DA.GetData(2, ref varData)) return;
            if (!DA.GetData(3, ref run)) return;
            if (!DA.GetData(4, ref refreshRate)) return;

            if (Encoding.ASCII.GetBytes(varData).Length > 255)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,"MessageSize is too big, messages can only contain a maximum of 255 bytes. Yours is: " + Encoding.ASCII.GetBytes(varData).Length);
            }

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


            if (run)
            {
                string response = Util.WriteVariable(ref _clientSocket, varWrite, varData, this);
                _oResponse = response;

                if (this.Params.Input[3].Sources[0].GetType() == typeof(GH_BooleanToggle))
                {
                    GH_Document doc = OnPingDocument();
                    doc?.ScheduleSolution(refreshRate, ScheduleCallback);
                }
            }

            DA.SetData(0, _oResponse);
        }


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
            get { return new Guid("c8899a75-2a24-4c7b-bf45-1c47782075d4"); }
        }
        #endregion

    }
}