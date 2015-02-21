using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace SkyJukebox.Utils
{
    public static class UpdateCheck
    {
        public static async Task<string> CheckForUpdate()
        {
            var github = new GitHubClient(new ProductHeaderValue("Sky-Jukebox-UpdateCheck"));
            var releaseClient = github.Release;
            var releases = await releaseClient.GetAll("OronDF343", "Sky-Jukebox");
            var rels = releases.Where(r => !r.Prerelease).ToList();
            if (!rels.Any()) return "";
            var lr = rels.Aggregate((max, x) => (x.PublishedAt > max.PublishedAt ? x : max));
            return lr == null || lr.TagName == InstanceManager.CurrentReleaseTag ? "" : await github.Miscellaneous.RenderRawMarkdown(lr.Body);
        }
    }
}
