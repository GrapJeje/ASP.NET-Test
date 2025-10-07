using System.Net;
using System.Reflection;
using System.Text;
using ASPNET.app.attributes;
using ASPNET.app.Controllers;
using ASPNET.app.middleware;

namespace ASPNET;

public class WebServer
{
    public static HttpListenerContext context { get; set; }
    public static HttpListenerRequest request { get; set; }
    public static HttpListenerResponse response { get; set; }

    private readonly string[] prefixes;

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
            listener.Prefixes.Add(prefix);

        listener.Start();
        Console.WriteLine("Server is running...");

        var routes = InitRoutes();
        var middlewareMap = InitMiddlewares();

        while (true)
        {
            context = listener.GetContext();
            request = context.Request;
            response = context.Response;

            Console.WriteLine($"Received request for {request.Url}");

            var path = request.Url.AbsolutePath;

            if (routes.TryGetValue(path, out var routeInfo))
            {
                try
                {
                    foreach (var mwName in routeInfo.Middlewares)
                    {
                        if (middlewareMap.TryGetValue(mwName.ToLower(), out var mwType))
                        {
                            var middleware = (Middleware)Activator.CreateInstance(mwType)!;
                            middleware.Context = context;
                            Console.WriteLine($"Running middleware: {mwName}");
                            middleware.Handle();
                            
                            if (!response.OutputStream.CanWrite)
                                break;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Middleware '{mwName}' not found!");
                        }
                    }
                    
                    if (response.OutputStream.CanWrite)
                    {
                        var result = (string)routeInfo.Method.Invoke(null, null)!;

                        var buffer = Encoding.UTF8.GetBytes(result);
                        response.ContentType = "text/html; charset=UTF-8";
                        response.StatusCode = 200;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    var buffer = Encoding.UTF8.GetBytes("500 - Internal Server Error\n" + ex);
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
    
    private class RouteInfo
    {
        public MethodInfo Method { get; set; } = null!;
        public List<string> Middlewares { get; set; } = new();
    }
    
    private static Dictionary<string, RouteInfo> InitRoutes()
    {
        var routes = new Dictionary<string, RouteInfo>();

        var controllerTypes = typeof(Controller).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)));

        foreach (var controller in controllerTypes)
        {
            foreach (var method in controller.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var routeAttr = method.GetCustomAttribute<RouteAttribute>();
                if (routeAttr == null) continue;

                var mwAttrs = method.GetCustomAttributes<MiddlewareAttribute>().ToList();
                var middlewareNames = mwAttrs.Select(a => a.Name.ToLower()).ToList();

                var path = "/" + routeAttr.Path.Trim('/');
                routes[path] = new RouteInfo
                {
                    Method = method,
                    Middlewares = middlewareNames
                };

                Console.WriteLine($"Route registered: {path} -> {controller.Name}.{method.Name} " +
                                  $"(middlewares: {string.Join(", ", middlewareNames)})");
            }
        }

        return routes;
    }
    
    private static Dictionary<string, Type> InitMiddlewares()
    {
        var types = typeof(Middleware).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Middleware)));

        var result = new Dictionary<string, Type>();

        foreach (var type in types)
        {
            var instance = (Middleware)Activator.CreateInstance(type)!;
            var name = instance.GetName().ToLower();
            result[name] = type;

            Console.WriteLine($"Middleware registered: {name} -> {type.Name}");
        }

        return result;
    }
}
