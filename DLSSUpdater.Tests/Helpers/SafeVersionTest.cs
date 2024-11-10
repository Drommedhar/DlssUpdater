using System;
using DLSSUpdater.Helpers;
using NUnit.Framework;

namespace DLSSUpdater.Tests.Helpers;

[TestFixture]
[TestOf(typeof(SafeVersion))]
public class SafeVersionTest
{
    [Test]
    public void StringToSafeVersion()
    {
        var version = "1.2.3.4";
        var safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
        
        version = "10.20.30.40";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("10.20.30.40"));
        
        version = "1.2.3.4.u";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
        
        version = "1-2-3-4";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
        
        version = "1a2ab3ac4aa";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
        
        version = "xx1a-2abx3ac*4aa+";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
        
        version = "xx1a-2abx3ac*4aa+5rtuz;";
        safeVersion = new SafeVersion(version);
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.4"));
    }

    [Test]
    public void VersionToSafeVersion()
    {
        var version = new Version(1, 2, 3,4);
        var safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 0, 0, 1);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 0);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 0, 0, 0);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 2, 3, 0);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 2, 3);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
        
        version = new Version(1, 2);
        safeVersion = new SafeVersion(version);
        Assert.That((Version)safeVersion, Is.EqualTo(version));
    }

    [Test]
    public void CheckIfVersionIsSafe()
    {
        var safeVersion = new SafeVersion("1");
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.0.0.0"));
        
        safeVersion = new SafeVersion("1.2");
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.0.0"));
        
        safeVersion = new SafeVersion("1.2.3");
        Assert.That(safeVersion.ToString(), Is.EqualTo("1.2.3.0"));
    }
}