using ASPNET.attributes;
using ASPNET.Views;

namespace ASPNET.app.Controllers;

public class KaasController : Controller
{
    [Route("kaas")]
    public static string Index()
    {
        return new View("Kaas.html");
    }
}