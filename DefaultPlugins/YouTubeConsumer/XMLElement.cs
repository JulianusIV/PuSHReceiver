using System.ComponentModel;
using System.Xml.Serialization;

namespace DefaultPlugins.YouTubeConsumer
{
#pragma warning disable IDE1006
#pragma warning disable CS8618
#pragma warning disable CS8981
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    [XmlRoot(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
    public partial class feed
    {

        private feedLink[] linkField;

        private string titleField;

        private DateTime updatedField;

        private feedEntry entryField;

        /// <remarks/>
        [XmlElement("link")]
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
        public DateTime updated
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
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedLink
    {

        private string relField;

        private string hrefField;

        /// <remarks/>
        [XmlAttribute()]
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
        [XmlAttribute()]
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
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedEntry
    {

        private string idField;

        private string videoIdField;

        private string channelIdField;

        private string titleField;

        private feedEntryLink linkField;

        private feedEntryAuthor authorField;

        private DateTime publishedField;

        private DateTime updatedField;

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
        [XmlElement(Namespace = "http://www.youtube.com/xml/schemas/2015")]
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
        [XmlElement(Namespace = "http://www.youtube.com/xml/schemas/2015")]
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
        public DateTime published
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
        public DateTime updated
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
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    public partial class feedEntryLink
    {

        private string relField;

        private string hrefField;

        /// <remarks/>
        [XmlAttribute()]
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
        [XmlAttribute()]
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
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
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
#pragma warning restore CS8981
#pragma warning restore CS8618
#pragma warning restore IDE1006
}
