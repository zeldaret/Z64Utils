using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class GithubUser
    {
        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("node_id")]
        public string? NodeId { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("gravatar_id")]
        public string? GravatarId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("followers_url")]
        public string? FollowersUrl { get; set; }

        [JsonPropertyName("following_url")]
        public string? FollowingUrl { get; set; }

        [JsonPropertyName("gists_url")]
        public string? GistsUrl { get; set; }

        [JsonPropertyName("starred_url")]
        public string? StarredUrl { get; set; }

        [JsonPropertyName("subscriptions_url")]
        public string? SubscriptionsUrl { get; set; }

        [JsonPropertyName("organizations_url")]
        public string? OrganizationsUrl { get; set; }

        [JsonPropertyName("repos_url")]
        public string? ReposUrl { get; set; }

        [JsonPropertyName("events_url")]
        public string? EventsUrl { get; set; }

        [JsonPropertyName("received_events_url")]
        public string? ReceivedEventsUrl { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class GithubAsset
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("node_id")]
        public string? NodeId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("uploader")]
        public GithubUser? Uploader { get; set; }

        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("size")]
        public int size { get; set; }

        [JsonPropertyName("download_count")]
        public int DownloadCount { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }

    public class GithubRelease
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("assets_url")]
        public string? AssetsUrl { get; set; }

        [JsonPropertyName("upload_url")]
        public string? UploadUrl { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("node_id")]
        public string? NodeId { get; set; }

        [JsonPropertyName("author")]
        public GithubUser? Author { get; set; }

        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("target_commitish")]
        public string? TargetCommitish { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool PreRelease { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public string? PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public List<GithubAsset>? Assets { get; set; }

        [JsonPropertyName("tarball_url")]
        public string? TarballUrl { get; set; }

        [JsonPropertyName("zipball_url")]
        public string? ZipballUrl { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }

    public static class UpdateChecker
    {
        public const string ReleaseURL =
            @"https://api.github.com/repos/zeldaret/Z64Utils/releases/latest";
        public static readonly string CurrentTag = "v" + Z64Utils.Program.Version;
        private static HttpClient GithubApiHttpClient = new()
        {
            BaseAddress = new("https://api.github.com"),
        };

        static UpdateChecker()
        {
            GithubApiHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Z64Utils Updater");
        }

        public static async Task<GithubRelease> GetLatestRelease()
        {
            CancellationTokenSource timeoutTokenSource = new();
            timeoutTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            var resp = await GithubApiHttpClient.GetAsync(ReleaseURL, timeoutTokenSource.Token);
            string json = await resp.Content.ReadAsStringAsync();
            var r = JsonSerializer.Deserialize<GithubRelease>(json);
            Utils.Assert(r != null);
            return r;
        }
    }
}
