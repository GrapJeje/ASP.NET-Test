namespace ASPNET.app.attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MiddlewareAttribute : Attribute
{ 
    public string Name { get; }

    public MiddlewareAttribute(string name)
    {
        Name = name;
    }
}