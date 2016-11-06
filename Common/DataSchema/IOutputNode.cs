using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common.Encoding;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Common.DataSchema
{
    public interface IOutputNode
    {
        XmlQualifiedName QName
        {
            get;
        }

        XPathNavigator XPathNav
        {
            get;
        }

        System.Text.Encoding OutputEncoding
        {
            get;
        }

        String PIText
        {
            get;
        }

        TypeRestrictor Restrictor
        {
            get;
        }

        Coder.OutputAsType OutputAs
        {
            get;
        }

        DataSchemaTypeCode TypeCode
        {
            get;
        }
    }
}
