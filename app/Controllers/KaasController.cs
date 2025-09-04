using ASPNET.attributes;

namespace ASPNET.app.Controllers;

public class KaasController : Controller
{
    [Route("kaas")]
    public static string Index()
    {
        return "KaasController.index";
    }
}