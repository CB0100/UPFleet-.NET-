using System.Web;
using System.Web.Mvc;
using UPFleet.Filters;

namespace UPFleet
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomErrorAttribute());
        }
    }
}
