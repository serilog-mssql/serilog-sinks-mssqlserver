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
            get => (string)base[nameof(ExcludeAdditionalProperties)];
            set
            {
                base[nameof(ExcludeAdditionalProperties)] = value;
            }
        }

        [ConfigurationProperty("DictionaryElementName")]
        public string DictionaryElementName
        {
            get => (string)base[nameof(DictionaryElementName)];
            set
            {
                base[nameof(DictionaryElementName)] = value;
            }
        }

        [ConfigurationProperty("ItemElementName")]
        public string ItemElementName
        {
            get => (string)base[nameof(ItemElementName)];
            set
            {
                base[nameof(ItemElementName)] = value;
            }
        }

        [ConfigurationProperty("OmitDictionaryContainerElement")]
        public string OmitDictionaryContainerElement
        {
            get => (string)base[nameof(OmitDictionaryContainerElement)];
            set
            {
                base[nameof(OmitDictionaryContainerElement)] = value;
            }
        }

        [ConfigurationProperty("OmitSequenceContainerElement")]
        public string OmitSequenceContainerElement
        {
            get => (string)base[nameof(OmitSequenceContainerElement)];
            set
            {
                base[nameof(OmitSequenceContainerElement)] = value;
            }
        }

        [ConfigurationProperty("OmitStructureContainerElement")]
        public string OmitStructureContainerElement
        {
            get => (string)base[nameof(OmitStructureContainerElement)];
            set
            {
                base[nameof(OmitStructureContainerElement)] = value;
            }
        }

        [ConfigurationProperty("OmitElementIfEmpty")]
        public string OmitElementIfEmpty
        {
            get => (string)base[nameof(OmitElementIfEmpty)];
            set
            {
                base[nameof(OmitElementIfEmpty)] = value;
            }
        }

        [ConfigurationProperty("PropertyElementName")]
        public string PropertyElementName
        {
            get => (string)base[nameof(PropertyElementName)];
            set
            {
                base[nameof(PropertyElementName)] = value;
            }
        }

        [ConfigurationProperty("RootElementName")]
        public string RootElementName
        {
            get => (string)base[nameof(RootElementName)];
            set
            {
                base[nameof(RootElementName)] = value;
            }
        }

        [ConfigurationProperty("SequenceElementName")]
        public string SequenceElementName
        {
            get => (string)base[nameof(SequenceElementName)];
            set
            {
                base[nameof(SequenceElementName)] = value;
            }
        }

        [ConfigurationProperty("StructureElementName")]
        public string StructureElementName
        {
            get => (string)base[nameof(StructureElementName)];
            set
            {
                base[nameof(StructureElementName)] = value;
            }
        }

        [ConfigurationProperty("UsePropertyKeyAsElementName")]
        public string UsePropertyKeyAsElementName
        {
            get => (string)base[nameof(UsePropertyKeyAsElementName)];
            set
            {
                base[nameof(UsePropertyKeyAsElementName)] = value;
            }
        }

    }
}

#pragma warning restore 1591

