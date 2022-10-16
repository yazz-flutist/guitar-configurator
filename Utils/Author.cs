using System;

namespace GuitarConfiguratorSharp.NetCore.Utils;

public class Author
{
    public string Login { get; set; } = "";

    public long Id { get; set; } = 0;

    public string NodeId { get; set; } = "";

    public Uri AvatarUrl { get; set; } = new("https://www.github.com");

    public string GravatarId { get; set; } = "";

    public Uri Url { get; set; } = new("https://www.github.com");

    public Uri HtmlUrl { get; set; } = new("https://www.github.com");

    public Uri FollowersUrl { get; set; } = new("https://www.github.com");

    public Uri FollowingUrl { get; set; } = new("https://www.github.com");

    public Uri GistsUrl { get; set; } = new("https://www.github.com");

    public Uri StarredUrl { get; set; } = new("https://www.github.com");

    public Uri SubscriptionsUrl { get; set; } = new("https://www.github.com");

    public Uri OrganizationsUrl { get; set; } = new("https://www.github.com");

    public Uri ReposUrl { get; set; } = new("https://www.github.com");

    public Uri EventsUrl { get; set; } = new("https://www.github.com");

    public Uri ReceivedEventsUrl { get; set; } = new("https://www.github.com");

    public string Type { get; set; } = "";

    public bool SiteAdmin { get; set; } = false;
}