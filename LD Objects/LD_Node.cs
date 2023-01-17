using System.Text.RegularExpressions;

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

    [JsonIgnore]
    public List<string> static_images => Regex.Matches(
        static_body,
        @"\((https:\/\/.*?\.(png|jpg|jpeg|gif))\)"
    ).Select(x => x.Groups[1].Value).ToList();

    [JsonIgnore]
    public List<string> static_gifs => static_images.Where(x => x.EndsWith(".gif")).ToList();
}