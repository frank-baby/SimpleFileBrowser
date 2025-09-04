using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleFileBrowser.Tests
{
    [TestClass]
    public class ItemTests
    {
        [TestMethod]
        public void Item_Type_ReturnsFolder_WhenIsDirectoryTrue()
        {
            var item = new Item { IsDirectory = true };

            var result = item.Type;

            Assert.AreEqual("Folder", result);
        }

        [TestMethod]
        public void Item_Type_ReturnsFile_WhenIsDirectoryFalse()
        {
            var item = new Item { IsDirectory = false };

            var result = item.Type;

            Assert.AreEqual("File", result);
        }
    }
}
