namespace ASPNET;

internal class Program
{
    private static WebServer Server;

    public static void Main(string[] args)
    {
        string[] prefixes = { "http://localhost:8080/" };
        Server = new WebServer(prefixes);
    }
    
    public static WebServer GetServer()
    {
        return Server;
    }
}
