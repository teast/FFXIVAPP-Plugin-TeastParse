using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using ChatCodesClass = FFXIVAPP.Plugin.TeastParse.ChatCodes;

namespace FFXIVAPP.Plugin.TeastParse
{
    public static class ResourceReader
    {
        public static List<ChatCodesClass> ChatCodes()
        {
            using (var stream = typeof(Plugin).GetTypeInfo().Assembly.GetManifestResourceStream("FFXIVAPP.Plugin.TeastParse.Resources.ChatCodes.xml"))
            {
                var xdoc = new XmlDocument();
                xdoc.Load(stream);
                return ChatCodesClass.FromXmlDocument(xdoc).ToList();
            }
        }
    }
}