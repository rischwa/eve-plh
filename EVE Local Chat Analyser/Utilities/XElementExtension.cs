using System.Xml.Linq;

namespace EveLocalChatAnalyser.Utilities
{
    public static class XElementExtension
    {
        public static string GetAttributeValue(this XElement node, string attributeName)
        {
            var attr = node.Attribute(attributeName);
            return attr != null ? attr.Value : null;
        }

        public static long GetLongAttributeValue(this XElement node, string attributeName)
        {
            var attr = node.Attribute(attributeName);
            return long.Parse(attr.Value);
        }

        public static int GetIntAttributeValue(this XElement node, string attributeName)
        {
            var attr = node.Attribute(attributeName);
            return int.Parse(attr.Value);
        }

        public static bool GetBoolAttributeValue(this XElement node, string attributeName)
        {
            return bool.Parse(node.Attribute(attributeName).Value);
        }
    }
}