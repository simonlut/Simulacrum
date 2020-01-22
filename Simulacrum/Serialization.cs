using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Simulacrum
{
    //Prefined KRC Datatypes:

    //The following data types for motion programming are predefined in the controller software.
    //STRUC AXIS REAL A1, A2, A3, A4, A5, A6
    //The components A1 to A6 of the structure AXIS are angle values(rotational axes) or translation values(translational axes) for the axis--specific movement of robot axes 1 to 6.
    //STRUC E6AXIS REAL A1, A2, A3, A4, A5, A6, E1, E2, E3, E4, E5, E6
    //The angle values or translation values for the external axes are stored in the additional
    //components E1 to E6.
    //STRUC FRAME REAL X, Y, Z, A, B, C
    //The space coordinates are stored in X, Y, Z and the orientation of the coordinate system is
    //stored in A, B, C.
    //STRUC POS REAL X, Y, Z, A, B, C, INT S, T
    //The additional components S (Status) and T (Turn) can be used for the unambiguous definition of axis positions.
    //STRUC E6POS REAL X, Y, Z, A, B, C, E1, E2, E3, E4, E5, E6, INT S, T
    public struct E6POS
    {
        public double X;
        public double Y;
        public double Z;
        public double A;
        public double B;
        public double C;
        public double S;
        public double T;
        public double E1;
        public double E2;
        public double E3;
        public double E4;
        public double E5;

        public string SerializedString;

        /// <summary>
        /// De-serialize E6POS string to doubles.
        /// </summary>
        /// <param name="encodedE6"></param>
        public void DeserializeE6POS(string encodedE6)
        { 
            SerializedString = encodedE6;
            Regex _regex = new Regex(@"\W([+-]?(?=\.\d|\d)(?:\d+)?(?:\.?\d*))(?:[eE]([+-]?\d+))?");
            MatchCollection result = _regex.Matches(encodedE6);

            List<string> captures = new List<string>();

            for (int i = 0; i < result.Count; i++)
            {
                string val = result[i].Captures[0].Value.ToString();
                string val2 = val.Trim();
                captures.Add(val2);
            }
            List<string> results = captures;
            //
            X = Convert.ToDouble(results[0]);
            Y = Convert.ToDouble(results[1]);
            Z = Convert.ToDouble(results[2]);
            A = Convert.ToDouble(results[3]);
            B = Convert.ToDouble(results[4]);
            C = Convert.ToDouble(results[5]);
            S = Convert.ToDouble(results[6]);
            T = Convert.ToDouble(results[7]);
            E1 = Convert.ToDouble(results[8]);
            E2 = Convert.ToDouble(results[9]);
            E3 = Convert.ToDouble(results[10]);
            E4 = Convert.ToDouble(results[11]);
            E5 = Convert.ToDouble(results[12]);
            

        }

        /// <summary>
        /// Serializes doubles to a E6POS string.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public string SerializeE6POS(double x, double y, double z, double a, double b, double c)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            string valueString = "{E6POS: X " + X + ", Y " + Y + ", Z " + Z + ", A " + A + ", B " + B + ", C " + C + ", E1 0.0, E2 0.0, E3 0.0, E4 0.0}";
            SerializedString = valueString;
            return SerializedString;
        }

        /// <summary>
        /// Update E6POS values with new values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public void UpdateE6Pos(double x, double y, double z, double a, double b, double c)
        {
            X += x;
            Y += y;
            Z += z;
            A += a;
            B += b;
            C += c;
            //e1Value += deltaArray[6];
            //e2Value += deltaArray[7];
            //e3Value += deltaArray[8];
            string valueString = "{E6POS: X " + X + ", Y " + Y + ", Z " + Z + ", A " + A + ", B " + B + ", C " + C + ", E1 0.0, E2 0.0, E3 0.0, E4 0.0}";
            SerializedString = valueString;
        }

        /// <summary>
        /// Converts an E6POS datatype to a Rhino plane, relative to XY plane.
        /// </summary>
        /// <returns></returns>
        public Plane GetPlane()
        {
            double num = Math.PI / 180.0;
            Plane result = Plane.WorldXY;

            result.Translate(new Vector3d(X, Y, Z));

            result.Rotate(num * A, result.ZAxis, result.Origin);
            result.Rotate(num * B, result.YAxis, result.Origin);
            result.Rotate(num * C, result.XAxis, result.Origin);

            return result;
        }

        public List<string> GetNameList()
        {
            return new List<string> { "X", "Y", "Z", "A", "B", "C", "S", "T", "E1", "E2", "E3", "E4", "E5" };
        }

        public List<double> GetValuesList()
        {
            return new List<double> {X, Y, Z, A, B, C, S, T, E1, E2, E3, E4, E5};
        }

        public bool IsNull()
        {
            if (SerializedString == null) return true;
            else
            {
                return false;
            }
        }

    }

    public struct FRAME
    {
        public double X;
        public double Y;
        public double Z;
        public double A;
        public double B;
        public double C;

        public string SerializedString;

        /// <summary>
        /// De-serialize E6POS string to doubles.
        /// </summary>
        /// <param name="encodedE6"></param>
        public void DeserializeFrame(string encodedE6)
        {
            SerializedString = encodedE6;
            Regex _regex = new Regex(@"\W([+-]?(?=\.\d|\d)(?:\d+)?(?:\.?\d*))(?:[eE]([+-]?\d+))?");
            MatchCollection result = _regex.Matches(encodedE6);

            List<string> captures = new List<string>();

            for (int i = 0; i < result.Count; i++)
            {
                string val = result[i].Captures[0].Value.ToString();
                string val2 = val.Trim();
                captures.Add(val2);
            }
            List<string> results = captures;
            //
            X = Convert.ToDouble(results[0]);
            Y = Convert.ToDouble(results[1]);
            Z = Convert.ToDouble(results[2]);
            A = Convert.ToDouble(results[3]);
            B = Convert.ToDouble(results[4]);
            C = Convert.ToDouble(results[5]);


        }

        /// <summary>
        /// Serializes doubles to a E6POS string.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public string SerializeFrame(double x, double y, double z, double a, double b, double c)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            string valueString = "{FRAME: X " + X + ", Y " + Y + ", Z " + Z + ", A " + A + ", B " + B + ", C " + C + "}";
            SerializedString = valueString;
            return SerializedString;
        }

        /// <summary>
        /// Update E6POS values with new values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public void UpdateFrame(double x, double y, double z, double a, double b, double c)
        {
            X += x;
            Y += y;
            Z += z;
            A += a;
            B += b;
            C += c;

            string valueString = "{E6POS: X " + X + ", Y " + Y + ", Z " + Z + ", A " + A + ", B " + B + ", C " + C + "}";
            SerializedString = valueString;
        }

        /// <summary>
        /// Converts an E6POS datatype to a Rhino plane, relative to XY plane.
        /// </summary>
        /// <returns></returns>
        public Plane GetPlane()
        {
            double num = Math.PI / 180.0;
            Plane result = Plane.WorldXY;

            result.Translate(new Vector3d(X, Y, Z));

            result.Rotate(num * A, result.ZAxis, result.Origin);
            result.Rotate(num * B, result.YAxis, result.Origin);
            result.Rotate(num * C, result.XAxis, result.Origin);

            return result;
        }

        public List<string> GetNameList()
        {
            return new List<string> { "X", "Y", "Z", "A", "B", "C" };
        }

        public List<double> GetValuesList()
        {
            return new List<double> { X, Y, Z, A, B, C};
        }

        public bool IsNull()
        {
            if (SerializedString == null) return true;
            else
            {
                return false;
            }
        }

    }

    public struct E6AXIS
    {
        public double A1;
        public double A2;
        public double A3;
        public double A4;
        public double A5;
        public double A6;
        public double E1;
        public double E2;
        public double E3;
        public double E4;
        public double E5;
        public double E6;

        public string SerializedString;

        /// <summary>
        /// De-serialize E6POS string to doubles.
        /// </summary>
        /// <param name="encodedE6"></param>
        public void DeserializeE6AXIS(string encodedE6)
        {
            SerializedString = encodedE6;
            Regex _regex = new Regex(@"\W([+-]?(?=\.\d|\d)(?:\d+)?(?:\.?\d*))(?:[eE]([+-]?\d+))?");
            MatchCollection result = _regex.Matches(encodedE6);

            List<string> captures = new List<string>();

            for (int i = 0; i < result.Count; i++)
            {
                string val = result[i].Captures[0].Value.ToString();
                string val2 = val.Trim();
                captures.Add(val2);
            }
            List<string> results = captures;

            A1 = Convert.ToDouble(results[0]);
            A2 = Convert.ToDouble(results[1]);
            A3 = Convert.ToDouble(results[2]);
            A4 = Convert.ToDouble(results[3]);
            A5 = Convert.ToDouble(results[4]);
            A6 = Convert.ToDouble(results[5]);
            E1 = Convert.ToDouble(results[6]);
            E2 = Convert.ToDouble(results[7]);
            E3 = Convert.ToDouble(results[8]);
            E4 = Convert.ToDouble(results[9]);
            E5 = Convert.ToDouble(results[10]);
            E6 = Convert.ToDouble(results[11]);
        }

        /// <summary>
/// Serialize doubles to a E6AXIS string
/// </summary>
/// <param name="A1"></param>
/// <param name="A2"></param>
/// <param name="A3"></param>
/// <param name="A4"></param>
/// <param name="A5"></param>
/// <param name="A6"></param>
/// <param name="E1"></param>
/// <param name="E2"></param>
/// <param name="E3"></param>
/// <param name="E4"></param>
/// <param name="E5"></param>
/// <param name="E6"></param>
/// <returns></returns>
        public string SerializeE6AXIS(double A1, double A2, double A3, double A4, double A5, double A6, 
            double E1 = 0.0, double E2 = 0.0, double E3 = 0.0, double E4 = 0.0, double E5 = 0.0, double E6 = 0.0)
        {

            this.A1 = A1;
            this.A2 = A2;
            this.A3 = A3;
            this.A4 = A4;
            this.A5 = A5;
            this.A6 = A6;
            this.E1 = E1;
            this.E2 = E2;
            this.E3 = E3;
            this.E4 = E4;
            this.E5 = E5;
            this.E6 = E6;


            string valueString = "{E6AXIS: A1 " + Math.Round(A1,1) + ", A2 " + Math.Round(A2,1) + ", A3 " + Math.Round(A3,1) + ", A4 " + Math.Round(A4,1) + ", A5 " + Math.Round(A5,1) + ", A6 " + Math.Round(A6) + ", E1 " 
                                 + Math.Round(E1,1) + ", E2 " + Math.Round(E2,1) + ", E3 " + Math.Round(E3,1) + ", E4 " + Math.Round(E4,1) + ", E5 " + Math.Round(E5,1) + ", E6 " + Math.Round(E6,1) + "}";
            SerializedString = valueString;
            return SerializedString;
        }

        public List<double> GetAxisValues()
        {
            return new List<double>() { A1 ,A2 ,A3 ,A4 ,A5 ,A6 };
        }

        public List<string> GetNameList()
        {
            return new List<string> { "A1", "A2", "A3", "A4", "A5", "A6", "E1", "E2", "E3", "E4", "E5", "E6"};
        }

        public List<double> GetValuesList()
        {
            return new List<double> { A1, A2, A3, A4, A5, A6, E1, E2, E3, E4, E5 , E6};
        }

        public bool IsNull()
        {
            if (SerializedString == null) return true;
            else
            {
                return false;
            }
        }

    }
}



