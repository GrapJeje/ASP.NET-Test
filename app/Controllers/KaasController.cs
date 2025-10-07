using ASPNET.app.attributes;
using ASPNET.Views;

namespace ASPNET.app.Controllers;

public class KaasController : Controller
{
    [Route("kaas")]
    [Middleware("kaas")]
    public static string Index()
    {
        return new View("Kaas.html");
    }
}