using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes.ErrorMessages
{
    public class CustomErrorMessagesSection : ConfigurationSection
    {

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CustomErrorMessages), AddItemName = "CustomErrorMessage")]
        public CustomErrorMessages Messages
        {
            get => (CustomErrorMessages) this[""];
            set => this[""] = value;
        }
        
        public override bool IsReadOnly()
        {
            return false;
        }

    }

    public class CustomErrorMessages : ConfigurationElementCollection
    {

        private static CustomErrorMessagesSection _DefaultSection;

        private Dictionary<int, CustomErrorMessage> _PageErrors;

        private Dictionary<string, CustomErrorMessage> _ValidationErrors;

        public Dictionary<int, CustomErrorMessage> PageErrors
        {
            get
            {
                if (_PageErrors == null)
                {
                    _PageErrors = this.ToList<CustomErrorMessage>()
                        .Where(e => string.IsNullOrWhiteSpace(e.Validator))
                        .ToDictionary(c => c.Code);
                }

                return _PageErrors;
            }
        }

        public Dictionary<string, CustomErrorMessage> ValidationErrors
        {
            get
            {
                if (_ValidationErrors == null)
                {
                    _ValidationErrors = this.ToList<CustomErrorMessage>()
                        .Where(e => !string.IsNullOrWhiteSpace(e.Validator))
                        .ToDictionary(c => c.Validator, StringComparer.CurrentCultureIgnoreCase);
                }

                return _ValidationErrors;
            }
        }

        public CustomErrorMessage this[int code] => PageErrors.ContainsKey(code) ? PageErrors[code] : null;

        protected override string ElementName => "CustomErrorMessage";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

        public static CustomErrorMessagesSection DefaultSection
        {
            get
            {
                if (_DefaultSection == null)
                {
                    string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}App_Data/CustomErrorMessages.config";
                    var map = new ExeConfigurationFileMap {ExeConfigFilename = filePath};
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                    ConfigurationSection theSection = config.GetSection("CustomErrorMessages");
                    _DefaultSection = (CustomErrorMessagesSection) theSection;
                }

                return _DefaultSection;
            }
        }

        public static CustomErrorMessage DefaultPageError
        {
            get { return DefaultSection.Messages.PageErrors.Values.FirstOrDefault(e => e.Default); }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomErrorMessage();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CustomErrorMessage) element).Code;
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals("CustomErrorMessage", StringComparison.InvariantCultureIgnoreCase);
        }

        public static CustomErrorMessage GetPageError(int code)
        {
            return DefaultSection.Messages.PageErrors.ContainsKey(code) ? DefaultSection.Messages.PageErrors?[code] : null;
        }

    }

    [Serializable]
    public class CustomErrorMessage : ConfigurationElement
    {

        private readonly bool _isReadOnly;

        [ConfigurationProperty("code", IsKey = true, IsRequired = true)]
        public int Code
        {
            get
            {
                var result = (int) base["code"];
                return result;
            }
            set => base["code"] = value;
        }

        [ConfigurationProperty("title", IsRequired = false)]
        public string Title
        {
            get => (string) base["title"];
            set => base["title"] = value;
        }

        [ConfigurationProperty("subtitle", IsRequired = false)]
        public string Subtitle
        {
            get => (string) base["subtitle"];
            set => base["subtitle"] = value;
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get => (string) base["description"];
            set => base["description"] = value;
        }

        [ConfigurationProperty("callToAction", IsRequired = false)]
        public string CallToAction
        {
            get => (string) base["callToAction"];
            set => base["callToAction"] = value;
        }

        [ConfigurationProperty("actionUrl", IsRequired = false)]
        public string ActionUrl
        {
            get => (string) base["actionUrl"];
            set => base["actionUrl"] = value;
        }

        [ConfigurationProperty("validator", IsRequired = false)]
        public string Validator
        {
            get => (string) base["validator"];
            set => base["validator"] = value;
        }

        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get => (string) base["displayName"];
            set => base["displayName"] = value;
        }

        [ConfigurationProperty("actionText", IsRequired = false, DefaultValue = "Continue")]
        public string ActionText
        {
            get => (string) base["actionText"];
            set => base["actionText"] = value;
        }

        [ConfigurationProperty("default", IsRequired = false, DefaultValue = false)]
        public bool Default
        {
            get
            {
                var result = (bool) base["default"];
                return result;
            }
            set => base["default"] = value;
        }

        public override bool IsReadOnly()
        {
            return _isReadOnly;
        }

        public override string ToString()
        {
            return $"{Title} {Description}".Trim();
        }

    }
}
