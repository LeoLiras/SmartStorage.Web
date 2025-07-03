using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Hypermedia.Constants;
using System.Threading;

namespace SmartStorage_API.Hypermedia.Enricher
{
    public class ShelfEnricher : ContentResponseEnricher<ShelfVO>
    {
        private readonly object _lock = new object();

        protected override Task EnrichModel(ShelfVO content, IUrlHelper urlHelper)
        {
            var path = "storage/shelf/v1";
            string link = getLink(content.Id, urlHelper, path);

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.GET,
                Href = link,
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultGet,
            });

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.POST,
                Href = link,
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultPost,
            });

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.PUT,
                Href = link + "/{id}",
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultPut,
            });

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.DELETE,
                Href = link + "/{id}",
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultDelete,
            });

            return null;
        }

        private string getLink(int id, IUrlHelper urlHelper, string path)
        {
            lock (_lock)
            {
                return $"https://localhost:44330/api/{path}";
            }
        }
    }
}
