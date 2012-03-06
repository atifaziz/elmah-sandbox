using System.Configuration;
using System.Linq;
using System.Xml;

namespace Elmah.SignalR.Test
{
    public class ElmahRSectionHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            var configurators = from XmlNode node in section.SelectNodes("application")
                                select new ElmahRApplicationSection(
                                    GetStringValue(node, "name"),
                                    GetStringValue(node, "handshakeToken"));

            return new ElmahRSection(configurators.ToArray());
        }

        static string GetStringValue(XmlNode node, string attribute)
        {
            System.Diagnostics.Debug.Assert(node != null);
            System.Diagnostics.Debug.Assert(node.Attributes != null);

            var a = node.Attributes[attribute];
            return a == null
                       ? null
                       : a.Value;
        }
    }

    public class ElmahRSection
    {
        public ElmahRApplicationSection[] Applications { get; private set; }

        public ElmahRSection(ElmahRApplicationSection[] applications)
        {
            Applications = applications;
        }
    }

    public class ElmahRApplicationSection
    {
        public ElmahRApplicationSection(string applicationName, string handshakeToken)
        {
            ApplicationName = applicationName;
            HandshakeToken = handshakeToken;
        }

        public string ApplicationName { get; private set; }
        public string HandshakeToken { get; private set; }
    }
}