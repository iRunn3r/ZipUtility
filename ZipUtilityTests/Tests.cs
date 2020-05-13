using System.IO;
using System.Linq;
using ZipUtility;
using NUnit.Framework;

namespace ZipUtilityTests
{
    [TestFixture]
    public class Tests
    {
        private readonly string m_TestZip = 
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "test.zip");

        private ZipBundle m_TestBundle;

        [OneTimeSetUp]
        public void Setup()
        {
            m_TestBundle = new ZipBundle(m_TestZip);
        }

        [Test]
        public void BundleEntryCount_IsCorrect()
        {
            Assert.AreEqual(3, m_TestBundle.Entries.Count);
        }

        [Test]
        public void EntrySizeSum_IsLessThanBundleSize()
        {
            Assert.Less(m_TestBundle.Entries.Sum(entry => entry.CompressedSize), m_TestBundle.Length);
        }
    }
}