using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.Interop
{
    public class TypeDescHelper
    {
        // typedef struct FARSTRUCT tagTYPEDESC {
        //     union {
        //         /* VT_PTR - the pointed-at type */
        //         struct FARSTRUCT tagTYPEDESC FAR* lptdesc;
        //         /* VT_CARRAY */
        //         struct FARSTRUCT tagARRAYDESC FAR* lpadesc;
        //         /* VT_USERDEFINED - this is used to get a TypeInfo for the UDT*/
        //         HREFTYPE hreftype;
        //     }UNION_NAME(u);
        //     VARTYPE vt;
        // } TYPEDESC;
        // If the variable is VT_SAFEARRAY or VT_PTR, the union portion of the TYPEDESC contains a pointer to a TYPEDESC that 
        // specifies the element type.

        // typedef struct tagARRAYDESC {
        //     TYPEDESC tdescElem;                          /* element type */
        //     USHORT cDims;                                /* dimension count */
        //     [size_is(cDims)] SAFEARRAYBOUND rgbounds[];  /* var len array of bounds */
        // } ARRAYDESC;


        /// <summary>
        /// Converts a TYPEDESC into a XML simple type (pre-defined in http://www.w3.org/2001/XMLSchema).  If the type is not a
        /// simple type then null is returned;
        /// </summary>
        public static XmlQualifiedName GetSimpleTypeFromTYPEDESC(ComTypes.TYPEDESC tdesc)
        {
            // Convert VT_* into Xml Schema type
            switch ((VarEnum)tdesc.vt)
            {
                case VarEnum.VT_I2:         // Indicates a short integer. 
                    return new XmlQualifiedName("short", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_I4:         // Indicates a long integer. 
                    return new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_R4:         // Indicates a float value.
                    return new XmlQualifiedName("float", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_R8:         // Indicates a double value. 
                    return new XmlQualifiedName("double", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_BOOL:       // Indicates a Boolean value. 
                    return new XmlQualifiedName("boolean", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_DECIMAL:    // Indicates a decimal value. 
                    return new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_I1:         // Indicates a char value. 
                    return new XmlQualifiedName("byte", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_UI1:        // Indicates a byte. 
                    return new XmlQualifiedName("unsignedByte", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_UI2:        // Indicates an unsignedshort. 
                    return new XmlQualifiedName("unsignedShort", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_UI4:        // Indicates an unsignedlong. 
                    return new XmlQualifiedName("unsignedInt", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_I8:         // Indicates a 64-bit integer. 
                    return new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_UI8:        // Indicates an 64-bit unsigned integer. 
                    return new XmlQualifiedName("unsignedLong", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_INT:        // Indicates an integer value. 
                    return new XmlQualifiedName("integer", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_UINT:       // Indicates an unsigned integer value. 
                    return new XmlQualifiedName("nonNegativeInteger", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_BSTR:       // Indicates a BSTR string. 
                case VarEnum.VT_LPSTR:      // Indicates a null-terminated string. 
                case VarEnum.VT_LPWSTR:     // Indicates a wide string terminated by a null reference (Nothing in Visual Basic). 
                    return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");

                default:
                    return null;
            }

        }

        public static XmlQualifiedName CreateSchemaTypeFromTYPEDESC(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper, ComTypes.ITypeInfo oTypeInfo, ComTypes.TYPEDESC tdesc)
        {
            XmlQualifiedName QName;

            // Check to see if it's a Simple Type
            QName = GetSimpleTypeFromTYPEDESC(tdesc);
            if (null != QName)
                return QName;

            // It's complicated.

            // Convert VT_* into Xml Schema type
            switch ((VarEnum)tdesc.vt)
            {
                case VarEnum.VT_EMPTY:      //Indicates that a value was not specified.
                case VarEnum.VT_NULL:       // Indicates a null value, similar to a null value in SQL. 
                case VarEnum.VT_VOID:       // Indicates a C style void. 
                    return new XmlQualifiedName();
                case VarEnum.VT_HRESULT:    // Indicates an HRESULT. 
                    return new XmlQualifiedName("unsignedInt", "http://www.w3.org/2001/XMLSchema");
                case VarEnum.VT_PTR:        // Indicates a pointer type. 
                    // Get the TYPEDESC of the pointer
                    ComTypes.TYPEDESC oPtrTypeDesc = (ComTypes.TYPEDESC)Marshal.PtrToStructure(tdesc.lpValue, typeof(ComTypes.TYPEDESC));
                    return CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, oPtrTypeDesc);
                case VarEnum.VT_SAFEARRAY:  // Indicates a SAFEARRAY. Not valid in a VARIANT. 
                    // Get the TYPEDESC of the elements of the array
                    ComTypes.TYPEDESC oArrayEleTypeDescs = (ComTypes.TYPEDESC)Marshal.PtrToStructure(tdesc.lpValue, typeof(ComTypes.TYPEDESC));
                    XmlQualifiedName ArrayTypeQName = CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, oArrayEleTypeDescs);
                    XmlQualifiedName ArrayOfElementsQName = new XmlQualifiedName("ArrayOf" + ArrayTypeQName.Name, oXmlSchema.TargetNamespace);
                    // See if this array type already exists
                    if (!SchemaTypeExists(oXmlSchema, ArrayOfElementsQName))
                    {
                        // Need to add a new type that is a sequence of ArrayTypeQName
                        XmlSchemaComplexType oComplexType = new XmlSchemaComplexType();
                        oComplexType.Name = ArrayOfElementsQName.Name;
                        XmlSchemaSequence oSeq = new XmlSchemaSequence();
                        oComplexType.Particle = oSeq;
                        XmlSchemaElement oElement = new XmlSchemaElement();
                        oElement.Name = "ArrayElement";
                        oElement.SchemaTypeName = ArrayTypeQName;
                        oElement.MinOccurs = 0;
                        oElement.MaxOccursString = "unbounded";
                        oSeq.Items.Add(oElement);
                        oXmlSchema.Items.Add(oComplexType);
                    }
                    return ArrayOfElementsQName;

                case VarEnum.VT_DISPATCH:   // Indicates an IDispatch pointer. 
                case VarEnum.VT_USERDEFINED:// Indicates a user defined type.
                    int hRef = (int)tdesc.lpValue;  /* This is used to get a TypeInfo for the UDT*/
                    ComTypes.ITypeInfo oUDTTypeInfo;
                    oTypeInfo.GetRefTypeInfo(hRef, out oUDTTypeInfo);

                    TypeInfoHelper oTypeInfoHelper = oTypeLibHelper.AddInterfaceTypeInfo(oUDTTypeInfo);
                    return oTypeInfoHelper.AddToSchema(oXmlSchema, oTypeLibHelper);
                
                case VarEnum.VT_VARIANT:    // Indicates a VARIANT far pointer.
                    // Create a special variant type
                    XmlQualifiedName VariantQName = new XmlQualifiedName("variant", oXmlSchema.TargetNamespace);
                    if (!SchemaTypeExists(oXmlSchema, VariantQName))
                    {
                        // Add the variant type
                        XmlSchemaComplexType oVariantCT = new XmlSchemaComplexType();
                        oVariantCT.Name = VariantQName.Name;
                        XmlSchemaComplexContent oContentAnyType = new XmlSchemaComplexContent();
                        oVariantCT.ContentModel = oContentAnyType;
                        XmlSchemaComplexContentExtension oCCR = new XmlSchemaComplexContentExtension();
                        oCCR.BaseTypeName = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
                        oContentAnyType.Content = oCCR;
                        oXmlSchema.Items.Add(oVariantCT);
                    }
                    return VariantQName;

                case VarEnum.VT_CY:         // Indicates a currency value. 
                case VarEnum.VT_DATE:       // Indicates a DATE value. 
                case VarEnum.VT_ERROR:      // Indicates an SCODE.  
                case VarEnum.VT_UNKNOWN:    // Indicates an IUnknown pointer. 
                case VarEnum.VT_CARRAY:     // Indicates a C style array.
                
                case VarEnum.VT_RECORD:     // Indicates a user defined type. 
                case VarEnum.VT_FILETIME:   // Indicates a FILETIME value. 
                case VarEnum.VT_BLOB:       // Indicates length prefixed bytes. 
                case VarEnum.VT_STREAM:     // Indicates that the name of a stream follows. 
                case VarEnum.VT_STORAGE:    // Indicates that the name of a storage follows. 
                case VarEnum.VT_STREAMED_OBJECT: // Indicates that a stream contains an object. 
                case VarEnum.VT_STORED_OBJECT: // Indicates that a storage contains an object. 
                case VarEnum.VT_BLOB_OBJECT: // Indicates that a blob contains an object. 
                case VarEnum.VT_CF:         // Indicates the clipboard format. 
                case VarEnum.VT_CLSID:      // Indicates a class ID. 
                case VarEnum.VT_VECTOR:     // Indicates a simple, counted array. 
                case VarEnum.VT_ARRAY:      // Indicates a SAFEARRAY pointer. 
                case VarEnum.VT_BYREF:      // Indicates that a value is a reference. 
                default:
                    if (IntPtr.Zero != tdesc.lpValue)
                        Log.Write(MethodBase.GetCurrentMethod(), "Type '" + Enum.GetName(typeof(VarEnum), (VarEnum)tdesc.vt) + "' not supported but it should be", Log.LogType.Warning);
                    // TODO: support these types
                    return new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
            }

        }

        /// <summary>
        /// Returns true if the schema object already exists, otherwise false
        /// </summary>
        private static bool SchemaTypeExists(XmlSchema oXmlSchema, XmlQualifiedName oName)
        {
            // Check to see if we have already added this type
            foreach (XmlSchemaObject oSchemaObject in oXmlSchema.Items)
            {
                if (oSchemaObject is XmlSchemaType)
                {
                    XmlSchemaType oExistingType = oSchemaObject as XmlSchemaType;
                    if (oExistingType.Name.Equals(oName.Name, StringComparison.CurrentCulture))
                        return true;
                }
            }
            return false;
        }
    }
}
