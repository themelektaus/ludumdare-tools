using Newtonsoft.Json;
using System;

public struct Grade
{
    public long id;
    public string name;
    public float value;
    public DateTime timestamp;
}

public struct Rating
{
    public float overall;
    public float fun;
    public float innovation;
    public float theme;
    public float graphics;
    public float audio;
    public float humor;
    public float mood;
}

public class Node
{
    public struct Magic
    {
        public float cool;
        public int feedback;
        public float given;
        public float grade;
        public float smart;
    }

    public long id;
    public long parent;
    public long superparent;
    public long author;
    public string type;
    public string subtype;
    public string subsubtype;
    public DateTime published;
    public DateTime created;
    public DateTime modified;
    public long version;
    public string slug;
    public string name;
    public string body;
    public string path;
    public int comments;
    public Magic magic;
    public string cover;

    [JsonIgnore]
    public Rating rating;
    
    [JsonIgnore]
    public User user;

    [JsonIgnore]
    public string static_cover =>
        cover?.Replace("///content", "https://static.jam.vg/content") + ".fit.jpg" ?? "";

    [JsonIgnore]
    public string static_body =>
        body.Replace("///raw", "https://static.jam.vg/raw");

    [JsonIgnore]
    public string static_path =>
        $"https://ldjam.com{path}";
}

public class User
{
    public long id;
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
    public string static_body =>
        body.Replace("///raw", "https://static.jam.vg/raw");

    [JsonIgnore]
    public string static_path =>
        $"https://ldjam.com{path}";
}