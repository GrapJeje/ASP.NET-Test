using System.Net;

namespace ASPNET.app.middleware;

public abstract class Middleware
{
    public HttpListenerContext Context { get; set; }

    public Middleware()
    {
        Context = WebServer.context;
    }
    
    public abstract string GetName();
    public abstract void Handle();
}