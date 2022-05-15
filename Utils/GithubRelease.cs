namespace GuitarConfiguratorSharp.Utils.Github
{
    using System;
    using System.Text.Json;

    public partial class GithubRelease
    {
        public Uri? Url { get; set; }

        public Uri? AssetsUrl { get; set; }

        public string? UploadUrl { get; set; }

        public Uri? HtmlUrl { get; set; }

        public long? Id { get; set; }

        public Author? Author { get; set; }

        public string? NodeId { get; set; }

        public string? TagName { get; set; }

        public string? TargetCommitish { get; set; }

        public string? Name { get; set; }

        public bool? Draft { get; set; }

        public bool? Prerelease { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public Asset[]? Assets { get; set; }

        public Uri? TarballUrl { get; set; }

        public Uri? ZipballUrl { get; set; }

        public string? Body { get; set; }

        public Reactions? Reactions { get; set; }
    }

    public partial class Asset
    {
        public Uri? Url { get; set; }

        public long? Id { get; set; }

        public string? NodeId { get; set; }

        public string? Name { get; set; }

        public string? Label { get; set; }

        public Author? Uploader { get; set; }

        public string? ContentType { get; set; }

        public string? State { get; set; }

        public long? Size { get; set; }

        public long? DownloadCount { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public Uri? BrowserDownloadUrl { get; set; }
    }

    public partial class Author
    {
        public string? Login { get; set; }

        public long? Id { get; set; }

        public string? NodeId { get; set; }

        public Uri? AvatarUrl { get; set; }

        public string? GravatarId { get; set; }

        public Uri? Url { get; set; }

        public Uri? HtmlUrl { get; set; }

        public Uri? FollowersUrl { get; set; }

        public Uri? FollowingUrl { get; set; }

        public Uri? GistsUrl { get; set; }

        public Uri? StarredUrl { get; set; }

        public Uri? SubscriptionsUrl { get; set; }

        public Uri? OrganizationsUrl { get; set; }

        public Uri? ReposUrl { get; set; }

        public Uri? EventsUrl { get; set; }

        public Uri? ReceivedEventsUrl { get; set; }

        public string? Type { get; set; }

        public bool? SiteAdmin { get; set; }
    }

    public partial class Reactions
    {
        public Uri? Url { get; set; }

        public long? TotalCount { get; set; }

        public long? The1 { get; set; }

        public long? Reactions1 { get; set; }

        public long? Laugh { get; set; }

        public long? Hooray { get; set; }

        public long? Confused { get; set; }

        public long? Heart { get; set; }

        public long? Rocket { get; set; }

        public long? Eyes { get; set; }
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
