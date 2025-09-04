using System.Net;

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
        do
        {
            context = listener.GetContext();
            request = context.Request;
            response = context.Response;
            
            Console.WriteLine($"Received request for {request.Url}");

            var responseString = "<html><body>Hello, world!</body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        } while (true);
    }
}