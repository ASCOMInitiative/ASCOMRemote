using System;
using System.Collections.Generic;

using Semver;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;

namespace ASCOM.Remote
{
    internal class Updates
    {
        #region Internal properties

        /// <summary>
        /// True if a newer release verison is available
        /// </summary>
        internal static bool HasNewerRelease { get; set; }

        /// <summary>
        /// Latest release name
        /// </summary>
        internal static string LatestReleaseName { get; set; } = "";
        /// <summary>
        /// Latest release version
        /// </summary>
        internal static string LatestReleaseVersion { get; set; } = "";

        /// <summary>
        /// Download URL for the latest release version
        /// </summary>
        internal static string ReleaseUrl { get; set; }

        /// <summary>
        /// True if a new preview version is available
        /// </summary>
        internal static bool HasNewerPreview { get; set; }

        /// <summary>
        /// Latest preview version
        /// </summary>
        internal static string LatestPreviewName { get; set; } = "";

        /// <summary>
        /// Latest preview version
        /// </summary>
        internal static string LatestPreviewVersion { get; set; } = "";

        /// <summary>
        /// Download URL for the latest preview version
        /// </summary>
        internal static string PreviewURL { get; set; } = "";

        /// <summary>
        /// True if the client is running the latest release version
        /// </summary>
        internal static bool UpToDate { get; set; }

        /// <summary>
        /// True if the client has a version that is ahead of the latest preview release
        /// </summary>
        internal static bool AheadOfPreview { get; set; } = false;

        /// <summary>
        /// True if the client has a version that is ahead of the latest main release
        /// </summary>
        internal static bool AheadOfRelease { get; set; } = false;

        /// <summary>
        /// True if some releases have been retrieved from GitHub
        /// </summary>
        internal static bool HasReleases { get => Releases.Count > 0; }

        /// <summary>
        /// List of releases
        /// </summary>
        internal static IReadOnlyList<Octokit.Release> Releases { get; set; } = new List<Octokit.Release>(); //null;

        internal static string AscomRemoteVersion => $"{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

        internal static string AscomRemoteVersionDisplayString
        {
            get
            {
                string informationalVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                string shortGitId = $"{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyMetadataAttribute>().Value}";

                SemVersion.TryParse(informationalVersion, SemVersionStyles.AllowV, out SemVersion semver);
                if (semver is not null)
                    return $"{semver.Major}.{semver.Minor}.{semver.Patch}{(semver.Prerelease == "" ? "" : "-")}{semver.Prerelease} (Build {shortGitId})";
                else
                    return $" Bad informational version string: ##{informationalVersion}##";
            }
        }
        #endregion

        #region Internal methods

        internal static async Task<IReadOnlyList<Octokit.Release>> GetReleases()
        {
            try
            {
                LogDebug("GetReleases", "Getting release details");
                Releases = await GitHubReleases.GetReleases("ASCOMInitiative", "ASCOMRemote");
                SetProperties();
                LogDebug("GetReleases", $"Found {Releases.Count} releases");

                foreach (Octokit.Release release in Releases)
                {
                    LogDebug("CheckForUpdatesSync", $"Found release: {release.Name}, ReleaseSemVersionFromTag: {release.ReleaseSemVersionFromTag()}, Published on: {release.PublishedAt.GetValueOrDefault()}, Major: {release.ReleaseSemVersionFromTag().Major}, Minor: {release.ReleaseSemVersionFromTag().Minor}, Patch: {release.ReleaseSemVersionFromTag().Patch}, Pre-release: {release.Prerelease}");
                }

                return Releases;
            }
            catch (Exception ex)
            {
                LogDebug("GetReleases", $"Exception: {ex}");
                throw;
            }
        }

        internal async static Task CheckForUpdates()
        {
            try
            {
                LogDebug("CheckForUpdates", "Getting release details");
                Releases = await Task.Run(() => { return GitHubReleases.GetReleases("ASCOMInitiative", "ASCOMRemote"); });
                SetProperties();
                LogDebug("CheckForUpdates", $"Found {Releases.Count} releases");

                foreach (Octokit.Release release in Releases)
                {
                    LogDebug("CheckForUpdates", $"Found release: {release.Name}, ReleaseSemVersionFromTag: {release.ReleaseSemVersionFromTag()}, Published on: {release.PublishedAt.GetValueOrDefault()}, Major: {release.ReleaseSemVersionFromTag().Major}, Minor: {release.ReleaseSemVersionFromTag().Minor}, Patch: {release.ReleaseSemVersionFromTag().Patch}, Pre-release: {release.Prerelease}");
                }
            }
            catch (Exception ex)
            {
                LogDebug("CheckForUpdates", $"Exception: {ex}");
                throw;
            }
        }

        internal static bool UpdateAvailable()
        {
            try
            {
                if (Releases != null)
                {
                    if (Releases.Count > 0)
                    {
                        if (SemVersion.TryParse(Updates.AscomRemoteVersion, SemVersionStyles.AllowV, out SemVersion currentversion))
                        {
                            LogDebug("UpdateAvailable", $"Application semver - Major: {currentversion.Major}, Minor: {currentversion.Minor}, Patch: {currentversion.Patch}, Pre-release: {currentversion.Prerelease}, Metadata: {currentversion.Metadata}");
                            Octokit.Release Release = Releases?.Latest();

                            if (Release != null)
                            {
                                if (SemVersion.TryParse(Release.TagName, SemVersionStyles.AllowV, out SemVersion latestrelease))
                                {
                                    LogDebug("UpdateAvailable", $"Found release semver - Major: {latestrelease.Major}, Minor: {latestrelease.Minor}, Patch: {latestrelease.Patch}, Pre-release: {latestrelease.Prerelease}, Metadata: {latestrelease.Metadata}");
                                    return SemVersion.ComparePrecedence(currentversion, latestrelease) == -1;
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidValueException($"The informational product version set in the project file is not a valid SEMVER string: {Updates.AscomRemoteVersion}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("UpdateAvailable", $"Exception: {ex}");
            }
            return false;
        }

        #endregion

        #region Support code
        /// <summary>
        /// Set properties according to the releases returned
        /// </summary>
        /// <param name="logger">ConfgormLogger instance to record operational messages</param>
        private static void SetProperties()
        {
            try
            {
                LogDebug("SetProperties", $"Running...");
                if (SemVersion.TryParse(Updates.AscomRemoteVersion, SemVersionStyles.AllowV, out SemVersion installedVersion))
                {
                    LogDebug("SetProperties", $"Installed version: {installedVersion}");

                    Release latestRelease = Updates.Releases?.LatestRelease();
                    Release latestPreRelease = Updates.Releases?.LatestPrerelease();
                    if ((latestRelease is not null) & (latestPreRelease is not null))
                    {

                        bool latesOk = SemVersion.TryParse(latestRelease.TagName, SemVersionStyles.AllowV, out SemVersion latestVersion);

                        bool latestPreOk = SemVersion.TryParse(latestPreRelease.TagName, SemVersionStyles.AllowV, out SemVersion latestPreReleaseVersion);

                        LogDebug("SetProperties", $"latestrelease: {latestVersion}, latestprerelease: {latestPreReleaseVersion}");

                        if ((SemVersion.ComparePrecedence(installedVersion, latestVersion) == 0) || (SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == 0))
                        {
                            UpToDate = true;
                        }

                        if (latestVersion != null)
                        {
                            if (SemVersion.ComparePrecedence(installedVersion, latestVersion) == -1)  //(installedRelease < latestrelease)
                            {
                                HasNewerRelease = true;
                                LatestReleaseVersion = latestRelease.TagName;
                                LatestReleaseName = latestRelease.Name;
                                ReleaseUrl = latestRelease.HtmlUrl;
                            }

                            if (SemVersion.ComparePrecedence(installedVersion, latestVersion) == 1)  //(installedRelease > latestrelease)
                            {
                                LogDebug("SetProperties", $"Setting AheadOfRelease True");
                                AheadOfRelease = true;
                            }
                        }
                        else
                        {
                            latestVersion = new SemVersion(0);
                        }

                        if (latestPreReleaseVersion != null)
                        {
                            LogDebug("SetProperties", $"(SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == -1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == -1): {(SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == -1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == -1)}");
                            if ((SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == -1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == -1)) //installedVersion < latestPreReleaseVersion && latestVersion < latestPreReleaseVersion
                            {
                                HasNewerPreview = true;
                                LatestPreviewVersion = latestPreRelease.TagName;
                                LatestPreviewName = latestPreRelease.Name;
                                PreviewURL = latestPreRelease.HtmlUrl;
                            }

                            LogDebug("SetProperties", $"(SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == -1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == 1): {(SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == 1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == -1)}");
                            if ((SemVersion.ComparePrecedence(installedVersion, latestPreReleaseVersion) == 1) && (SemVersion.ComparePrecedence(latestVersion, latestPreReleaseVersion) == -1)) //(installedVersion > latestPreReleaseVersion && latestVersion < latestPreReleaseVersion)
                            {
                                AheadOfPreview = true;
                            }
                        }
                        LogDebug("SetProperties", $"UpToDate: {UpToDate}, HasNewerRelease: {HasNewerRelease}, HasNewerPreview: {HasNewerPreview}, AheadOfPreview: {AheadOfPreview}, LatestVersion: {LatestReleaseVersion}, URL: {ReleaseUrl}, LatestPreviewVersion: {LatestPreviewVersion}, PreviewURL: {PreviewURL}");
                    }
                }
                else
                {
                    LogDebug("SetProperties", $"Failed to parse {Updates.AscomRemoteVersion}");
                }
            }
            catch (Exception ex)
            {
                LogDebug("SetProperties", $"Exception: {ex}");
            }
        }

        static void LogDebug(string prefix, string message)
        {
            if (ServerForm.DebugTraceState)
                ServerForm.LogMessage(0, 0, 0, $"* {prefix}", message);
        }

        #endregion
    }
}
