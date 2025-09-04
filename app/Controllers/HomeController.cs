using ASPNET.attributes;
using ASPNET.Views;

namespace ASPNET.app.Controllers;

public class HomeController : Controller
{
    [Route("")]
    public static string Index()
    {
        return new View("Home.html");
    }
}