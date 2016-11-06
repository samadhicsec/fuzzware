using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Common.Encoding
{
    /// <summary>
    /// The default encoder/decoder, does nothing.
    /// 
    /// For all the encoding and decoding functions, if the input/output is a String, then we are just dealing with Unicode, if the 
    /// input/output is a byte[] then the input/output is in the global output format.
    /// </summary>
    public class Coder
    {
        public const String Encoding = "Encoding";
        public enum EncodingType
        {
            None = DataSchemaTypeCode.None,
            Base64 = DataSchemaTypeCode.Base64,
            Hex = DataSchemaTypeCode.Hex,
            MSBase64 = DataSchemaTypeCode.MSBase64,
        }

        public const String OutputAs = "outputAs";
        public enum OutputAsType
        {
            Unchanged,
            Decoded,
            Base64,
            Hex,
            MSBase64,
            BinaryLittleEndian,
            BinaryBigEndian,
            ASCIIString,
            UTF7String,
            UTF8String,
            UTF8StringWithBOM,
            UTF16LEString,
            UTF16BEString,
            UTF32LEString,
            UTF32BEString,
        }

        public virtual String Encode(byte[] input)
        {
            System.Text.Encoding Enc = new UnicodeEncoding();
            return Enc.GetString(input);
        }

        public virtual Byte[] DecodeToBytes(String input)
        {
            System.Text.Encoding Enc = new UnicodeEncoding();
            return Enc.GetBytes(input);
        }

        public static bool NeedsEncoding(DataSchemaTypeCode Encoding)
        {
            if (Encoding == DataSchemaTypeCode.Hex)
                return true;
            else if (Encoding == DataSchemaTypeCode.Base64)
                return true;
            return false;
        }

        public static String MakeEncoded(byte[] Unencoded, DataSchemaTypeCode Encoding)
        {
            Coder Enc = null;
            if (Encoding == DataSchemaTypeCode.Hex)
                Enc = new HexCoder();
            else if (Encoding == DataSchemaTypeCode.Base64)
                Enc = new Base64Coder();

            if (null != Enc)
            {
                return Enc.Encode(Unencoded);
            }
            return null;
        }

        public static Coder AssignCoder(DataSchemaTypeCode Encoding)
        {
            return Coder.AssignCoder((Coder.EncodingType)Encoding);
        }

        public static Coder AssignCoder(Coder.EncodingType Encoding)
        {
            if (Encoding == Coder.EncodingType.Base64)
                return new Base64Coder();
            else if (Encoding == Coder.EncodingType.Hex)
                return new HexCoder();
            else if (Encoding == Coder.EncodingType.MSBase64)
                return new MSBase64Coder();

            return new Coder();
        }
    }

    class Base64Coder : Coder
    {
        public override String Encode(byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        public override Byte[] DecodeToBytes(String EncodedInput)
        {
            try
            {
                return Convert.FromBase64String(EncodedInput);
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return null;
        }
    }

    public class HexCoder : Coder
    {
        public override String Encode(byte[] input)
        {
            StringBuilder output = new StringBuilder(2 * input.Length);
            for (int i = 0; i < input.Length; i++)
                output.Append(input[i].ToString("X2"));
            return output.ToString();
        }

        public override Byte[] DecodeToBytes(String EncodedInput)
        {
            Byte[] output = null;
            try
            {
                output = new Byte[(EncodedInput.Length + 1) / 2];

                int i = 0;
                int outindex = 0;
                // If the hex number is an odd number of digits, the first digit is a single hex character
                if (EncodedInput.Length % 2 == 1)
                {
                    output[outindex++] = Byte.Parse(EncodedInput.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    i++;
                }
                for(; i < EncodedInput.Length; i +=2)
                {
                    output[outindex++] = Byte.Parse(EncodedInput.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
                                
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return output;
        }
    }

    class MSBase64Coder : Base64Coder
    {
        public override String Encode(byte[] input)
        {
            String output = base.Encode(input);
            output = output.Replace('+', '!');
            output = output.Replace('\\', '*');
            return output;
        }

        public override Byte[] DecodeToBytes(String input)
        {
            input = input.Replace('!', '+');
            input = input.Replace('*', '\\');
            return base.DecodeToBytes(input);
        }
    }
}
