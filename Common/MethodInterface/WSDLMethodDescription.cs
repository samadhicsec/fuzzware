using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common.XML;

namespace Fuzzware.Common.MethodInterface
{
    /*
     * Notes on WSDLs
     * The wsdl:binding/soap:binding element 
     *   - Has a STYLE attribute, this is the DEFAULT value of style for all soap:body for that binding.
     *      - A value of 'document' means the message parts appear directly under the SOAP Body element
     *      - A value of 'rpc' means each part is a parameter or a return value and appears inside a wrapper element within the 
     *      body (following Section 7.1 of the SOAP specification). The wrapper element is named identically to the operation 
     *      name and its namespace is the value of the namespace attribute. Each message part (parameter) appears under the 
     *      wrapper, represented by an accessor named identically to the corresponding parameter of the call. Parts are arranged 
     *      in the same order as the parameters of the call.
     *   - Has a TRANSPORT attribute, which is http://schemas.xmlsoap.org/soap/http for HTTP, different for other transports (SMTP, FTP)
     * 
     * The wsdl:binding/wsdl:operation/wsdl:input/soap:body element
     *   - Has a USE attribute that indicates whether the message parts are encoded using some encoding rules, or whether the 
     *   parts define the concrete schema of the message.
     *      - If use is ENCODED, then each message part references an abstract type using the type attribute. These abstract types 
     *      are used to produce a concrete message by applying an encoding specified by the encodingStyle attribute. The part names, 
     *      types and value of the namespace attribute are all inputs to the encoding, although the namespace attribute only applies 
     *      to content not explicitly defined by the abstract types. If the referenced encoding style allows variations in it’s 
     *      format (such as the SOAP encoding does), then all variations MUST be supported ("reader makes right").
     *      - If use is LITERAL, then each part references a concrete schema definition using either the element or type attribute. 
     *      In the first case, the element referenced by the part will appear directly under the Body element (for document style 
     *      bindings) or under an accessor element named after the message part (in rpc style). In the second, the type referenced 
     *      by the part becomes the schema type of the enclosing element (Body for document style or part accessor element for rpc style). 
     *      The value of the encodingStyle attribute MAY be used when the use is literal to indicate that the concrete format was 
     *      derived using a particular encoding (such as the SOAP encoding), but that only the specified variation is supported 
     *      ("writer makes right").
     *   - Has an ENCODINGSTYLE attribute that is a list of URIs, each separated by a single space. The URI's represent encodings used 
     *   within the message, in order from most restrictive to least restrictive (exactly like the encodingStyle attribute defined in 
     *   the SOAP specification).
     *   
     * The wsdl:operation/soap:operation element
     *   - Has a SOAPACTION attribute that specifes the soapAction that is used in the SOAP message.
     */

    /*
     * How style and use affect the layout of the soap:body
     * If style='document' and use='literal'
     *      How it appears in the wsdl:message element is how it appears in the SOAP body.  
     *      If bUsesWrapperElement=true (see DetermineElementWrapperUse)
     *          Then there is only 1 wsdl:message part and it is a wrapper element named identical to the Operation name.
     * If style='document' and use='encoded'
     *      This is not supported, no one uses it.
     * If style='rpc' and use='literal' or use='encoded'
     *      The parts appear in a wrapper element named identical to the Operation name.
     */

    /// <summary>
    /// Converts a WSDL OperationBinding into a MethodDescription object.
    /// </summary>
    public class WSDLMethodDescription : MethodDescription
    {
        SoapBindingStyle eBindingStyle = SoapBindingStyle.Document;
        SoapBindingStyle eDefaultBindingStyle = SoapBindingStyle.Document;
        String soapAction;
        bool soapActionReq;
        SoapBindingUse eInputBindingUse = SoapBindingUse.Literal;
        String encodingStyleInput;
        SoapBindingUse eOutputBindingUse = SoapBindingUse.Literal;
        String encodingStyleOutput;
        String outputMessageNamespace;
        //SoapBindingUse eFaultBindingUse = SoapBindingUse.Literal;
        //String encodingStyleFault;
        String RequestMessageNodeName;
        String ResponseMessageNodeName;

        List<ParameterDesc> oOutputParameterDescs;

        public String SoapAction
        {
            get{ return soapAction; }
        }
        public SoapBindingStyle BindingStyle
        {
            get { return eBindingStyle; }
        }
        public SoapBindingUse InputBindingUse
        {
            get { return eInputBindingUse; }
        }
        public String RequestNodeName
        {
            get { return RequestMessageNodeName; }
        }
        public String ResponseNodeName
        {
            get { return ResponseMessageNodeName; }
        }

        /// <summary>
        /// Get the style and transport attributes of the parent Binding
        /// </summary>
        private void GetSoapBindingAttributes(Binding oBinding)
        {
            // Get the SOAP binding style and transport from the parent Binding.  The style specifies whether or not wrapper elements are used
            // in the SOAP Body.  The transport indicates if HTTP is used.
            for (int i = 0; i < oBinding.Extensions.Count; i++)
            {
                if (oBinding.Extensions[i].GetType() == typeof(Soap12Binding))
                {
                    Soap12Binding oSoap12Binding = (Soap12Binding)oBinding.Extensions[i];
                    eDefaultBindingStyle = oSoap12Binding.Style;
                    if (!oSoap12Binding.Transport.Equals(@"http://schemas.xmlsoap.org/soap/http", StringComparison.CurrentCultureIgnoreCase)
                        && !oSoap12Binding.Transport.Equals(COMLibraryDescription.COMTransport, StringComparison.CurrentCultureIgnoreCase))
                        Log.Write(MethodBase.GetCurrentMethod(), "Schemer does not support the '" + oSoap12Binding.Transport + "' transport for Soap 1.2", Log.LogType.Error);
                }
                if (oBinding.Extensions[i].GetType() == typeof(SoapBinding))
                {
                    SoapBinding oSoapBinding = (SoapBinding)oBinding.Extensions[i];
                    eDefaultBindingStyle = oSoapBinding.Style;
                    if (!oSoapBinding.Transport.Equals(@"http://schemas.xmlsoap.org/soap/http", StringComparison.CurrentCultureIgnoreCase))
                        Log.Write(MethodBase.GetCurrentMethod(), "Schemer does not support the '" + oSoapBinding.Transport + "' transport for Soap 1.1", Log.LogType.Error);
                }
            }
        }

        /// <summary>
        /// Allows the over-riding of the default style attribute and specifies the soapAction
        /// </summary>
        private void GetSoapOperationAttributes(OperationBinding oOperationBinding)
        {
            for (int i = 0; i < oOperationBinding.Extensions.Count; i++)
            {
                if (oOperationBinding.Extensions[i].GetType() == typeof(Soap12OperationBinding))
                {
                    Soap12OperationBinding oSoap12OperationBinding = (Soap12OperationBinding)oOperationBinding.Extensions[i];
                    eBindingStyle = oSoap12OperationBinding.Style;
                    soapActionReq = oSoap12OperationBinding.SoapActionRequired;
                    soapAction = oSoap12OperationBinding.SoapAction;
                }
                if (oOperationBinding.Extensions[i].GetType() == typeof(SoapOperationBinding))
                {
                    SoapOperationBinding oSoapOperationBinding = (SoapOperationBinding)oOperationBinding.Extensions[i];
                    eBindingStyle = oSoapOperationBinding.Style;
                    // TODO: check what default value of this should be for SOAP 1.1
                    soapActionReq = false;
                    if (!String.IsNullOrEmpty(oSoapOperationBinding.SoapAction))
                    {
                        soapAction = oSoapOperationBinding.SoapAction;
                    }
                }
            }
        }

        /// <summary>
        /// Get the input, output and fault SOAP Body attributes
        /// </summary>
        private void GetBodyAttributes(OperationBinding oOperationBinding, String TargetNamespace)
        {
            if ((null != oOperationBinding.Input) && (oOperationBinding.Input.Extensions.Count > 0))
            {
                for (int i = 0; i < oOperationBinding.Input.Extensions.Count; i++)
                {
                    //Log.Write(MethodBase.GetCurrentMethod(), MethodName + " has an Operation Input element with more than 1 extension", Log.LogType.Error);
                    if (oOperationBinding.Input.Extensions[i].GetType() == typeof(Soap12BodyBinding))
                    {
                        Soap12BodyBinding oSoap12BodyBinding = (Soap12BodyBinding)oOperationBinding.Input.Extensions[i];
                        eInputBindingUse = oSoap12BodyBinding.Use;
                        encodingStyleInput = oSoap12BodyBinding.Encoding;
                        inputMessageNamespace = oSoap12BodyBinding.Namespace;
                    }
                    if (oOperationBinding.Input.Extensions[i].GetType() == typeof(SoapBodyBinding))
                    {
                        SoapBodyBinding oSoapBodyBinding = (SoapBodyBinding)oOperationBinding.Input.Extensions[i];
                        eInputBindingUse = oSoapBodyBinding.Use;
                        encodingStyleInput = oSoapBodyBinding.Encoding;
                        inputMessageNamespace = oSoapBodyBinding.Namespace;
                    }
                    //if (oOperationBinding.Input.Extensions[i].GetType() == typeof(SoapHeaderBinding))
                    //{
                    //    SoapHeaderBinding oSoapHeaderBinding = (SoapHeaderBinding)oOperationBinding.Input.Extensions[i];
                    //}
                    if (String.IsNullOrEmpty(inputMessageNamespace))
                        inputMessageNamespace = TargetNamespace;
                }
            }

            if ((null != oOperationBinding.Output) && (oOperationBinding.Output.Extensions.Count > 0))
            {
                for (int i = 0; i < oOperationBinding.Output.Extensions.Count; i++)
                {
                    if (oOperationBinding.Output.Extensions[i].GetType() == typeof(Soap12BodyBinding))
                    {
                        Soap12BodyBinding oSoap12BodyBinding = (Soap12BodyBinding)oOperationBinding.Output.Extensions[i];
                        eOutputBindingUse = oSoap12BodyBinding.Use;
                        encodingStyleOutput = oSoap12BodyBinding.Encoding;
                        outputMessageNamespace = oSoap12BodyBinding.Namespace;
                    }
                    if (oOperationBinding.Output.Extensions[i].GetType() == typeof(SoapBodyBinding))
                    {
                        SoapBodyBinding oSoapBodyBinding = (SoapBodyBinding)oOperationBinding.Output.Extensions[i];
                        eOutputBindingUse = oSoapBodyBinding.Use;
                        encodingStyleOutput = oSoapBodyBinding.Encoding;
                        outputMessageNamespace = oSoapBodyBinding.Namespace;
                    }
                    if (String.IsNullOrEmpty(outputMessageNamespace))
                        outputMessageNamespace = TargetNamespace;
                }
            }

            // TODO: record all the fault messages
            // TODO: record all the header messages
            // TODO: record all the header fault messages
        }

        /// <summary>
        /// Return the Operation, which matches the OperationBinding name and is listed under the portType for the 
        /// parent Binding.
        /// </summary>
        private Operation GetOperation(OperationBinding oOperationBinding, ServiceDescription oSvcDesc)
        {
            PortType oPortType = oSvcDesc.PortTypes[oOperationBinding.Binding.Type.Name];
            Operation oTargetOperation = null;
            foreach (Operation Op in oPortType.Operations)
            {
                if (Op.Name.Equals(oOperationBinding.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    oTargetOperation = Op;
                    // It is possible to have 2 operations of the same name, so check that the OperationInput and
                    // OperationOutput have names that match the InputBinding and OutputBinding respectively
                    OperationInput oOperationInput;
                    OperationOutput oOperationOutput;
                    GetInputAndOutputOperations(oTargetOperation, out oOperationInput, out oOperationOutput);
                    if(!String.IsNullOrEmpty(oOperationBinding.Input.Name) && (!String.IsNullOrEmpty(oOperationInput.Name)))
                        if(!oOperationBinding.Input.Name.Equals(oOperationInput.Name, StringComparison.CurrentCulture))
                        {
                            oTargetOperation = null;
                            continue;
                        }
                    if(!String.IsNullOrEmpty(oOperationBinding.Output.Name) && (!String.IsNullOrEmpty(oOperationOutput.Name)))
                        if(!oOperationBinding.Output.Name.Equals(oOperationOutput.Name, StringComparison.CurrentCulture))
                        {
                            oTargetOperation = null;
                            continue;
                        }
                    
                    break;
                }
            }
            if (null == oTargetOperation)
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find PortType Operation with name '" + oOperationBinding.Name + "'", Log.LogType.Error);
            return oTargetOperation;
        }

        /// <summary>
        /// Set the OperationInput and OperationOutput given an Operation
        /// </summary>
        private void GetInputAndOutputOperations(Operation oTargetOperation, out OperationInput oOperationInput, out OperationOutput oOperationOutput)
        {
            oOperationInput = null;
            oOperationOutput = null;
            for (int i = 0; i < oTargetOperation.Messages.Count; i++)
            {
                if (oTargetOperation.Messages[i] is OperationInput)
                    oOperationInput = (OperationInput)oTargetOperation.Messages[i];
                if (oTargetOperation.Messages[i] is OperationOutput)
                    oOperationOutput = (OperationOutput)oTargetOperation.Messages[i];
            }
            // Both input and output must exist
            if ((null == oOperationInput) || (null == oOperationOutput))
                Log.Write(MethodBase.GetCurrentMethod(), "The input or output message on PortType Operation '" + MethodName + "' was missing", Log.LogType.Error);
        }

        /// <summary>
        /// Converts a WSDL OperationBinding into a MethodDescription object
        /// </summary>
        public virtual void Create(OperationBinding oOperationBinding, ServiceDescription oSvcDesc, Dictionary<string, XmlSchema> oSchemaDictionary)
        {
            // Set the name, this may not be unique
            MethodName = oOperationBinding.Name;
            OriginalMethodName = oOperationBinding.Name;

            // Set the Invoke Kind
            InvokeKind = eInvokeKind.INVOKE_FUNC;
            if (MethodName.StartsWith(INVOKE_PROPERTYGET_PREFIX))
            {
                InvokeKind = eInvokeKind.INVOKE_PROPERTYGET;
                OriginalMethodName = OriginalMethodName.Substring(INVOKE_PROPERTYGET_PREFIX.Length);
            }
            if (MethodName.StartsWith(INVOKE_PROPERTYPUT_PREFIX))
            {
                InvokeKind = eInvokeKind.INVOKE_PROPERTYPUT;
                OriginalMethodName = OriginalMethodName.Substring(INVOKE_PROPERTYPUT_PREFIX.Length);
            }

            // Get the parent SOAP Binding attributes for this method.  These include the default style(document|rpc) and the transport.
            GetSoapBindingAttributes(oOperationBinding.Binding);

            // Get the Operation attributes.  This include the style(document|rpc) over-ride and soapAction
            GetSoapOperationAttributes(oOperationBinding);

            // Get the input and output Soap Body attributes.  These include the use(literal|encoded) and encodingStyle.
            GetBodyAttributes(oOperationBinding, oSvcDesc.TargetNamespace);

            // Find the target Operation from the operations listed under the PortType of the parent Binding
            Operation oTargetOperation = GetOperation(oOperationBinding, oSvcDesc);
            
            // Get the input and output messages of the Operation
            OperationInput oOperationInput;
            OperationOutput oOperationOutput;
            GetInputAndOutputOperations(oTargetOperation, out oOperationInput, out oOperationOutput);

            // Set the bUsesWrapperElement class variable
            DetermineElementWrapperUse(oOperationInput.Message.Name, oSvcDesc, oTargetOperation, oSchemaDictionary);

            // Set RequestMessageNodeName and ResponseMessageNodeName class variables
            DetermineSOAPMethodNodeNames(oOperationInput.Message.Name, oOperationOutput.Message.Name, oSvcDesc, oTargetOperation, oSchemaDictionary);

            // Create the list of input parameters from the input Message
            List<XmlSchemaElement> oInputParameterElements = CreateParametersFromMessage(oOperationInput.Message.Name, oSvcDesc, oTargetOperation, oSchemaDictionary);

            // Create the list of output parameters from the output Message
            List<XmlSchemaElement> oOutputParameterElements = CreateParametersFromMessage(oOperationOutput.Message.Name, oSvcDesc, oTargetOperation, oSchemaDictionary);

            // Create ParameterDescs List
            WriteParameters(oTargetOperation, oInputParameterElements, oOutputParameterElements);

            // Find the return value.  Only relevant if the method Binding is RPC.
            //ReturnDesc = WSDLMethodDescription.GetReturnParameter(eBindingStyle, InvokeKind, oTargetOperation, ParameterDescs);
        }

        /// <summary>
        /// Determines if this method uses a wrapper element around its parameters
        /// </summary>
        private void DetermineElementWrapperUse(String MessageName, ServiceDescription oServiceDesc, Operation oTargetOperation, Dictionary<string, XmlSchema> oSchemaDictionary)
        {
            Message oMessage = oServiceDesc.Messages[MessageName];
            // Need to determine if the parameters are wrapped.  This happends in 2 cases
            // - The STYLE is RPC
            // - The STYLE is Document and the USE is Literal but a wrapper is used.  This is not a standard, but is commonly used,
            //   so we must identify this situation, to do so we use the following rules
            //      1) The Message.Parts count = 1
            //      2) The Part is an element
            //      3) The element has the same name as the Operation
            //      4) The element's complex type has no attributes
            // (This is from http://www.ibm.com/developerworks/webservices/library/ws-whichwsdl/)
            bUsesWrapperElement = false;
            if ((eBindingStyle == SoapBindingStyle.Rpc) || ((eBindingStyle == SoapBindingStyle.Default) && (eDefaultBindingStyle == SoapBindingStyle.Rpc)))
                bUsesWrapperElement = true;
            else if (eInputBindingUse == SoapBindingUse.Literal)  // The STYLE is Document
            {
                if ((oMessage.Parts.Count == 1) &&                                                                  // Condition 1
                    ((null != oMessage.Parts[0].Element) && !(oMessage.Parts[0].Element.IsEmpty)) &&                // Condition 2
                    (oTargetOperation.Name.Equals(oMessage.Parts[0].Element.Name, StringComparison.CurrentCulture)) // Condition 3
                    )
                {
                    XmlSchemaElement oXmlSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oSchemaDictionary[oMessage.Parts[0].Element.Namespace], oMessage.Parts[0].Element.Name);
                    if((null != oXmlSchemaElement) && (oXmlSchemaElement.SchemaType is XmlSchemaComplexType) &&                           // Condition 4
                            ((oXmlSchemaElement.SchemaType as XmlSchemaComplexType).Attributes.Count == 0))
                        bUsesWrapperElement = true;
                }   
            }
        }

        /// <summary>
        /// Determine the Request and Response method node names
        /// </summary>
        private void DetermineSOAPMethodNodeNames(String RequestMessageName, String ResponseMessageName, ServiceDescription oServiceDesc, Operation oTargetOperation, Dictionary<string, XmlSchema> oSchemaDictionary)
        {
            // Determine the name of the Request method node
            Message oRequestMessage = oServiceDesc.Messages[RequestMessageName];

            // In almost all cases this is the Request method node name
            RequestMessageNodeName = oTargetOperation.Name;

            // ... except when ...
            if (!bUsesWrapperElement && (oRequestMessage.Parts.Count == 1) && (null != oRequestMessage.Parts[0].Element) &&
                (!oRequestMessage.Parts[0].Element.IsEmpty))
            {
                XmlSchemaElement oXmlSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oSchemaDictionary[oRequestMessage.Parts[0].Element.Namespace], oRequestMessage.Parts[0].Element.Name);
                if(null != oXmlSchemaElement)
                    RequestMessageNodeName = oXmlSchemaElement.Name;
            }

            // Determine the name of the Response method node
            Message oResponseMessage = oServiceDesc.Messages[ResponseMessageName];

            // In almost all cases this is the Response method node name
            ResponseMessageNodeName = oTargetOperation.Name + "Response";

            // ... except when ...
            if (!bUsesWrapperElement && (oResponseMessage.Parts.Count == 1) && (null != oResponseMessage.Parts[0].Element) &&
                (!oResponseMessage.Parts[0].Element.IsEmpty))
            {
                XmlSchemaElement oXmlSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oSchemaDictionary[oResponseMessage.Parts[0].Element.Namespace], oResponseMessage.Parts[0].Element.Name);
                if (null != oXmlSchemaElement)
                    ResponseMessageNodeName = oXmlSchemaElement.Name;
            }
        }

        /// <summary>
        /// Create a List&lt;XmlSchemaElement> for the parameters of a method.  The XmlSchemaElement list created
        /// should match the contents of the SOAP Body message in the web service method call
        /// </summary>
        private List<XmlSchemaElement> CreateParametersFromMessage(String MessageName, ServiceDescription oServiceDesc, Operation oTargetOperation, Dictionary<string, XmlSchema> oSchemaDictionary)
        {
            Message oMessage = oServiceDesc.Messages[MessageName];

            List<XmlSchemaElement> oParameters = new List<XmlSchemaElement>();
            // Message parts can look like this
            //  <wsdl:message name="messagename">
            //    <wsdl:part name="part1" type="xs:int"/>
            //    <wsdl:part name="part2" type="xs:string"/>
            //    <wsdl:part name="part3" type="ns:othertype"/>
            //  </wsdl:message>
            // or                                           
            //  <wsdl:message name="messagename">
            //    <wsdl:part name="part1" element="ns:someelement"/>
            //  </wsdl:message>
            //
            // If bUsesWrapperElement = true, then we need to look inside the wrapper element to get the params, but only
            // if the style='document'.
            //
            // EC: I am assuming that wsdl:parts in a wsdl:message cannot mix 'type' and 'element'
            //

            for (int i = 0; i < oMessage.Parts.Count; i++)
            {
                MessagePart oMessagePart = oMessage.Parts[i];

                if (0 == oServiceDesc.Types.Schemas.Count)
                {
                    // The message references a standard predefined types
                    if ((null != oMessagePart.Type))
                    {
                        // Create an element for this type
                        XmlSchemaElement oSimpleXmlSchemaElement = new XmlSchemaElement();
                        oSimpleXmlSchemaElement.Name = oMessagePart.Name;
                        oSimpleXmlSchemaElement.SchemaTypeName = oMessagePart.Type;
                        oParameters.Add(oSimpleXmlSchemaElement);
                    }
                    else if ((null != oMessagePart.Element) && !(oMessagePart.Element.IsEmpty))
                        Log.Write(MethodBase.GetCurrentMethod(), "Cannot have a message part that references an element if there are no schemas in Types", Log.LogType.Error);
                }

                XmlSchema oSchema = null;
                if(!String.IsNullOrEmpty(oMessagePart.Element.Namespace) && oSchemaDictionary.ContainsKey(oMessagePart.Element.Namespace))
                    oSchema = oSchemaDictionary[oMessagePart.Element.Namespace];
                // Message references an element
                if ((null != oMessagePart.Element) && !(oMessagePart.Element.IsEmpty) &&
                    (null != oSchema) && (null != XMLHelper.GetElementFromUncompiledSchema(oSchema, oMessagePart.Element.Name)))
                {
                    XmlSchemaElement oXmlSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oSchema, oMessagePart.Element.Name);

                    if (!bUsesWrapperElement)
                        oParameters.Add(oXmlSchemaElement);
                    else if((eBindingStyle == SoapBindingStyle.Document) || ((eBindingStyle == SoapBindingStyle.Default) && (eDefaultBindingStyle == SoapBindingStyle.Document)))
                    {
                        // Add a parameter for each child element
                        if(!(oXmlSchemaElement.SchemaType is XmlSchemaComplexType))
                            Log.Write(MethodBase.GetCurrentMethod(), "The WSDL message '" + MessageName + "' uses a wrapper element, but the element is not a complex type", Log.LogType.Error);

                        XmlSchemaComplexType oCT = oXmlSchemaElement.SchemaType as XmlSchemaComplexType;
                        if (null != oCT.Particle)
                        {
                            if (!(oCT.Particle is XmlSchemaGroupBase))
                                Log.Write(MethodBase.GetCurrentMethod(), "The WSDL message '" + MessageName + "' uses a wrapper element, but the element particle is not a Sequence, Choice or All", Log.LogType.Error);
                            XmlSchemaGroupBase oGroup = oCT.Particle as XmlSchemaGroupBase;
                            for (int j = 0; j < oGroup.Items.Count; j++)
                            {
                                if (oGroup.Items[j] is XmlSchemaElement)
                                    oParameters.Add(oGroup.Items[j] as XmlSchemaElement);
                            }
                        }
                    }
                }
                // Message Part references a type
                else if ((null != oMessagePart.Type) && !oMessagePart.Type.IsEmpty)
                {
                    // Create an element for this type
                    XmlSchemaElement oSimpleXmlSchemaElement = new XmlSchemaElement();
                    oSimpleXmlSchemaElement.Name = oMessagePart.Name;
                    oSimpleXmlSchemaElement.SchemaTypeName = oMessagePart.Type;
                    oParameters.Add(oSimpleXmlSchemaElement);
                }
            
            }

            //// If style='rpc', or style='literal' and there is >1 parameter, create the wrapper schema element, 
            //// add the current parameters and then set the wrapper to be the only parameter.  If this is the response 
            //// wrapper the name is not significant.
            //// In the case style='literal' and there is >1 parameter we do this as the SOAP body will contain >1 nodes
            //// and we need a wrapper to associate them.  We make note of this though via the AddedWrapperElement field.
            //if ((eBindingStyle == SoapBindingStyle.Rpc) || (oParameters.Count > 1))
            //{
            //    XmlSchemaElement oWrapper = new XmlSchemaElement();
            //    oWrapper.Name = MethodName;
            //    XmlSchemaComplexType oWrapperComplexType = new XmlSchemaComplexType();
            //    XmlSchemaSequence oWrapperSequence = new XmlSchemaSequence();
            //    // Put the current parameters into the wrapper
            //    for (int i = 0; i < oParameters.Count; i++)
            //        oWrapperSequence.Items.Add(oParameters[i]);
            //    oWrapperComplexType.Particle = oWrapperSequence;
            //    oWrapper.SchemaType = oWrapperComplexType;
            //    // Clear the current parameters and add the wrapper
            //    oParameters.Clear();
            //    oParameters.Add(oWrapper);

            //    if (oParameters.Count > 1)
            //        AddedWrapperElement = true;
            //}

            return oParameters;
        }


        /// <summary>
        /// Write the ParameterDesc List, using any ordering information that is available.
        /// </summary>
        private void WriteParameters(Operation oTargetOperation, List<XmlSchemaElement> oInputParameterElements, List<XmlSchemaElement> oOutputParameterElements)
        {
            ParameterDescs = new List<ParameterDesc>();
            oOutputParameterDescs = new List<ParameterDesc>();
            ReturnDesc = null;
            
            // If the binding style is Document
            if ((eBindingStyle == SoapBindingStyle.Document)
                || ((eBindingStyle == SoapBindingStyle.Default) && (eDefaultBindingStyle == SoapBindingStyle.Document)))
            {
                // Add all input parameters to the parameter descriptions
                for (int i = 0; i < oInputParameterElements.Count; i++)
                {
                    ParameterDesc oParameterDesc = new ParameterDesc();
                    oParameterDesc.Name = oInputParameterElements[i].Name;
                    oParameterDesc.ParamSchemaElement = oInputParameterElements[i];
                    oParameterDesc.ParamDirection = eParamDirection.In;
                    ParameterDescs.Add(oParameterDesc);
                }
                // For Document binding style no output parameters are passed in the request (unless in/out), so no
                // output parameters should be part of ParameterDescs (since they are not parameters of the request call)

                // Add all output parameters to the output parameter descriptions
                for (int i = 0; i < oOutputParameterElements.Count; i++)
                {
                    ParameterDesc oParameterDesc = new ParameterDesc();
                    oParameterDesc.Name = oOutputParameterElements[i].Name;
                    oParameterDesc.ParamSchemaElement = oOutputParameterElements[i];
                    oParameterDesc.ParamDirection = eParamDirection.Out;
                    oOutputParameterDescs.Add(oParameterDesc);
                }
            }
            // If the binding style is Rpc
            else
            {
                // If an order has been given use it
                string[] ParameterOrder = oTargetOperation.ParameterOrder;
                if (null == ParameterOrder)
                {
                    // Add all the input parameters.  This is only useful if there is one response element.  This will be caught at the
                    // end of the function.
                    ParameterOrder = new string[oInputParameterElements.Count];
                    for (int i = 0; i < oInputParameterElements.Count; i++)
                        ParameterOrder[i] = oInputParameterElements[i].Name;
                }

                List<XmlSchemaElement> ParameterElements = new List<XmlSchemaElement>();
                ParameterElements.AddRange(oInputParameterElements);
                ParameterElements.AddRange(oOutputParameterElements);
                // Set the up the order correctly
                for (int i = 0; i < ParameterOrder.Length; i++)
                {
                    bool bFoundParam = false;
                    String ParamName = ParameterOrder[i];
                    
                    for (int j = 0; j < ParameterElements.Count; j++)
                    {
                        if (ParameterElements[j].Name.Equals(ParamName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ParameterDesc oParameterDesc = new ParameterDesc();
                            oParameterDesc.Name = ParamName;
                            oParameterDesc.ParamSchemaElement = ParameterElements[j];
                            if(oInputParameterElements.Contains(ParameterElements[j]))
                                oParameterDesc.ParamDirection = eParamDirection.In;
                            else
                                oParameterDesc.ParamDirection = eParamDirection.Out;
                            ParameterDescs.Add(oParameterDesc);
                            bFoundParam = true;
                            ParameterElements.RemoveAt(j);
                            break;
                        }
                    }
                    if (!bFoundParam)
                        Log.Write(MethodBase.GetCurrentMethod(), "The parameterOrder specified for '" + oTargetOperation.Name + "' named a parameter that was not found", Log.LogType.Error);
                }

                // Try to set a return parameter
                // If we ParameterElements is zero then there is no return parameter.
                // If we ParameterElements is 1 then that is the return parameter
                if (ParameterElements.Count > 0)
                {
                    if (ParameterElements.Count == 1)
                    {
                        ReturnDesc = new ParameterDesc();
                        if (String.IsNullOrEmpty(ParameterElements[0].Name))
                            ReturnDesc.Name = "return";
                        else
                            ReturnDesc.Name = ParameterElements[0].Name;
                        ReturnDesc.ParamSchemaElement = ParameterElements[0];
                        ReturnDesc.ParamDirection = eParamDirection.Out;
                    }
                    else
                    {
                        if (null == oTargetOperation.ParameterOrder)
                            Log.Write(MethodBase.GetCurrentMethod(), "The parameterOrder specified for '" + oTargetOperation.Name + "' did not specify all parameters", Log.LogType.Error);
                        else
                            Log.Write(MethodBase.GetCurrentMethod(), "For Rpc binding operations, either a parameterOrder must be specified, or there must be only 1 node in the response message.", Log.LogType.Error);
                    }
                }
            }
        }

        #region Get or Set return parameter

        // From http://www.w3.org/TR/2007/REC-soap12-part2-20070427/#rpcresponse
        // 4.2.2 RPC Response
        // An RPC response is modeled as follows:
        // The response is represented by a single struct containing an outbound edge for the return value and each [out] or [in/out] parameter. 
        // The name of the struct is not significant.
        //
        // Each parameter is represented by an outbound edge with a label corresponding to the name of the parameter. The conventions of 
        // B. Mapping Application-Defined Names to XML Names SHOULD be used to represent parameter names that are not legal XML names.
        //
        // A non-void return value is represented as follows:
        //      There MUST be an outbound edge with a local name of 'result' and a namespace name of "http://www.w3.org/2003/05/soap-rpc" which 
        //      terminates in a terminal node
        //      The type of that terminal node is a xs:QName and its value is the name of the outbound edge which terminates in the actual 
        //      return value. 
        //
        // If the return value of the procedure is void then an outbound edge with a local name of result and a namespace name of 
        // "http://www.w3.org/2003/05/soap-rpc" MUST NOT be present.
        //
        // THE ABOVE IS USELESS!!!  It only gives us the return parameter after we make a call, we need to know the return parameter
        // before we make a call.

        /// <summary>
        /// For RPC binding style, and a parameter order, this function will find the parameter in the list that is not in the parameter 
        /// order, this is the return parameter.
        /// </summary>
        public static ParameterDesc GetReturnParameter(SoapBindingStyle eBindingStyle, eInvokeKind InvokeKind, Operation oTargetOperation, List<ParameterDesc> ParameterDescs)
        {
            ParameterDesc ReturnParamDesc = null;
            // If the Binding STYLE is not RPC then this convention is not used
            if (eBindingStyle != SoapBindingStyle.Rpc)
                return ReturnParamDesc;

            // If the method is put, then return
            if ( (InvokeKind == eInvokeKind.INVOKE_PROPERTYPUT) || (InvokeKind == eInvokeKind.INVOKE_PROPERTYPUTREF) )
                return ReturnParamDesc;
            // If the method is a get, there should be 1 parameter and it should be the return value
            else if (InvokeKind == eInvokeKind.INVOKE_PROPERTYGET)
            {
                if (ParameterDescs.Count != 1)
                    Log.Write(MethodBase.GetCurrentMethod(), "Encountered a get method '" + oTargetOperation.Name + "' that had more than 1 parameter", Log.LogType.Error);
                if(ParameterDescs[0].ParamDirection != eParamDirection.Out)
                    Log.Write(MethodBase.GetCurrentMethod(), "Encountered a get method '" + oTargetOperation.Name + "' with 1 parameter, but it was not an out parameter", Log.LogType.Error);
                ReturnParamDesc = ParameterDescs[0];
                ParameterDescs.RemoveAt(0);
            }
            else
            {
                // If no parameter order is specified for RPC then we cannot determine the return param
                if ((ParameterDescs.Count > 0) && (null == oTargetOperation.ParameterOrder))
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "The method '" + oTargetOperation.Name + "' does not have a parameterOrder, cannot determine return value", Log.LogType.Warning);
                    return ReturnParamDesc;
                }
                
                // Loop through all the output parameters
                for (int i = 0; i < ParameterDescs.Count; i++)
                {
                    if (ParameterDescs[i].ParamDirection == eParamDirection.Out)
                    {
                        bool bMatchFound = false;
                        // Loop through the parameter order
                        for (int j = 0; j < oTargetOperation.ParameterOrder.Length; j++)
                        {
                            if (ParameterDescs[i].ParamSchemaElement.Name.Equals(oTargetOperation.ParameterOrder[j]))
                            {
                                bMatchFound = true;
                                break;
                            }
                        }
                        if (!bMatchFound)
                        {
                            // Record the return param and remove from the parameter description list
                            ReturnParamDesc = ParameterDescs[i];
                            ParameterDescs.RemoveAt(i);
                        }
                    }
                }
            }

            return ReturnParamDesc;
        }

        #endregion
    }
}
