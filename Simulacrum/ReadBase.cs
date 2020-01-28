using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Simulacrum
{
    public class ReadBase : GH_Component
    {
        #region Fields
        Socket _clientSocket;

        FRAME frameBase = new FRAME();
        FRAME frameTool = new FRAME();

        #endregion

        /// <summary>
        /// Initializes a new instance of the ReadBase class.
        /// </summary>
        public ReadBase()
          : base("Read Base", "Read Base",
              "Read standard defined values such as: base, tool and speed.",
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
            pManager.AddBooleanParameter("Run", "Run", "Run to read", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("BASE", "BASE", "Get the base FRAME that is used in this .src file, Output: XYZABC",
                GH_ParamAccess.list);
            pManager.AddPlaneParameter("BASE plane", "BASE Plane", "The plane set for the BASE in the KUKA",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("TOOL", "TOOL", "Get the tool FRAME that is used in this .src file, Output: XYZABC",
                GH_ParamAccess.list);
            pManager.AddPlaneParameter("TOOL plane", "TOOL Plane", "The plane set for the TOOL in the KUKA",
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
            bool run = false;
            
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
            if (!DA.GetData(1, ref run)) return;

            //If trigger is pressed, read data and output.
            if (run)
            {
                    string response = Util.ReadVariable(ref _clientSocket, "$BASE", this);
                    string response2 = Util.ReadVariable(ref _clientSocket, "$TOOL", this);
                    frameBase.DeserializeFrame(response);
                    frameTool.DeserializeFrame(response2);

            }

            DA.SetDataList(0, frameBase.GetValuesList());
            DA.SetData(1, new Plane(frameBase.GetPlane()));
            DA.SetDataList(2, frameTool.GetValuesList());
            DA.SetData(3, new Plane(frameTool.GetPlane()));
        }

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
            get { return new Guid("a7937e79-cc7a-4a1a-a142-c5879cc3e084"); }
        }
    }
}