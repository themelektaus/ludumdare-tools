namespace LudumDareTools;

public abstract class LD_Node : LD_Object
{
    public long parent;
    public long superparent;
    public string type;
    public DateTime published;
    public DateTime created;
    public DateTime modified;
    public long version;
    public string slug;
    public string name;
    public string body;
    public string path;

    [JsonIgnore]
    public string static_path => $"https://ldjam.com{path}";

    [JsonIgnore]
    public string static_body => body.Replace("///raw", "https://static.jam.vg/raw");

    [JsonIgnore]
    public string static_body_html => Markdig.Markdown.ToHtml(static_body);
}