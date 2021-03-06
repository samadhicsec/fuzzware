using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common;
using Fuzzware.Schemer.AutoGenerated;
using Fuzzware.Schemer.Editors;

namespace Fuzzware.Schemer.Fuzzers.OccurrenceFuzzing
{
    class AtrOppositeOccurrence : FuzzerBase, IFuzzer
    {
        protected ComplexTypeFuzzerConfig config;
        protected AttributeEditor oEditor;

        public AtrOppositeOccurrence(ConfigData oConfigData, PreCompData oPreCompData)
            : base(oConfigData, oPreCompData)
        {
            // Add different types of occurance
            config = oConfigData.CTFConfig;

            if (null == config.AttributeFuzzing)
                return;

            if (null != config.AttributeFuzzing.AtrOppositeOccurrence)
            {
                // If an attribute is present then remove it, if it isn't present then add it
                Add("AtrOppositeOccurrence", FuzzAttributeOppositeOccurrence);
            }
        }

        public override IValuesEditor ValuesEditor
        {
            set
            {
                oValuesEditor = value;
                oEditor = value as AttributeEditor;
            }
        }

        /// <summary>
        /// If the specifed attribute exists, this removes it, if the specified attribute does not exist, it creates it.
        /// </summary>
        public bool FuzzAttributeOppositeOccurrence(int FuzzIndex, int NodeIndex)
        {
            if (oEditor == null)
                Log.Write(MethodBase.GetCurrentMethod(), "The Attribute Editor is null", Log.LogType.Error);

            if (FuzzIndex >= oEditor.FuzzableAttributes.Count)
                return false;

            if (-1 == NodeIndex)
                throw new SkipStateNoAllCaseException();

            // Check to see if the attribute exists
            XmlSchemaAttribute SchAtt = oEditor.FuzzableAttributes[FuzzIndex];
            if (oEditor.CheckAttributeExists(SchAtt.QualifiedName, NodeIndex))
            {
                // Remove it
                oEditor.DeleteAttribute(SchAtt.QualifiedName, NodeIndex);
            }
            else
            {
                // Create it
                oEditor.CreateAttribute(SchAtt, NodeIndex);
            }

            return true;
        }

        
    }
}
