using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.XML;

namespace Fuzzware.Common.Encoding
{
    public class EncodingHelper
    {
        public static System.Text.Encoding LoadEncoding(String EncodingName)
        {
            System.Text.Encoding Enc = null;

            try
            {
                Enc = System.Text.Encoding.GetEncoding(EncodingName);
            }
            catch (ArgumentException)
            {
                EncodingInfo[] info = System.Text.Encoding.GetEncodings();
                StringBuilder AllEncodings = new StringBuilder();
                for (int i = 0; i < info.Length; i++)
                    AllEncodings.Append(info[i].Name + "\n");
                Log.Write(MethodInfo.GetCurrentMethod(), "The encoding '" + EncodingName + "' is not supported.  Using default encoding 'ascii'.  \nValid encodings are: \n" + AllEncodings.ToString(), Log.LogType.Warning);
                Enc = new ASCIIEncoding();
            }
            catch (Exception e)
            {
                Log.Write(e);
            }

            return Enc;
        }

        public static byte[] EncodeStringForOutput(String Node, DataSchemaTypeCode TypeCode, Coder.OutputAsType outputType, System.Text.Encoding oCurrentEncoding, System.Text.Encoding oOutputEncoding)
        {
            if (Coder.OutputAsType.Unchanged == outputType)
                return oOutputEncoding.GetBytes(Node);
            return EncodeForOutput(oCurrentEncoding.GetBytes(Node), TypeCode, outputType, oCurrentEncoding, oOutputEncoding);
        }

        public static byte[] EncodeForOutput(byte[] rawNode, DataSchemaTypeCode TypeCode, Coder.OutputAsType outputType, System.Text.Encoding oCurrentEncoding, System.Text.Encoding oOutputEncoding)
        {
            if (Coder.OutputAsType.Unchanged == outputType)
                return rawNode;

            // If the rawNode was encoded somehow (Base64, Hex, etc) then decodedbuffer contains the binary representation of this data

            // Create and encode the output
            Coder Encoder;
            switch (outputType)
            {
                case Coder.OutputAsType.Decoded:
                    // Convert back to unicode
                    String strNode = oCurrentEncoding.GetString(rawNode);
                    // Create the decoder
                    Coder Decoder = Coder.AssignCoder((Coder.EncodingType)TypeCode);
                    // Decode the raw input
                    return Decoder.DecodeToBytes(strNode);
                case Coder.OutputAsType.Base64:
                    Encoder = Coder.AssignCoder(Coder.EncodingType.Base64);
                    return oOutputEncoding.GetBytes(Encoder.Encode(rawNode));
                case Coder.OutputAsType.Hex:
                    Encoder = Coder.AssignCoder(Coder.EncodingType.Hex);
                    return oOutputEncoding.GetBytes(Encoder.Encode(rawNode));
                case Coder.OutputAsType.MSBase64:
                    Encoder = Coder.AssignCoder(Coder.EncodingType.MSBase64);
                    return oOutputEncoding.GetBytes(Encoder.Encode(rawNode));
                case Coder.OutputAsType.BinaryLittleEndian:
                    byte[] Num1 = ConvertNumberToByteArray(rawNode, TypeCode, oCurrentEncoding);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(Num1);
                    return Num1;
                case Coder.OutputAsType.BinaryBigEndian:
                    byte[] Num2 = ConvertNumberToByteArray(rawNode, TypeCode, oCurrentEncoding);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(Num2);
                    return Num2;
                case Coder.OutputAsType.ASCIIString:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), ASCIIEncoding.ASCII, null);
                case Coder.OutputAsType.UTF7String:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UTF7Encoding.UTF7, UTF7Encoding.UTF7.GetPreamble());
                case Coder.OutputAsType.UTF8String:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UTF8Encoding.UTF8, null);
                case Coder.OutputAsType.UTF8StringWithBOM:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UTF8Encoding.UTF8, UTF8Encoding.UTF8.GetPreamble());
                case Coder.OutputAsType.UTF16LEString:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UnicodeEncoding.Unicode, UnicodeEncoding.Unicode.GetPreamble());
                case Coder.OutputAsType.UTF16BEString:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UnicodeEncoding.BigEndianUnicode, UnicodeEncoding.BigEndianUnicode.GetPreamble());
                case Coder.OutputAsType.UTF32LEString:
                    return ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UTF32Encoding.UTF32, UTF32Encoding.UTF32.GetPreamble());
                case Coder.OutputAsType.UTF32BEString:
                    // We only have access to little endian UTF32, so we need to reverse the bytes ourself
                    byte[] ret = ConvertStringEncoding(oCurrentEncoding.GetString(rawNode), UTF32Encoding.UTF32, UTF32Encoding.UTF32.GetPreamble());
                    for (int i = 0; i < ret.Length; i += 4)
                        Array.Reverse(ret, i, 4);
                    return ret;
                default:
                    return rawNode;
            }
        }

        private static byte[] ConvertStringEncoding(String str, System.Text.Encoding Enc, byte[] Preamble)
        {
            if (null == Preamble)
                Preamble = new byte[0];
            // Encode in new string format
            byte[] output = new byte[Preamble.Length + Enc.GetByteCount(str)];
            Array.Copy(Preamble, output, Preamble.Length);
            Array.Copy(Enc.GetBytes(str), 0, output, Preamble.Length, Enc.GetByteCount(str));
            return output;
        }

        private static byte[] ConvertNumberToByteArray(byte[] bytebuffer, DataSchemaTypeCode TypeCode, System.Text.Encoding oEncoding)
        {
            // Convert back to a string
            String NumStr = oEncoding.GetString(bytebuffer);

            // BitConverter returns all the bytes for the type, e.g. 4 bytes for UInt32, even when the number is smaller than 4 bytes 
            // e.g. 1 would return 00000001 (or 01000000 as the case may be).
            switch (TypeCode)
            {
                case DataSchemaTypeCode.UnsignedLong:
                    UInt64 U64Num = 0;
                    if (!UInt64.TryParse(NumStr, out U64Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to UInt64", Log.LogType.Error);
                    return BitConverter.GetBytes(U64Num);
                case DataSchemaTypeCode.Long:
                    Int64 S64Num = 0;
                    if (!Int64.TryParse(NumStr, out S64Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to Int64", Log.LogType.Error);
                    return BitConverter.GetBytes(S64Num);
                case DataSchemaTypeCode.UnsignedInt:
                    UInt32 U32Num = 0;
                    if (!UInt32.TryParse(NumStr, out U32Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to UInt32", Log.LogType.Error);
                    return BitConverter.GetBytes(U32Num);
                case DataSchemaTypeCode.Int:
                    Int32 S32Num = 0;
                    if (!Int32.TryParse(NumStr, out S32Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to Int32", Log.LogType.Error);
                    return BitConverter.GetBytes(S32Num);
                case DataSchemaTypeCode.UnsignedShort:
                    UInt16 U16Num = 0;
                    if (!UInt16.TryParse(NumStr, out U16Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to UInt16", Log.LogType.Error);
                    return BitConverter.GetBytes(U16Num);
                case DataSchemaTypeCode.Short:
                    Int16 S16Num = 0;
                    if (!Int16.TryParse(NumStr, out S16Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to Int16", Log.LogType.Error);
                    return BitConverter.GetBytes(S16Num);
                case DataSchemaTypeCode.UnsignedByte:
                    Byte U8Num = 0;
                    if (!Byte.TryParse(NumStr, out U8Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to Byte", Log.LogType.Error);
                    byte[] retU8 = BitConverter.GetBytes(U8Num);
                    Array.Resize<byte>(ref retU8, 1);
                    return retU8;
                case DataSchemaTypeCode.Byte:
                    SByte S8Num = 0;
                    if (!SByte.TryParse(NumStr, out S8Num))
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + NumStr + "' to SByte", Log.LogType.Error);
                    byte[] retS8 = BitConverter.GetBytes(S8Num);
                    Array.Resize<byte>(ref retS8, 1);
                    return retS8;
                case DataSchemaTypeCode.Hex:
                    Coder Decoder = Coder.AssignCoder(Coder.EncodingType.Hex);
                    byte[] retHex = Decoder.DecodeToBytes(NumStr);
                    return retHex;
            }
            return bytebuffer;
        }

        //public static String DecodeToInput(byte[] rawNode, XmlTypeCode TypeCode, Coder.OutputAsType outputType, System.Text.Encoding oEncoding)
        //{
        //    if (Coder.OutputAsType.Unchanged == outputType)
        //        return oEncoding.GetString(rawNode);

        //    // If the rawNode was encoded somehow (Base64, Hex, etc) then decodedbuffer contains the binary representation of this data

        //    // Create and encode the output
        //    Coder Decoder;
        //    switch (outputType)
        //    {
        //        case Coder.OutputAsType.Decoded:
        //            // Create the encoder
        //            Coder Encoder = Coder.AssignCoder((Coder.EncodingType)TypeCode);
        //            // Convert back to unicode
        //            return Encoder.Encode(rawNode);
        //        case Coder.OutputAsType.Base64:
        //            Decoder = Coder.AssignCoder(Coder.EncodingType.Base64);
        //            return oEncoding.GetString(Decoder.DecodeToBytes(oEncoding.GetString(rawNode)));
        //        case Coder.OutputAsType.Hex:
        //            Decoder = Coder.AssignCoder(Coder.EncodingType.Hex);
        //            return oEncoding.GetString(Decoder.DecodeToBytes(oEncoding.GetString(rawNode)));
        //        case Coder.OutputAsType.MSBase64:
        //            Decoder = Coder.AssignCoder(Coder.EncodingType.MSBase64);
        //            return oEncoding.GetString(Decoder.DecodeToBytes(oEncoding.GetString(rawNode)));
        //        case Coder.OutputAsType.BinaryLittleEndian:
        //            if (!BitConverter.IsLittleEndian)
        //                Array.Reverse(rawNode);
        //            String Num1 = ConvertByteArrayToNumber(rawNode, TypeCode, oEncoding);
        //            return Num1;
        //        case Coder.OutputAsType.BinaryBigEndian:
        //            if (BitConverter.IsLittleEndian)
        //                Array.Reverse(rawNode);
        //            String Num2 = ConvertByteArrayToNumber(rawNode, TypeCode, oEncoding);
        //            return Num2;
        //        default:
        //            return oEncoding.GetString(rawNode);
        //    }
        //}

        private static String ConvertByteArrayToNumber(byte[] bytebuffer, XmlTypeCode TypeCode, System.Text.Encoding oEncoding)
        {
            // BitConverter returns all the bytes for the type, e.g. 4 bytes for UInt32, even when the number is smaller than 4 bytes 
            // e.g. 1 would return 00000001 (or 01000000 as the case may be).
            switch (TypeCode)
            {
                case XmlTypeCode.UnsignedLong:
                    if (bytebuffer.Length < 8)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to UInt64", Log.LogType.Error);
                    return BitConverter.ToUInt64(bytebuffer, 0).ToString();
                case XmlTypeCode.Long:
                    if (bytebuffer.Length < 8)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to Int64", Log.LogType.Error);
                    return BitConverter.ToInt64(bytebuffer, 0).ToString();
                case XmlTypeCode.UnsignedInt:
                    if (bytebuffer.Length < 4)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to UInt32", Log.LogType.Error);
                    return BitConverter.ToUInt32(bytebuffer, 0).ToString();
                case XmlTypeCode.Int:
                    if (bytebuffer.Length < 4)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to Int32", Log.LogType.Error);
                    return BitConverter.ToInt32(bytebuffer, 0).ToString();
                case XmlTypeCode.UnsignedShort:
                    if (bytebuffer.Length < 2)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to UInt16", Log.LogType.Error);
                    return BitConverter.ToUInt16(bytebuffer, 0).ToString();
                case XmlTypeCode.Short:
                    if (bytebuffer.Length < 2)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to Int16", Log.LogType.Error);
                    return BitConverter.ToInt16(bytebuffer, 0).ToString();
                case XmlTypeCode.UnsignedByte:
                    if (bytebuffer.Length < 1)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to UnsignedByte", Log.LogType.Error);
                    return ((Byte)bytebuffer[0]).ToString();
                case XmlTypeCode.Byte:
                    if (bytebuffer.Length < 1)
                        Log.Write(MethodBase.GetCurrentMethod(), "Array to short to convert to UnsignedByte", Log.LogType.Error);
                    return ((SByte)bytebuffer[0]).ToString();
                case XmlTypeCode.HexBinary:
                    Coder Encoder = Coder.AssignCoder(Coder.EncodingType.Hex);
                    return Encoder.Encode(bytebuffer);
            }

            // Convert back to a string
            return oEncoding.GetString(bytebuffer);
        }

        /// <summary>
        /// Used to convert bytes from an input file into a string.  The bytes are expected to be in the outputAs format, but
        /// only string types e.g. UTF8 UTF16LE etc. are allowed.
        /// </summary>
        /// <param name="rawBytes">The array of bytes to convert</param>
        /// <param name="outputAs">The string type the bytes are encoded in</param>
        /// <param name="OutputEncoding">The default output encoding, which should be the encoding of the input</param>
        /// <returns>Null if outputAs is not a string type, otherwise the string</returns>
        public static String ConvertBytesToString(byte[] rawBytes, Coder.OutputAsType outputAs, System.Text.Encoding OutputEncoding)
        {
            System.Text.Encoding StrEncoding = OutputEncoding;
            byte[] prefixBytes = new byte[0];
            switch (outputAs)
            {
                case Coder.OutputAsType.Unchanged:
                    // Use OutputEncoding, this is the default
                    break;
                case Coder.OutputAsType.ASCIIString:
                    StrEncoding = ASCIIEncoding.ASCII;
                    break;
                case Coder.OutputAsType.UTF16BEString:
                    StrEncoding = UnicodeEncoding.BigEndianUnicode;
                    break;
                case Coder.OutputAsType.UTF16LEString:
                    StrEncoding = UnicodeEncoding.Unicode;
                    break;
                case Coder.OutputAsType.UTF7String:
                    StrEncoding = UTF7Encoding.UTF7;
                    prefixBytes = UTF7Encoding.UTF7.GetPreamble();
                    break;
                case Coder.OutputAsType.UTF8String:
                    StrEncoding = UTF8Encoding.UTF8;
                    break;
                case Coder.OutputAsType.UTF8StringWithBOM:
                    StrEncoding = UTF8Encoding.UTF8;
                    prefixBytes = UTF8Encoding.UTF8.GetPreamble();
                    break;
                case Coder.OutputAsType.UTF32LEString:
                    StrEncoding = UTF32Encoding.UTF32;
                    break;
                case Coder.OutputAsType.UTF32BEString:
                    StrEncoding = UTF32Encoding.UTF32;
                    // Need to reverse the order of the bytes
                    for (int i = 0; i < rawBytes.Length; i += 4)
                        Array.Reverse(rawBytes, i, 4);
                    break;
                default:
                    // Return null
                    return null;
            }

            int startIndex = 0;

            // Account for the prefix
            if (prefixBytes.Length > 0)
            {
                int i = 0;
                for (; i < prefixBytes.Length; i++)
                {
                    if (prefixBytes[i] != rawBytes[i])
                        break;
                }
                if (i == prefixBytes.Length)
                    startIndex = prefixBytes.Length;
            }

            // Convert the string
            return StrEncoding.GetString(rawBytes, startIndex, rawBytes.Length - startIndex);
        }
    }
}
