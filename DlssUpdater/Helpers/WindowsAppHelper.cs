using System.IO;
using System.Xml.Linq;
using Windows.Management.Deployment;

namespace DLSSUpdater.Helpers
{
    public static class WindowsAppHelper
    {
        public static XDocument? InitXDocumentFromManifest(string manifestPath, out XNamespace? uap)
        {
            XDocument? xmlDoc = null;
            if (!File.Exists(manifestPath))
            {
                uap = null;
                return null;
            }

            // Load the XML file
            xmlDoc = XDocument.Load(manifestPath);
            
            // Extract the "uap" namespace dynamically from the XML
            uap = xmlDoc.Root?.GetNamespaceOfPrefix("uap");
            
            return xmlDoc;
        }
        
        public static string? GetIdentityNameFromXml(XDocument xmlDoc, XNamespace ns)
        {
            // Find the package element
            var packageElement = xmlDoc
                .Descendants("{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Package")
                .FirstOrDefault();

            // Get the identity name from the Identity element
            string? name = packageElement?
                .Descendants("{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity")
                .FirstOrDefault()?.Attribute("Name")?.Value;

            return name;
        }
        
        public static string? GetDisplayNameFromXml(XDocument xmlDoc, XNamespace uap)
        {
            string? displayName = null;

            // Query the DisplayName inside uap:VisualElements
            displayName = xmlDoc
                .Descendants(uap + "VisualElements")
                .FirstOrDefault()?.Attribute("DisplayName")?.Value;

            if (displayName is not null && displayName.Contains("AppDisplayName") || displayName is null)
            {
                displayName = xmlDoc.Descendants("{http://schemas.microsoft.com/appx/manifest/foundation/windows10}DisplayName")
                    .FirstOrDefault()?.Value;
            }

            return displayName;
        }
        
        public static List<ApplicationInfo> GetInstalledAppsFromWindowsStore()
        {
            List<ApplicationInfo> apps = new List<ApplicationInfo>();
            PackageManager packageManager = new PackageManager();
            
            // Query the package manager to get every packages installed.
            var packages = packageManager.FindPackagesForUserWithPackageTypes(
                string.Empty,
                PackageTypes.Main
            );

            foreach (var package in packages)
            {
                try
                {
                    if (package.InstalledLocation?.Path != null)
                    {
                        var manifestPath = Path.Combine(package.InstalledLocation?.Path!, "appxmanifest.xml");
                        var xmlDoc = InitXDocumentFromManifest(manifestPath, out var uap);
                        if (xmlDoc is not null && uap is not null)
                        {
                            var identityName = GetIdentityNameFromXml(xmlDoc, uap);
                            if(identityName is not null)
                            {
                                // Associate the correct name to the one found in the manifest
                                apps.Add(new ApplicationInfo
                                {
                                    displayName = package.DisplayName,
                                    identityName = identityName
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return apps;
        }
    }

    public class ApplicationInfo
    {
        public string displayName { get; set; }
        public string identityName { get; set; }
    }
}