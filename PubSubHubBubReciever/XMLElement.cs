namespace PubSubHubBubReciever
{
#pragma warning disable IDE1006
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
    public partial class feed
    {

        private feedLink[] linkField;

        private string titleField;

        private System.DateTime updatedField;

        private feedEntry entryField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("link")]
        public feedLink[] link
        {
            get
            {
                return linkField;
            }
            set
            {
                linkField = value;
            }
        }

        /// <remarks/>
        public string title
        {
            get
            {
                return titleField;
            }
            set
            {
                titleField = value;
            }
        }

        /// <remarks/>
        public System.DateTime updated
        {
            get
            {
                return updatedField;
            }
            set
            {
                updatedField = value;
            }
        }

        /// <remarks/>
        public feedEntry entry
        {
            get
            {
                return entryField;
            }
            set
            {
                entryField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedLink
    {

        private string relField;

        private string hrefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rel
        {
            get
            {
                return relField;
            }
            set
            {
                relField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string href
        {
            get
            {
                return hrefField;
            }
            set
            {
                hrefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedEntry
    {

        private string idField;

        private string videoIdField;

        private string channelIdField;

        private string titleField;

        private feedEntryLink linkField;

        private feedEntryAuthor authorField;

        private System.DateTime publishedField;

        private System.DateTime updatedField;

        /// <remarks/>
        public string id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string videoId
        {
            get
            {
                return videoIdField;
            }
            set
            {
                videoIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string channelId
        {
            get
            {
                return channelIdField;
            }
            set
            {
                channelIdField = value;
            }
        }

        /// <remarks/>
        public string title
        {
            get
            {
                return titleField;
            }
            set
            {
                titleField = value;
            }
        }

        /// <remarks/>
        public feedEntryLink link
        {
            get
            {
                return linkField;
            }
            set
            {
                linkField = value;
            }
        }

        /// <remarks/>
        public feedEntryAuthor author
        {
            get
            {
                return authorField;
            }
            set
            {
                authorField = value;
            }
        }

        /// <remarks/>
        public System.DateTime published
        {
            get
            {
                return publishedField;
            }
            set
            {
                publishedField = value;
            }
        }

        /// <remarks/>
        public System.DateTime updated
        {
            get
            {
                return updatedField;
            }
            set
            {
                updatedField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedEntryLink
    {

        private string relField;

        private string hrefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rel
        {
            get
            {
                return relField;
            }
            set
            {
                relField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string href
        {
            get
            {
                return hrefField;
            }
            set
            {
                hrefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedEntryAuthor
    {

        private string nameField;

        private string uriField;

        /// <remarks/>
        public string name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        public string uri
        {
            get
            {
                return uriField;
            }
            set
            {
                uriField = value;
            }
        }
    }
#pragma warning restore IDE1006
}
