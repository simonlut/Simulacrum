using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Simulacrum
{
    public class _SimulacrumInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Simulacrum";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("c3730b6d-e9b6-490a-a1d5-36bc4b5b15bb");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
