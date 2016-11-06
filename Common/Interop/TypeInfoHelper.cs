using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Fuzzware.Common;

namespace Fuzzware.Common.Interop
{
    /// <summary>
    /// Helps retrieve type information from a System.Runtime.InteropServices.ComTypes.ITypeInfo object
    /// </summary>
    public class TypeInfoHelper
    {
        ComTypes.ITypeInfo oTypeInfo;

        IntPtr pTypeAttr;
        ComTypes.TYPEATTR stTypeAttr;
        List<FuncDescHelper> oFuncDescs = new List<FuncDescHelper>();
        List<VarDescHelper> oVarDescs = new List<VarDescHelper>();

        String strName;
        String strDocString;
        int dwHelpContext;
        String strHelpFile;
        Guid TypeGuid;

        public TypeInfoHelper(ComTypes.ITypeInfo ITypeInfo)
        {
            oTypeInfo = ITypeInfo;

            // Get the Type Attributes
            oTypeInfo.GetTypeAttr(out pTypeAttr);
            stTypeAttr = (ComTypes.TYPEATTR)Marshal.PtrToStructure(pTypeAttr, typeof(ComTypes.TYPEATTR));

            oTypeInfo.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            TypeGuid = stTypeAttr.guid;

            // Get the Function Descriptions
            for (int i = 0; i < stTypeAttr.cFuncs; i++)
            {
                FuncDescHelper oFuncDesc = new FuncDescHelper(oTypeInfo, i);
                if(!IsIUnknownOrIDispatchFunction(oFuncDesc))
                    oFuncDescs.Add(oFuncDesc);
            }

            // Get the Variable Descriptions
            for (int i = 0; i < stTypeAttr.cVars; i++)
            {
                VarDescHelper oVarDesc = new VarDescHelper(oTypeInfo, i);
                oVarDescs.Add(oVarDesc);
            }
        }

        ~TypeInfoHelper()
        {
            if(pTypeAttr != IntPtr.Zero)
                oTypeInfo.ReleaseTypeAttr(pTypeAttr);
        }

        /// <summary>
        /// Returns true if this function matches any of the following functions
        /// IUnknown
        ///  QueryInterface, AddRef, Release
        /// IDispatch
        ///  GetTypeInfoCount, GetTypeInfo, GetIDsOfNames, Invoke
        /// </summary>
        private bool IsIUnknownOrIDispatchFunction(FuncDescHelper oFuncDesc)
        {
            if (oFuncDesc.Name.Equals("QueryInterface", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("AddRef", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("Release", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("GetTypeInfoCount", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("GetTypeInfo", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("GetIDsOfNames", StringComparison.CurrentCultureIgnoreCase) ||
                oFuncDesc.Name.Equals("Invoke", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the list of functions for this type
        /// </summary>
        public List<FuncDescHelper> Functions
        {
            get
            {
                return oFuncDescs;
            }
        }

        /// <summary>
        /// Gets the name of the Type
        /// </summary>
        public string Name
        {
            get
            {
                return strName;
            }
        }

        /// <summary>
        /// Gets the help documentation for the Type
        /// </summary>
        public string HelpDocumentation
        {
            get
            {
                return strDocString;
            }
        }

        /// <summary>
        /// Gets the GUID of this type
        /// </summary>
        public Guid Guid
        {
            get
            {
                return TypeGuid;
            }
        }

        /// <summary>
        /// Is this type an interface (as opposed to a union, record or enum)
        /// </summary>
        public bool IsInterface
        {
            get
            {
                if ((stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_INTERFACE) ||
                    (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_DISPATCH))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds XML Schema types describing this ITypeInfo to an existing XML Schema 
        /// </summary>
        /// <param name="oXmlSchema"></param>
        public XmlQualifiedName AddToSchema(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper)
        {
            if ((stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_INTERFACE) ||
                (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_DISPATCH))
            {
                return AddInterfaceToSchema(oXmlSchema);
            }
            else if ((stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_UNION))
            {
                return AddUnionToSchema(oXmlSchema, oTypeLibHelper);
            }
            else if (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_RECORD)
            {
                return AddRecordToSchema(oXmlSchema, oTypeLibHelper);
            }
            else if (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_ENUM)
            {
                return AddEnumToSchema(oXmlSchema);
            }
            else if (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_ALIAS)
            {
                return AddAliasToSchema(oXmlSchema, oTypeLibHelper);
            }
            else if (stTypeAttr.typekind == ComTypes.TYPEKIND.TKIND_COCLASS)
            {
                // This will add a handle for this type, but it doesn't appear as if TKIND_COCLASS have methods.
                return AddInterfaceToSchema(oXmlSchema);
            }
            
            Log.Write(MethodBase.GetCurrentMethod(), "A TYPEKIND of '" + Enum.GetName(typeof(ComTypes.TYPEKIND), stTypeAttr.typekind) + "' is not supported", Log.LogType.Error);
            return null;
        }

        /// <summary>
        /// Adds functions of an interface to the schema
        /// </summary>
        private XmlQualifiedName AddInterfaceToSchema(XmlSchema oXmlSchema)
        {
            // Check to see if we have already added this type
            if (SchemaTypeExists(oXmlSchema))
                return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);

            // Add complex type with single 'handle' value
            XmlSchemaElement oHandleEle = new XmlSchemaElement();
            oHandleEle.Name = "handle";
            // Make it a long to handle 64 bit machines
            oHandleEle.SchemaTypeName = new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema");

            XmlSchemaSequence oSeq = new XmlSchemaSequence();
            oSeq.Items.Add(oHandleEle);

            XmlSchemaComplexType oInterfaceType = new XmlSchemaComplexType();
            oInterfaceType.Name = strName;
            oInterfaceType.Particle = oSeq;

            AddAnnotation(oInterfaceType);
            oXmlSchema.Items.Add(oInterfaceType);

            return new XmlQualifiedName(oInterfaceType.Name, oXmlSchema.TargetNamespace); ;
        }

        /// <summary>
        /// Adds an Enumeration type to the XML Schema
        /// </summary>
        private XmlQualifiedName AddEnumToSchema(XmlSchema oXmlSchema)
        {
            // Check to see if we have already added this type
            if(SchemaTypeExists(oXmlSchema))
                return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);

            // Create a new Enumeration type
            XmlSchemaSimpleType oSimpleType = new XmlSchemaSimpleType();
            oSimpleType.Name = strName;
            if (oVarDescs.Count > 0)
            {
                XmlSchemaSimpleTypeRestriction oEnumRestriction = new XmlSchemaSimpleTypeRestriction();
                oEnumRestriction.BaseTypeName = oVarDescs[0].SimpleTypeName;
                for (int i = 0; i < oVarDescs.Count; i++)
                {
                    XmlSchemaEnumerationFacet oFacet = new XmlSchemaEnumerationFacet();
                    oFacet.Value = oVarDescs[i].ConstValue.ToString();
                    if (!String.IsNullOrEmpty(oVarDescs[i].Name))
                        oFacet.Id = oVarDescs[i].Name;
                    oEnumRestriction.Facets.Add(oFacet);
                }
                oSimpleType.Content = oEnumRestriction;
            }
            AddAnnotation(oSimpleType);
            oXmlSchema.Items.Add(oSimpleType);
            return new XmlQualifiedName(oSimpleType.Name, oXmlSchema.TargetNamespace);
        }

        /// <summary>
        /// Adds a Union type to the XML Schema
        /// </summary>
        private XmlQualifiedName AddUnionToSchema(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper)
        {
            // Check to see if we have already added this type
            if (SchemaTypeExists(oXmlSchema))
                return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);

            // Add a choice of elements
            XmlSchemaComplexType oComplexType = new XmlSchemaComplexType();
            oComplexType.Name = strName;
            XmlSchemaChoice oChoice = new XmlSchemaChoice();
            oComplexType.Particle = oChoice;

            for (int i = 0; i < oVarDescs.Count; i++)
            {
                XmlSchemaElement oEle = new XmlSchemaElement();
                oEle.Name = oVarDescs[i].Name;
                oEle.SchemaTypeName = oVarDescs[i].SimpleTypeName;
                if (oEle.SchemaTypeName.IsEmpty)
                    oEle.SchemaTypeName = TypeDescHelper.CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, oVarDescs[i].TypeDesc);
                oChoice.Items.Add(oEle);
            }

            AddAnnotation(oComplexType);
            oXmlSchema.Items.Add(oComplexType);
            return new XmlQualifiedName(oComplexType.Name, oXmlSchema.TargetNamespace);
        }

        /// <summary>
        /// Adds a record (User-Defined Type(UDT) type to the XML Schema
        /// </summary>
        private XmlQualifiedName AddRecordToSchema(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper)
        {
            // Check to see if we have already added this type
            if (SchemaTypeExists(oXmlSchema))
                return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);

            // Add a sequence of elements
            XmlSchemaComplexType oComplexType = new XmlSchemaComplexType();
            oComplexType.Name = strName;
            XmlSchemaSequence oSeq = new XmlSchemaSequence();
            oComplexType.Particle = oSeq;

            for (int i = 0; i < oVarDescs.Count; i++)
            {
                XmlSchemaElement oEle = new XmlSchemaElement();
                oEle.Name = oVarDescs[i].Name;
                oEle.SchemaTypeName = oVarDescs[i].SimpleTypeName;
                if (oEle.SchemaTypeName.IsEmpty)
                    oEle.SchemaTypeName = TypeDescHelper.CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, oVarDescs[i].TypeDesc);
                oSeq.Items.Add(oEle);
            }

            AddAnnotation(oComplexType);
            oXmlSchema.Items.Add(oComplexType);
            return new XmlQualifiedName(oComplexType.Name, oXmlSchema.TargetNamespace);
        }

        /// <summary>
        /// Adds a Alias type to the XML Schema
        /// </summary>
        private XmlQualifiedName AddAliasToSchema(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper)
        {
            // Check to see if we have already added this type
            if (SchemaTypeExists(oXmlSchema))
                return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);

            XmlQualifiedName OrigQName = TypeDescHelper.CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, stTypeAttr.tdescAlias);

            // If the alias is a pre-defined simple type, then use it directly
            if (OrigQName.Namespace.Equals("http://www.w3.org/2001/XMLSchema", StringComparison.CurrentCultureIgnoreCase))
                return OrigQName;

            // See if this is an ALIAS for an interface, in which case we need to change the name
            for (int i = oTypeLibHelper.InterfaceTypeInfoHelpers.Count - 1; i >= 0; i--)
            {
                if (OrigQName.Name.Equals(oTypeLibHelper.InterfaceTypeInfoHelpers[i].Name))
                {
                    oTypeLibHelper.InterfaceTypeInfoHelpers[i].strName = strName;
                    break;
                }
            }
            // Find the type that was just added to the XmlSchema and change it's name in the Schema
            for (int i = oXmlSchema.Items.Count - 1; i >= 0; i--)
            {
                if (oXmlSchema.Items[i] is XmlSchemaType)
                {
                    XmlSchemaType oType = oXmlSchema.Items[i] as XmlSchemaType;
                    if (oType.Name.Equals(OrigQName.Name))
                    {
                        oType.Name = strName;
                        break;
                    }
                }
            }
            return new XmlQualifiedName(strName, oXmlSchema.TargetNamespace);
        }

        /// <summary>
        /// Returns true if the schema object already exists, otherwise false
        /// </summary>
        private bool SchemaTypeExists(XmlSchema oXmlSchema)
        {
            // Check to see if we have already added this type
            foreach (XmlSchemaObject oSchemaObject in oXmlSchema.Items)
            {
                if (oSchemaObject is XmlSchemaType)
                {
                    XmlSchemaType oExistingType = oSchemaObject as XmlSchemaType;
                    if (oExistingType.Name.Equals(strName, StringComparison.CurrentCulture))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the documentation string of a type as annotation to its XML Schema object
        /// </summary>
        private void AddAnnotation(XmlSchemaAnnotated oSchemaAnnotated)
        {
            if (!String.IsNullOrEmpty(strDocString))
            {
                XmlDocument oXmlDoc = new XmlDocument();
                XmlText oText = oXmlDoc.CreateTextNode(strDocString);
                XmlSchemaDocumentation oDocumentation = new XmlSchemaDocumentation();
                oDocumentation.Markup = new XmlNode[1];
                oDocumentation.Markup[0] = oText;
                XmlSchemaAnnotation oAnnotation = new XmlSchemaAnnotation();
                oAnnotation.Items.Add(oDocumentation);
                oSchemaAnnotated.Annotation = oAnnotation;
            }
        }
    }
}
