using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EveLocalChatAnalyser.Utilities
{
    public class CrestGroup
    {
        public string Name { get; set; }

        public string Href { get; set; }

        public IEnumerable<TypeInfo> Types
        {
            get
            {
                var uri = new Uri(Href);
                var groupDetail = uri.GetJsonResponse<CrestGroupDetail>();
                return groupDetail.Types.Select(x => x.AsTypeInfo());
            }
        }

        public class CrestType
        {
            public string Name { get; set; }

            public string Href { get; set; }

            public TypeInfo AsTypeInfo()
            {
                var index = Href.IndexOf("types/", StringComparison.Ordinal);
                var startIndex = index + "types/".Length;
                var idStr = Href.Substring(startIndex, Href.Length - startIndex - "/".Length);
                var id = int.Parse(idStr);

                return new TypeInfo(id, Name);
            }
        }

        public class CrestGroupDetail
        {
            public IList<CrestType> Types { get; set; }
        }
    }

    public class CrestCategory
    {
        public string Name { get; set; }

        public IList<CrestGroup> Groups { get; set; }
    }

    public class CrestTypeLoader : ITypeLoader
    {
        private static readonly Uri SHIP_CATEGORY_URI = new Uri("https://crest-tq.eveonline.com/inventory/categories/6/");
        private static readonly Uri CAPSULE_CATEGORY_URI = new Uri("https://crest-tq.eveonline.com/inventory/categories/29/");

        public async Task<TypeInfo[]> LoadShipTypes()
        {
            return await Task.Factory.StartNew(
                                               () =>
                                               {
                                                   var shipCategory = SHIP_CATEGORY_URI.GetJsonResponse<CrestCategory>();
                                                   var capsuleCateogry = CAPSULE_CATEGORY_URI.GetJsonResponse<CrestCategory>();
                                                   return shipCategory.Groups.SelectMany(x => x.Types).Concat(capsuleCateogry.Groups.SelectMany(x=>x.Types))
                                                       .ToArray();
                                               });
            ;
        }
    }
}
