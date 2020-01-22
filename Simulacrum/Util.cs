using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Rhino.Geometry;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security;

namespace Simulacrum
{
    class Util
    {

        public static string calculateTargetE6Pos(Plane _targetPlane, Plane _worldPlane)
        {
            double _targetX = _targetPlane.OriginX;
            double _targetY = _targetPlane.OriginY;
            double _targetZ = _targetPlane.OriginZ;
            Transform rotMatrix = Transform.ChangeBasis(_targetPlane, _worldPlane);
            double _targetA = Rhino.RhinoMath.ToDegrees(Math.Atan2(rotMatrix[2, 1], rotMatrix[2, 2]));
            double _targetB = Rhino.RhinoMath.ToDegrees(-Math.Atan2(rotMatrix[2, 0], Math.Sqrt(rotMatrix[2, 1] * rotMatrix[2, 1] + rotMatrix[2, 2] * rotMatrix[2, 2])));
            double _targetC = Rhino.RhinoMath.ToDegrees(Math.Atan2(rotMatrix[1, 0], rotMatrix[0, 0]));

            E6POS pos = new E6POS();
            return pos.SerializeE6POS(_targetX, _targetY, _targetZ, _targetA, _targetB, _targetC);
        }


        #region Static Methods
        public static string ReadVariable(ref Socket clientSocket, string variableRead, GH_Component component)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));
            if (variableRead == null) throw new ArgumentNullException(nameof(variableRead));
            if (component == null) throw new ArgumentNullException(nameof(component));

            byte[] messageReq = ReadMessageRequest(variableRead, out var outputString);
            byte[] receivedData = new byte[256];
            int receivedBytes = 0;

            try //Try to receive message.
            {
                int sentBytes = clientSocket.Send(messageReq); //Request a specific message according to VarRead variable

                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    "Sent:" + sentBytes.ToString() + " bytes as " + outputString);
                component.Message = "Sent";

                receivedBytes = clientSocket.Receive(receivedData); //Receive data back.
            }
            catch (SocketException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Read Variable :{0}" + e.ToString());
            }
            catch (ArgumentNullException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Read Variable :{0}" + e.ToString());

            }
            catch (ObjectDisposedException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Read Variable :{0}" + e.ToString());
            }
            catch (SecurityException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Write Variable Receive :{0}" + e.ToString());
            }

            //Format received data to extract value.
            MessageReceiveFormat response = new MessageReceiveFormat(receivedData);

            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Received:" + receivedBytes.ToString() + " bytes as" + response._varValue);
            component.Message = "Received";

            return response._varValue;
        }


        public static string WriteVariable(ref Socket clientSocket, string variableWrite, string dataWrite, GH_Component component)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));
            if (variableWrite == null) throw new ArgumentNullException(nameof(variableWrite));
            if (dataWrite == null) throw new ArgumentNullException(nameof(dataWrite));
            if (component == null) throw new ArgumentNullException(nameof(component));

            byte[] messageReq = WriteMessageRequest(variableWrite, dataWrite, out string outputString);
            byte[] receivedData = new byte[256];
            int receivedBytes = 0;
            try
            {
                int sentBytes = clientSocket.Send(messageReq);
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    "Sent:" + sentBytes.ToString() + " bytes as " + outputString);
                component.Message = "Sent";
                receivedBytes = clientSocket.Receive(receivedData);
            }
            catch (SocketException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "SocketException :{0}" + e.ToString());
            }
            catch (ArgumentNullException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Write Variable :{0}" + e.ToString());

            }
            catch (ObjectDisposedException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Write Variable :{0}" + e.ToString());
            }
            catch (SecurityException e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Write Variable Receive :{0}" + e.ToString());
            }

            MessageReceiveFormat response = new MessageReceiveFormat(receivedData);
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Received:" + receivedBytes.ToString() + " bytes as " + response._varValue);
            component.Message = "Received";
            return response._varValue;
        }

        /// <summary>
        /// 2 bytes Id (uint16)
        /// 2 bytes for content length (uint16)
        /// 1 byte for read/write mode (0=Read)
        /// 2 bytes for the variable name length (uint16)
        /// N bytes for the variable name to be read (ASCII)
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="outputString"></param>
        /// <returns></returns>
        private static byte[] ReadMessageRequest(string varName, out string outputString)
        {
            Random rnd = new Random();
            string id = (rnd.Next(0, 99)).ToString("X2"); // 2Bytes Id (uint16)
            string contentLength = (varName.Length + 3).ToString("X2"); ; //2 bytes contentLength (uint16)
            string mode = "0"; //1 byte for read/write format
            string nameLength = varName.Length.ToString("X2"); //2 bytes for the variable name length

            outputString = id + contentLength + mode + nameLength + varName;
            MessageSendFormat readRequest = new MessageSendFormat(id, contentLength, mode, nameLength, varName);

            return readRequest.messageReady;
        }
        #endregion

        /// <summary>
        /// 2 bytes Id (uint16)
        /// 2 bytes for content length (uint16)
        /// 1 byte for read/write mode (1=Write)
        /// 2 bytes for the variable name length (uint16)
        /// N bytes for the variable name to be written (ASCII)
        /// 2 bytes for the variable value length (uint16)
        /// M bytes for the variable value to be written (ASCII)
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="varValue"></param>
        /// <param name="outputString"></param>
        /// <returns></returns>
        private static byte[]  WriteMessageRequest(string varName, string varValue, out string outputString)
        {
            Random rnd = new Random();
            string messageId = (rnd.Next(0, 99)).ToString("X2"); //2 bytes for id
            string reqLength = (varName.Length + varValue.Length + 5).ToString("X2"); //2 bytes for content length
            string functionType = "1"; //1 byte for write mode
            string varNameLength = varName.Length.ToString("X2"); //2 bytes for variable name length
            //N bytes for variable name to be written
            string varValueLength = varValue.Length.ToString("X2"); //2 bytes for the variable value length.
            //N bytes for the variable value to be written.

            outputString = messageId + reqLength + functionType + varNameLength + varName + varValueLength + varValue;
            MessageSendFormat readRequest = new MessageSendFormat(messageId, reqLength, functionType, varNameLength, varName, varValueLength, varValue);

            return readRequest.messageReady;
        }

        //TODO: Check code.
        public class MessageReceiveFormat
        {
            public ushort _messageId;
            public ushort _reqLength;
            public int _functionType;
            public ushort _varLength;
            public string _varValue;
            public int _temp;
            public MessageReceiveFormat(byte[] messageResponse)
            {

                _messageId = (ushort)messageResponse[1];
                _reqLength = (ushort)messageResponse[3];
                _functionType = (int)messageResponse[4];
                _varLength = (ushort)messageResponse[6];

                byte[] _varValueBytes = new byte[_varLength];
                Buffer.BlockCopy(messageResponse, 7, _varValueBytes, 0, _varLength);
                _varValue = Encoding.UTF8.GetString(_varValueBytes);
                _temp = _messageId;
            }
        }
        public class MessageSendFormat
        {
            private ushort _reqLength; // In hexadecimal units
            private ushort _messageId; // In hexadecimal units
            public byte _functionType;
            private ushort _varNameLength; // In hexadecimal units
            private string _varName;
            private ushort _varValueLength; // In hexadecimal units
            private string _varValue;
            private string _combined;
            public byte[] messageReady;

            // Read Constructor
            public MessageSendFormat(string messageId, string reqLength, string functionType, string varNameLength, string varName)
            {
                _messageId = Convert.ToUInt16(messageId, 16);
                _reqLength = Convert.ToUInt16(reqLength, 16); // In hexadecimal units
                _functionType = Byte.Parse(functionType);
                _varNameLength = Convert.ToUInt16(varNameLength, 16); // In hexadecimal units
                _varName = varName;
                _combined = messageId + reqLength + functionType + varNameLength + varName;
                formatReadRequest(out messageReady);
            }
            // Write Constructor
            public MessageSendFormat(string messageId, string reqLength, string functionType, string varNameLength, string varName, string varValueLength, string varValue)
            {
                _messageId = Convert.ToUInt16(messageId, 16);
                _reqLength = Convert.ToUInt16(reqLength, 16); // In hexadecimal units
                _functionType = Byte.Parse(functionType);
                _varNameLength = Convert.ToUInt16(varNameLength, 16); // In hexadecimal units
                _varName = varName;
                _varValueLength = Convert.ToUInt16(varValueLength, 16); // In hexadecimal units
                _varValue = varValue;
                _combined = messageId + reqLength + functionType + varNameLength + varName + varValueLength + varValue;
                formatWriteRequest(out messageReady);
            }
            private void formatReadRequest(out byte[] messageReady)
            {
                List<byte[]> byteParts = new List<byte[]>();

                byte[] _messageId_bytes = BitConverter.GetBytes(_messageId);
                Array.Reverse(_messageId_bytes);
                byte[] _reqLength_bytes = BitConverter.GetBytes(_reqLength);
                Array.Reverse(_reqLength_bytes);
                byte[] _functionType_bytes = { _functionType };
                byte[] _varNameLength_bytes = BitConverter.GetBytes(_varNameLength);
                Array.Reverse(_varNameLength_bytes);
                byte[] _varName_bytes = Encoding.UTF8.GetBytes(_varName);
                byteParts.Add(_messageId_bytes);
                byteParts.Add(_reqLength_bytes);
                byteParts.Add(_functionType_bytes);
                byteParts.Add(_varNameLength_bytes);
                byteParts.Add(_varName_bytes);

                int totalArrayLength = _messageId_bytes.Length + _reqLength_bytes.Length +
                                       _functionType_bytes.Length + _varNameLength_bytes.Length +
                                       _varName_bytes.Length;

                messageReady = AppendByteArrays(byteParts, totalArrayLength, _functionType);
            }
            private void formatWriteRequest(out byte[] messageReady)
            {
                List<byte[]> byteParts = new List<byte[]>();

                byte[] _messageId_bytes = BitConverter.GetBytes(_messageId);
                Array.Reverse(_messageId_bytes);
                byte[] _reqLength_bytes = BitConverter.GetBytes(_reqLength);
                Array.Reverse(_reqLength_bytes);
                byte[] _functionType_bytes = { _functionType };
                byte[] _varNameLength_bytes = BitConverter.GetBytes(_varNameLength);
                Array.Reverse(_varNameLength_bytes);
                byte[] _varName_bytes = Encoding.UTF8.GetBytes(_varName);
                byte[] _varValueLength_bytes = BitConverter.GetBytes(_varValueLength);
                Array.Reverse(_varValueLength_bytes);
                byte[] _varValue_bytes = Encoding.UTF8.GetBytes(_varValue);
                byteParts.Add(_messageId_bytes);
                byteParts.Add(_reqLength_bytes);
                byteParts.Add(_functionType_bytes);
                byteParts.Add(_varNameLength_bytes);
                byteParts.Add(_varName_bytes);
                byteParts.Add(_varValueLength_bytes);
                byteParts.Add(_varValue_bytes);

                int totalArrayLength = _messageId_bytes.Length + _reqLength_bytes.Length +
                                       _functionType_bytes.Length + _varNameLength_bytes.Length +
                                       _varName_bytes.Length + _varValueLength_bytes.Length + _varValue_bytes.Length;

                messageReady = AppendByteArrays(byteParts, totalArrayLength, _functionType);
            }
            static byte[] AppendByteArrays(List<byte[]> _byteParts, int _totalArrayLength, int _functionType)
            {
                byte[] outputBytes = new byte[_totalArrayLength];
                Buffer.BlockCopy(_byteParts[0], 0, outputBytes, 0, _byteParts[0].Length);
                Buffer.BlockCopy(_byteParts[1], 0, outputBytes, _byteParts[0].Length, _byteParts[1].Length);
                Buffer.BlockCopy(_byteParts[2], 0, outputBytes, (_byteParts[0].Length + _byteParts[1].Length), _byteParts[2].Length);
                Buffer.BlockCopy(_byteParts[3], 0, outputBytes, (_byteParts[0].Length + _byteParts[1].Length + _byteParts[2].Length), _byteParts[3].Length);
                Buffer.BlockCopy(_byteParts[4], 0, outputBytes, (_byteParts[0].Length + _byteParts[1].Length + _byteParts[2].Length + _byteParts[3].Length), _byteParts[4].Length);
                if (_functionType == 1)
                {
                    Buffer.BlockCopy(_byteParts[5], 0, outputBytes, (_byteParts[0].Length + _byteParts[1].Length + _byteParts[2].Length + _byteParts[3].Length + _byteParts[4].Length), _byteParts[5].Length);
                    Buffer.BlockCopy(_byteParts[6], 0, outputBytes, (_byteParts[0].Length + _byteParts[1].Length + _byteParts[2].Length + _byteParts[3].Length + _byteParts[4].Length + _byteParts[5].Length), _byteParts[6].Length);
                }
                return outputBytes;
            }

        }

        //TODO: fix keyboard loop.
        public void keyboardLoop(string _keyInput, long _stepResolution, ref Socket _clientSocket, GH_Component component)
        {
            string presentValue = Util.ReadVariable(ref _clientSocket, "$POS_ACT", component);
            E6POS currentPos = new E6POS();
            currentPos.DeserializeE6POS(presentValue);

            if (_keyInput != "None")
            {
                var k = keyToVectorCalc(_keyInput, _stepResolution);
                currentPos.UpdateE6Pos(k[0],k[1],k[2],k[3],k[4],k[5]);
            }
        }

        public double[] keyToVectorCalc(string _keyInput, long movementResolution)
        {
            double[] _deltaArray = new double[8];
            switch (_keyInput)
            {
                case "Insert":
                    _deltaArray[0] = movementResolution;
                    break;
                case "Delete":
                    _deltaArray[1] = movementResolution;
                    break;
                case "Home":
                    _deltaArray[2] = movementResolution;
                    break;
                case "End":
                    _deltaArray[3] = movementResolution;
                    break;
                case "PgUp":
                    _deltaArray[4] = movementResolution;
                    break;
                case "PgDn":
                    _deltaArray[5] = movementResolution;
                    break;

            }
                

            return _deltaArray;

        }
    }
}
