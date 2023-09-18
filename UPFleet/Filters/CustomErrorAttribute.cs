using System.Web.Mvc;

namespace UPFleet.Filters
{
    public class CustomErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            // Redirect to the error view
            filterContext.Result = new ViewResult
            {
                ViewName = "Error" // You need to create an "Error" view for displaying the error message
            };
        }
    }


}