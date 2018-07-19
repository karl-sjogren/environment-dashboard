using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EnvironmentDashboard.Api.Extensions {
    public static class XDocumentExtensions {
        public static string FirstValueOrDefault(this IEnumerable<XElement> enumerable) {
            var first = enumerable.FirstOrDefault();
            if (first == null)
                return default(string);

            return first.Value;
        }

        public static string FirstValueOrDefault(this IEnumerable<XAttribute> enumerable) {
            var first = enumerable.FirstOrDefault();
            if (first == null)
                return default(string);

            return first.Value;
        }

        public static string ValueOrDefault(this XAttribute attribute) {
            if (attribute == null)
                return default(string);

            return attribute.Value;
        }
    }
}