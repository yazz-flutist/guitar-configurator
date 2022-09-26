using System;

namespace GuitarConfiguratorSharp.NetCore.Utils;

public partial class Asset
{
    public Uri Url { get; set; } = new("https://www.github.com");

    public long Id { get; set; } = 0;

    public string NodeId { get; set; } = "";

    public string Name { get; set; } = "";

    public string Label { get; set; } = "";

    public Author Uploader { get; set; } = new();

    public string ContentType { get; set; } = "";

    public string State { get; set; } = "";

    public long Size { get; set; } = 0;

    public long DownloadCount { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = new();

    public DateTimeOffset UpdatedAt { get; set; } = new();

    public Uri BrowserDownloadUrl { get; set; } = new("https://www.github.com");
}