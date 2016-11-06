using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.XML.Restrictions;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Schemer.Editors;
using Fuzzware.Schemer.Fuzzers.OccurrenceFuzzing;
using Fuzzware.Schemer.Fuzzers.NumberFuzzing;
using Fuzzware.Schemer.Fuzzers.RandomFuzzing;
using Fuzzware.Schemer.Fuzzers.StringFuzzing;
using Fuzzware.Schemer.Fuzzers.BinaryFuzzing;
using Fuzzware.Schemer.Fuzzers.BooleanFuzzing;
using Fuzzware.Schemer.Fuzzers.EnumFuzzing;
using Fuzzware.Schemer.Fuzzers.DateTimeFuzzing;

namespace Fuzzware.Schemer.Fuzzers
{
    class FuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        NumberFuzzerFactory oNumberFuzzerFactory;
        RandomFuzzerFactory oRandomFuzzerFactory;
        StringFuzzerFactory oStringFuzzerFactory;
        BinaryFuzzerFactory oBinaryFuzzerFactory;
        BooleanFuzzerFactory oBooleanFuzzerFactory;
        EnumFuzzerFactory oEnumFuzzerFactory;
        DateTimeFuzzerFactory oDateTimeFuzzerFactory;

        OccurrenceFuzzerFactory oOccurrenceFuzzerFactory;

        public FuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;

            oNumberFuzzerFactory = new NumberFuzzerFactory(ConfigData, PreComp);
            oRandomFuzzerFactory = new RandomFuzzerFactory(ConfigData, PreComp);
            oStringFuzzerFactory = new StringFuzzerFactory(ConfigData, PreComp);
            oBinaryFuzzerFactory = new BinaryFuzzerFactory(ConfigData, PreComp);
            oBooleanFuzzerFactory = new BooleanFuzzerFactory(ConfigData, PreComp);
            oEnumFuzzerFactory = new EnumFuzzerFactory(ConfigData, PreComp);
            oDateTimeFuzzerFactory = new DateTimeFuzzerFactory(ConfigData, PreComp);

            oOccurrenceFuzzerFactory = new OccurrenceFuzzerFactory(ConfigData, PreComp);
        }

        /// <summary>
        /// Creates Simple Type fuzzers and returns them in a list.
        /// </summary>
        /// <param name="TypeCode">The data type code by which to choose the fuzzers to use</param>
        /// <param name="oTypeRestrictor">The restrictions on this data type</param>
        /// <param name="oSchemaObject">The type of Schema Object being fuzzed</param>
        /// <returns></returns>
        public List<ITypeFuzzer> CreateSimpleTypeFuzzers(DataSchemaTypeCode TypeCode, TypeRestrictor oTypeRestrictor, ObjectDBEntry oSchemaObject)
        {
            // Create the return ITypeFuzzer list
            List<ITypeFuzzer> ITypeFuzzerList = new List<ITypeFuzzer>();

            // If the restrictions indcate this is an enumeration, then only call the EnumFuzzerFactory
            if (null != oTypeRestrictor.GetEnumerationValues())
            {
                oEnumFuzzerFactory.CreateEnumFuzzers(oTypeRestrictor, oSchemaObject, ITypeFuzzerList);
            }
            else
            {
                // Call the fuzzer factories of all the data types supported for fuzzing
                oNumberFuzzerFactory.CreateNumberFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
                oRandomFuzzerFactory.CreateRandomFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
                oStringFuzzerFactory.CreateStringFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
                oBinaryFuzzerFactory.CreateBinaryFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
                oBooleanFuzzerFactory.CreateBooleanFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
                oDateTimeFuzzerFactory.CreateDateTimeFuzzers(TypeCode, oSchemaObject, ITypeFuzzerList);
            }

            // For the convenience of using a loop, we cast out of the interface to assign the FacetRestrictions
            foreach (ITypeFuzzer oTypeFuzzer in ITypeFuzzerList)
            {
                (oTypeFuzzer as FuzzerBase).FacetRestrictions = oTypeRestrictor;
            }

            return ITypeFuzzerList;
        }

        /// <summary>
        /// Create Complex Type fuzzers and return them in a list.
        /// </summary>
        /// <param name="oElementId">The ElementId of the Complex Type to be fuzzed</param>
        /// <param name="XPathNodes">An XPathObjectList referencing the nodes to be fuzzed</param>
        /// <returns>A list of IFuzzers</returns>
        public List<IFuzzer> CreateComplexTypeFuzzers(XMLElementIdentifier oElementId, XPathObjectList XPathNodes)
        {
            // Create the return IFuzzer list
            List<IFuzzer> IFuzzerList = new List<IFuzzer>();

            oOccurrenceFuzzerFactory.CreateOccurrenceFuzzers(oElementId, IFuzzerList);

            for (int i = 0; i < IFuzzerList.Count; i++)
            {
                if (IFuzzerList[i] is IChildNodesFuzzer)
                {
                    (IFuzzerList[i] as IChildNodesFuzzer).ChildNodesEditor = new ChildNodeEditor(XPathNodes, oPreComp);
                }
            }

            return IFuzzerList;
        }

        /// <summary>
        /// Create Attribute fuzzers.  These fuzzers do not alter the value of the attributes, the value of attributes
        /// are fuzzed as Simple Types.
        /// </summary>
        /// <returns></returns>
        public List<IFuzzer> CreateAttributeFuzzers(XMLElementIdentifier oElementId, XPathObjectList XPathNodes)
        {
            // Create the return IFuzzer list
            List<IFuzzer> IFuzzerList = new List<IFuzzer>();

            oOccurrenceFuzzerFactory.CreateAtrOccurrenceFuzzers(oElementId, IFuzzerList);

            for (int i = 0; i < IFuzzerList.Count; i++)
            {
                if (IFuzzerList[i] is FuzzerBase)
                {
                    (IFuzzerList[i] as FuzzerBase).ValuesEditor = new AttributeEditor(XPathNodes, oElementId, oPreComp);
                }
            }

            return IFuzzerList;
        }
    }
}
