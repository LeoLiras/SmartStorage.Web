using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Hypermedia.Constants;
using System.Text;

namespace SmartStorage_API.Hypermedia.Enricher
{
    public class EnterEnricher : ContentResponseEnricher<EnterVO>
    {
        protected override Task EnrichModel(EnterVO content, IUrlHelper urlHelper)
        {
            var path = "storage/shelf";
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
                Href = link,
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultPut,
            });

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.DELETE,
                Href = link,
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultDelete,
            });

            return Task.CompletedTask;
        }

        private string getLink(int id, IUrlHelper urlHelper, string path)
        {
            lock (this)
            {
                var url = new { controller = path };
                return new StringBuilder(urlHelper.Link("DefaultApi", url)).Append($"/allocation/{id}").Replace("%2f", "/").ToString();
            }
        }
    }
}
