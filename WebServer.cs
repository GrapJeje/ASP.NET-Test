using System.Net;
using System.Reflection;
using System.Text;
using ASPNET.app.Controllers;
using ASPNET.attributes;

namespace ASPNET;

public class WebServer
{
    public static HttpListenerContext context { get; set; }
    public static HttpListenerRequest request { get; set; }
    public static HttpListenerResponse response { get; set; }

    private string[] prefixes;

    public WebServer(string[] prefixes)
    {
        this.prefixes = prefixes;
        Start();
    }

    private void Start()
    {
        Console.WriteLine("Starting server...");
        using var listener = new HttpListener();
        
        foreach (var prefix in prefixes)
        {
            listener.Prefixes.Add(prefix);
        }

        listener.Start();
        Console.WriteLine("Server is running...");

        var routes = InitRoutes();
        while (true)
        {
            context = listener.GetContext();
            request = context.Request;
            response = context.Response;

            Console.WriteLine($"Received request for {request.Url}");

            if (routes.TryGetValue(request.Url.AbsolutePath, out var method))
            {
                try
                {
                    var result = (string)method.Invoke(null, null);
                    
                    var buffer = Encoding.UTF8.GetBytes(result);
                    response.ContentType = "text/html; charset=UTF-8";
                    response.StatusCode = 200;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    var buffer = Encoding.UTF8.GetBytes("500 - Internal Server Error\n" + ex.Message);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                response.StatusCode = 404;
                var buffer = Encoding.UTF8.GetBytes("404 - Not Found");
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            response.OutputStream.Close();
        }
    }

    private static Dictionary<string, MethodInfo> InitRoutes()
    {
        var routes = new Dictionary<string, MethodInfo>();
        
        var controllerTypes = typeof(Controller).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)));

        foreach (var controller in controllerTypes)
        {
            foreach (var method in controller.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<RouteAttribute>();
                if (attr == null) continue;
                var path = "/" + attr.Path.Trim('/');
                routes[path] = method;
                Console.WriteLine($"Route registered: {path} -> {controller.Name}.{method.Name}");
            }
        }

        return routes;
    }
}