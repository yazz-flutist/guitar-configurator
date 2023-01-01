using System;
using System.Text.Json;

namespace GuitarConfiguratorSharp.NetCore.Utils
{
    public partial class GithubRelease
    {
        public Uri Url { get; set; } = new("https://www.github.com");

        public Uri AssetsUrl { get; set; } = new("https://www.github.com");

        public string UploadUrl { get; set; } = "";

        public Uri HtmlUrl { get; set; } = new("https://www.github.com");

        public long Id { get; set; }

        public Author Author { get; set; } = new();

        public string NodeId { get; set; } = "";

        public string TagName { get; set; } = "";

        public string TargetCommitish { get; set; } = "";

        public string Name { get; set; } = "";

        public bool Draft { get; set; }

        public bool Prerelease { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset PublishedAt { get; set; }

        public Asset[] Assets { get; set; } = Array.Empty<Asset>();

        public Uri? TarballUrl { get; set; } = new("https://www.github.com");

        public Uri? ZipballUrl { get; set; } = new("https://www.github.com");

        public string Body { get; set; } = "";

        public Reactions Reactions { get; set; } = new();
    }

    public partial class GithubRelease
    {
        public static GithubRelease FromJson(string json)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                WriteIndented = true
            };
            return JsonSerializer.Deserialize<GithubRelease>(json, serializeOptions)!;
        }
    }
}
