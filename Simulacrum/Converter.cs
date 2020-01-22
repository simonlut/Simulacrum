using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Simulacrum
{
    public class Converter : GH_Component
    {
        #region Fields
        Simulacrum.E6POS e6Pos = new E6POS();
        Simulacrum.E6AXIS e6Axis = new E6AXIS();
        Simulacrum.FRAME frame = new FRAME();

        #endregion

        /// <summary>
        /// Initializes a new instance of the Converter class.
        /// </summary>
        public Converter()
          : base("Converter", "Converter",
              "Converts standard KUKA datatypes to common datatypes.",
              "VirtualRobot", "KukaVarProxy")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("E6POS", "E6POS", "E6POS String.", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddTextParameter("E6AXIS", "E6AXIS", "E6AXIS String.", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("FRAME", "FRAME", "FRAME String.", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("E6POS Names", "E6POS Names", "List of E6POS Names.", GH_ParamAccess.list);
            pManager.AddTextParameter("E6POS Values", "E6POS Values", "E6POS List of doubles.", GH_ParamAccess.list);
            pManager.AddTextParameter("E6AXIS Names", "E6AXIS Names", "List of E6AXIS Names", GH_ParamAccess.list);
            pManager.AddTextParameter("E6AXIS Values", "E6AXIS Values", "E6AXIS List of doubles.", GH_ParamAccess.list);
            pManager.AddTextParameter("FRAME Names", "FRAME Names", "FRAME String. List of doubles.", GH_ParamAccess.list);
            pManager.AddTextParameter("FRAME Values", "FRAME Values", "FRAME String. List of doubles.", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            string E6POS = "";
            string E6AXIS = "";
            string FRAME = "";

            DA.GetData(0, ref E6POS);
            DA.GetData(1, ref E6AXIS);
            DA.GetData(2, ref FRAME);

            if (!string.IsNullOrEmpty(E6POS))
            {
                e6Pos.DeserializeE6POS(E6POS);
                DA.SetDataList(0, e6Pos.GetNameList());
                DA.SetDataList(1, e6Pos.GetValuesList());
            }

            if (!string.IsNullOrEmpty(E6AXIS))
            {
                e6Axis.DeserializeE6AXIS(E6AXIS);
                DA.SetDataList(2, e6Axis.GetNameList());
                DA.SetDataList(3, e6Axis.GetValuesList());
            }

            if (!string.IsNullOrEmpty(FRAME))
            {
                frame.DeserializeFrame(FRAME);
                DA.SetDataList(4, frame.GetNameList());
                DA.SetDataList(5, frame.GetValuesList());
            }

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
            get { return new Guid("6a93bee8-1160-4612-bbba-a98217f46345"); }
        }
    }
}