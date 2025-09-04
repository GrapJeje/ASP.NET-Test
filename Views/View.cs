namespace ASPNET.Views;

public class View
{
    private string path;
    
    public View(string path)
    {
        this.path = path;
    }
    
    public override string ToString()
    {
        var projectDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
        var fullPath = Path.GetFullPath(Path.Combine(projectDir, "Views", path));
        return !File.Exists(fullPath) ? $"View '{path}' not found ({fullPath})" : File.ReadAllText(fullPath);
    }
    
    public static implicit operator string(View view)
    {
        return view.ToString();
    }
}