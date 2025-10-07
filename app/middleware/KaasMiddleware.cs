namespace ASPNET.app.middleware;

public class KaasMiddleware : Middleware
{
    public override string GetName() => "kaas";

    public override void Handle()
    {
        var query = Context.Request.QueryString;

        var kaas = query["cheese"];
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🧀 Incoming cheese request: {kaas ?? "none"}");
        Console.ResetColor();

        if (kaas == "brie") return;
        Context.Response.StatusCode = 403;
        Context.Response.ContentType = "text/plain";
        Context.Response.OutputStream.Write(
            System.Text.Encoding.UTF8.GetBytes(@"
            Access denied! Only Brie lovers allowed!
            
               ___
              /   \
             |     |
              \___/
               |||
               |||
               |||
            "),
            0,
            180
        );

        Context.Response.OutputStream.Close();
    }
}