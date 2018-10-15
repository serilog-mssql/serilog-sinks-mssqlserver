using System.Configuration;

// Disable XML comment warnings for internal config classes which are required to have public members
#pragma warning disable 1591

namespace Serilog.Sinks.MSSqlServer
{
    public class StandardColumnConfigProperties : ColumnConfig
    {
        public StandardColumnConfigProperties() : base()
        { }

        // override to set IsRequired = false
        [ConfigurationProperty("ColumnName", IsRequired = false, IsKey = true)]
        public override string ColumnName
        {
            get => base.ColumnName;
            set => base.ColumnName = value;
        }

        [ConfigurationProperty("ExcludeAdditionalProperties")]
        public string ExcludeAdditionalProperties
        {
            get => (string)base["ExcludeAdditionalProperties"];
            set
            {
                base["ExcludeAdditionalProperties"] = value;
            }
        }

        [ConfigurationProperty("DictionaryElementName")]
        public string DictionaryElementName
        {
            get => (string)base["DictionaryElementName"];
            set
            {
                base["DictionaryElementName"] = value;
            }
        }

        [ConfigurationProperty("ItemElementName")]
        public string ItemElementName
        {
            get => (string)base["ItemElementName"];
            set
            {
                base["ItemElementName"] = value;
            }
        }

        [ConfigurationProperty("OmitDictionaryContainerElement")]
        public string OmitDictionaryContainerElement
        {
            get => (string)base["OmitDictionaryContainerElement"];
            set
            {
                base["OmitDictionaryContainerElement"] = value;
            }
        }

        [ConfigurationProperty("OmitSequenceContainerElement")]
        public string OmitSequenceContainerElement
        {
            get => (string)base["OmitSequenceContainerElement"];
            set
            {
                base["OmitSequenceContainerElement"] = value;
            }
        }

        [ConfigurationProperty("OmitStructureContainerElement")]
        public string OmitStructureContainerElement
        {
            get => (string)base["OmitStructureContainerElement"];
            set
            {
                base["OmitStructureContainerElement"] = value;
            }
        }

        [ConfigurationProperty("OmitElementIfEmpty")]
        public string OmitElementIfEmpty
        {
            get => (string)base["OmitElementIfEmpty"];
            set
            {
                base["OmitElementIfEmpty"] = value;
            }
        }

        [ConfigurationProperty("PropertyElementName")]
        public string PropertyElementName
        {
            get => (string)base["PropertyElementName"];
            set
            {
                base["PropertyElementName"] = value;
            }
        }

        [ConfigurationProperty("RootElementName")]
        public string RootElementName
        {
            get => (string)base["RootElementName"];
            set
            {
                base["RootElementName"] = value;
            }
        }

        [ConfigurationProperty("SequenceElementName")]
        public string SequenceElementName
        {
            get => (string)base["SequenceElementName"];
            set
            {
                base["SequenceElementName"] = value;
            }
        }

        [ConfigurationProperty("StructureElementName")]
        public string StructureElementName
        {
            get => (string)base["StructureElementName"];
            set
            {
                base["StructureElementName"] = value;
            }
        }

        [ConfigurationProperty("UsePropertyKeyAsElementName")]
        public string UsePropertyKeyAsElementName
        {
            get => (string)base["UsePropertyKeyAsElementName"];
            set
            {
                base["UsePropertyKeyAsElementName"] = value;
            }
        }

    }
}

#pragma warning restore 1591

