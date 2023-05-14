using Octokit;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public static class GitHubReleases
    {
        public static Task<IReadOnlyList<Release>> GetReleases(string owner, string name)
        {
            if (string.IsNullOrEmpty(owner))
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var Github = new GitHubClient(new ProductHeaderValue(name + @"-UpdateCheck"));

            return Github.Repository.Release.GetAll(owner, name);
        }

        public static Release LatestRelease(this IEnumerable<Release> releases)
        {
            if (releases == null)
            {
                throw new ArgumentNullException(nameof(releases));
            }
            return releases.Where(rp => !rp.Prerelease).Latest();
        }

        public static Release LatestPrerelease(this IEnumerable<Release> releases)
        {
            if (releases == null)
            {
                throw new ArgumentNullException(nameof(releases));
            }
            return releases.Where(rp => rp.Prerelease).Latest();

        }

        public static Release Latest(this IEnumerable<Release> releases)
        {
            if (releases == null)
            {
                throw new ArgumentNullException(nameof(releases));
            }
            if (releases.Any())
            {
                return releases.OrderBy(rp => rp.ReleaseSemVersionFromTag()).LastOrDefault();
            }
            return null;
        }

        public static SemVersion ReleaseSemVersionFromTag(this Release release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }
            if (!string.IsNullOrEmpty(release.TagName) && SemVersion.TryParse(release.TagName, SemVersionStyles.AllowV, out SemVersion _latest_release_version))
            {
                return _latest_release_version;
            }
            return SemVersion.ParsedFrom(0, 0, 0, release.TagName ?? "No Tag");
        }
    }
}
