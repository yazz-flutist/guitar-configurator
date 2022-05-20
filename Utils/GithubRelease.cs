namespace GuitarConfiguratorSharp.Utils.Github
{
    using System;
    using System.Text.Json;

    public partial class GithubRelease
    {
        public Uri Url { get; set; } = new Uri("https://www.github.com");

        public Uri AssetsUrl { get; set; } = new Uri("https://www.github.com");

        public string UploadUrl { get; set; } = "";

        public Uri HtmlUrl { get; set; } = new Uri("https://www.github.com");

        public long Id { get; set; } = 0;

        public Author Author { get; set; } = new Author();

        public string NodeId { get; set; } = "";

        public string TagName { get; set; } = "";

        public string TargetCommitish { get; set; } = "";

        public string Name { get; set; } = "";

        public bool Draft { get; set; } = false;

        public bool Prerelease { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = new DateTimeOffset();

        public DateTimeOffset PublishedAt { get; set; } = new DateTimeOffset();

        public Asset[] Assets { get; set; } = new Asset[0];

        public Uri? TarballUrl { get; set; } = new Uri("https://www.github.com");

        public Uri? ZipballUrl { get; set; } = new Uri("https://www.github.com");

        public string Body { get; set; } = "";

        public Reactions Reactions { get; set; } = new Reactions();
    }

    public partial class Asset
    {
        public Uri Url { get; set; } = new Uri("https://www.github.com");

        public long Id { get; set; } = 0;

        public string NodeId { get; set; } = "";

        public string Name { get; set; } = "";

        public string Label { get; set; } = "";

        public Author Uploader { get; set; } = new Author();

        public string ContentType { get; set; } = "";

        public string State { get; set; } = "";

        public long Size { get; set; } = 0;

        public long DownloadCount { get; set; } = 0;

        public DateTimeOffset CreatedAt { get; set; } = new DateTimeOffset();

        public DateTimeOffset UpdatedAt { get; set; } = new DateTimeOffset();

        public Uri BrowserDownloadUrl { get; set; } = new Uri("https://www.github.com");
    }

    public partial class Author
    {
        public string Login { get; set; } = "";

        public long Id { get; set; } = 0;

        public string NodeId { get; set; } = "";

        public Uri AvatarUrl { get; set; } = new Uri("https://www.github.com");

        public string GravatarId { get; set; } = "";

        public Uri Url { get; set; } = new Uri("https://www.github.com");

        public Uri HtmlUrl { get; set; } = new Uri("https://www.github.com");

        public Uri FollowersUrl { get; set; } = new Uri("https://www.github.com");

        public Uri FollowingUrl { get; set; } = new Uri("https://www.github.com");

        public Uri GistsUrl { get; set; } = new Uri("https://www.github.com");

        public Uri StarredUrl { get; set; } = new Uri("https://www.github.com");

        public Uri SubscriptionsUrl { get; set; } = new Uri("https://www.github.com");

        public Uri OrganizationsUrl { get; set; } = new Uri("https://www.github.com");

        public Uri ReposUrl { get; set; } = new Uri("https://www.github.com");

        public Uri EventsUrl { get; set; } = new Uri("https://www.github.com");

        public Uri ReceivedEventsUrl { get; set; } = new Uri("https://www.github.com");

        public string Type { get; set; } = "";

        public bool SiteAdmin { get; set; } = false;
    }

    public partial class Reactions
    {
        public Uri Url { get; set; } = new Uri("https://www.github.com");

        public long TotalCount { get; set; } = 0;

        public long The1 { get; set; } = 0;

        public long Reactions1 { get; set; } = 0;

        public long Laugh { get; set; } = 0;

        public long Hooray { get; set; } = 0;

        public long Confused { get; set; } = 0;

        public long Heart { get; set; } = 0;

        public long Rocket { get; set; } = 0;

        public long Eyes { get; set; } = 0;
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
