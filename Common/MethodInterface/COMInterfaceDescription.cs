﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Services.Description;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Common.MethodInterface
{
    public class COMInterfaceDescription : WSDLInterfaceDescription
    {
        protected COMInterfaceDescription()
            : base()
        {
        }

        public COMInterfaceDescription(Service oService, WSDLInputProtocol Protocol)
            : base(oService, Protocol, oService.ServiceDescription.TargetNamespace, null)
        {
            
        }

        public override String GetMethodNodeName(MethodDescription oMethodDesc)
        {
            return Name + "." + oMethodDesc.MethodName;
        }
    }
}
