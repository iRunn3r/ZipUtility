using System.IO;
using System.Linq;
using ZipUtility;
using NUnit.Framework;

namespace ZipUtilityTests
{
    [TestFixture]
    public class Tests
    {
        private readonly string m_ZipPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "test.zip");
        private ZipBundle m_TestBundle;

        [OneTimeSetUp]
        public void Setup()
        {
            m_TestBundle = new ZipBundle(m_ZipPath);
        }

        [Test]
        public void BundleEntryCount_IsCorrect()
        {
            Assert.AreEqual(3, m_TestBundle.Entries.Count);
        }

        [Test]
        public void EntrySizes_AreCorrect()
        {
            Assert.AreEqual(0, m_TestBundle.Entries[0].CompressedSize);
            Assert.AreEqual(0, m_TestBundle.Entries[0].UncompressedSize);

            Assert.AreEqual(6, m_TestBundle.Entries[1].CompressedSize);
            Assert.AreEqual(14, m_TestBundle.Entries[1].UncompressedSize);

            Assert.AreEqual(8, m_TestBundle.Entries[2].CompressedSize);
            Assert.AreEqual(6, m_TestBundle.Entries[2].UncompressedSize);
        }

        [Test]
        public void EntrySizeSum_IsLessThanBundleSize()
        {
            Assert.Less(m_TestBundle.Entries.Sum(entry => entry.CompressedSize), m_TestBundle.Length);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            m_TestBundle.Dispose();
        }
    }
}
