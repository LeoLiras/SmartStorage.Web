using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Hypermedia.Constants;

namespace SmartStorage_API.Hypermedia.Enricher
{
    public class EmployeeEnricher : ContentResponseEnricher<EmployeeVO>
    {
        private readonly object _lock = new object();

        protected override Task EnrichModel(EmployeeVO content, IUrlHelper urlHelper)
        {
            var path = "storage/employees/v1";
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
                Href = link + "/<<ID>>",
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultPut,
            });

            content.Links.Add(new HyperMediaLink()
            {
                Action = HttpActionVerb.DELETE,
                Href = link + "/<<ID>>",
                Rel = RelationType.self,
                Type = ResponseTypeFormat.DefaultDelete,
            });

            return null;
        }

        private string getLink(int id, IUrlHelper urlHelper, string path)
        {
            lock (_lock)
            {
                //var url = new { controller = path, id = id };
                //return new StringBuilder(urlHelper.Link("DefaultApi", url)).Replace("%2f", "/").ToString();
                return $"https://localhost:44330/api/{path}";
            }
        }
    }
}
