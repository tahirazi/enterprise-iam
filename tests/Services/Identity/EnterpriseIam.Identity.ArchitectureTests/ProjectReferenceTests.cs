// <copyright file="ProjectReferenceTests.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using System.Xml.Linq;
using NUnit.Framework;

namespace EnterpriseIam.Identity.ArchitectureTests;

[TestFixture]
public sealed class ProjectReferenceTests
{
    [TestCase("Domain")]
    [TestCase("Application", "Domain")]
    [TestCase("Infrastructure", "Application", "Domain")]
    [TestCase("Api", "Application", "Infrastructure")]
    public void Project_ShouldReferenceOnlyApprovedLayers(string layer, params string[] allowedLayers)
    {
        var root = FindRepositoryRoot();
        var projectName = $"EnterpriseIam.Identity.{layer}";
        var path = Path.Combine(root, "src", "Services", "Identity", projectName, $"{projectName}.csproj");
        var references = XDocument.Load(path)
            .Descendants("ProjectReference")
            .Select(reference => Path.GetFileNameWithoutExtension((string?)reference.Attribute("Include")))
            .ToArray();
        var allowed = allowedLayers.Select(item => $"EnterpriseIam.Identity.{item}").ToArray();

        Assert.That(references, Is.EquivalentTo(allowed));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
