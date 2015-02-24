using System.Linq;
using System.Threading.Tasks;
using Octokit;
using SkyJukebox.Core.Xml;

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
            var newprerels = releases.Where(r => r.Prerelease && r.PublishedAt > lr.PublishedAt).ToList();
            var lra = newprerels.Any() ? newprerels.Aggregate((max, x) => (x.PublishedAt > max.PublishedAt ? x : max)) : lr;
            var ctag = InstanceManager.Instance.CurrentReleaseTag;
            // There is a prerelease update if: (this is != the latest of all releases) and either (this release > the latest stable release) or (the setting is enabled)
            // There is a stable update if: (this is != the latest stable release) and (this is != the latest of all releases)
            return !lra.TagName.EqualsIgnoreCase(ctag) &&
                   (newprerels.Any(r => r.TagName.EqualsIgnoreCase(ctag)) ||
                    (bool)SettingsManager.Instance["DevUpdates"].Value)
                       ? await github.RenderText(lra.Body)
                       : lra.TagName.EqualsIgnoreCase(ctag) && lr.TagName.EqualsIgnoreCase(ctag)
                             ? ""
                             : await github.RenderText(lr.Body);
        }

        public static async Task<string> RenderText(this GitHubClient gtc, string txt)
        {
            return await gtc.Miscellaneous.RenderRawMarkdown(txt);
        }

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return s1.ToLowerInvariant().Equals(s2.ToLowerInvariant());
        }
    }
}
