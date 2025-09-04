using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFileBrowser.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IFileSystemService> _mockFileSystemService;
        private Mock<IDialogService> _mockDialogService;
        private MainViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _mockFileSystemService = new Mock<IFileSystemService>();
            _mockDialogService = new Mock<IDialogService>();

            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new List<Item>());

            _viewModel = new MainViewModel(_mockFileSystemService.Object, _mockDialogService.Object);
        }

        [TestMethod]
        public void Constructor_SetsInitialPath_ToUserProfile()
        {
            Assert.AreEqual(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                         _viewModel.CurrentPath);
        }

        [TestMethod]
        public void Constructor_InitializesEmptyItemsCollection()
        {
            Assert.IsNotNull(_viewModel.Items);
        }

        [TestMethod]
        public async Task LoadItems_PopulatesItemsCollection_WhenServiceReturnsData()
        {
            var testItems = new List<Item>
            {
                new Item { Name = "File1.txt", IsDirectory = false },
                new Item { Name = "Folder1", IsDirectory = true }
            };
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, null))
                .ReturnsAsync(testItems);

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            Assert.AreEqual(2, _viewModel.Items.Count);
            Assert.IsTrue(_viewModel.Items.Any(i => i.Name == "File1.txt"));
            Assert.IsTrue(_viewModel.Items.Any(i => i.Name == "Folder1"));
        }

        [TestMethod]
        public void OpenItemCommand_ChangesPath_WhenItemIsDirectory()
        {
            var directory = new Item
            {
                Name = "TestFolder",
                FullPath = @"C:\TestFolder",
                IsDirectory = true
            };
            _viewModel.SelectedItem = directory;

            _viewModel.OpenItemCommand.Execute(null);

            Assert.AreEqual(@"C:\TestFolder", _viewModel.CurrentPath);
        }

        [TestMethod]
        public void OpenItemCommand_CallsOpenFile_WhenItemIsFile()
        {
            var file = new Item
            {
                Name = "test.txt",
                FullPath = @"C:\test.txt",
                IsDirectory = false
            };
            _viewModel.SelectedItem = file;

            _viewModel.OpenItemCommand.Execute(null);

            _mockFileSystemService.Verify(x => x.OpenFile(@"C:\test.txt"), Times.Once);
        }

        [TestMethod]
        public void OpenItemCommand_ShowsError_WhenExceptionOccurs()
        {
            var file = new Item
            {
                Name = "test.txt",
                FullPath = @"C:\test.txt",
                IsDirectory = false
            };
            _viewModel.SelectedItem = file;
            _mockFileSystemService.Setup(x => x.OpenFile(It.IsAny<string>()))
                .Throws(new Exception("Test error"));

            _viewModel.OpenItemCommand.Execute(null);

            _mockDialogService.Verify(x => x.ShowError("Unable to open: Test error"), Times.Once);
        }

        [TestMethod]
        public async Task LoadItems_ShowsWarning_ForUnauthorizedAccessException()
        {
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(It.IsAny<string>(), null))
                .ThrowsAsync(new UnauthorizedAccessException());

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockDialogService.Verify(x => x.ShowWarning("Access denied to this folder"), Times.Once);
            Assert.AreEqual(0, _viewModel.Items.Count);
        }

        [TestMethod]
        public async Task LoadItems_ShowsWarning_ForDirectoryNotFoundException()
        {
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(It.IsAny<string>(), null))
                .ThrowsAsync(new DirectoryNotFoundException());

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockDialogService.Verify(x => x.ShowWarning("Folder not found"), Times.Once);
            Assert.AreEqual(0, _viewModel.Items.Count);
        }

        [TestMethod]
        public async Task LoadItems_ShowsError_ForGeneralException()
        {
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(It.IsAny<string>(), null))
                .ThrowsAsync(new Exception("Unexpected error"));

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockDialogService.Verify(x => x.ShowError("Error loading folder: Unexpected error"), Times.Once);
            Assert.AreEqual(0, _viewModel.Items.Count);
        }

        [TestMethod]
        public void CanOpenItem_ReturnsFalse_WhenNoItemSelected()
        {
            _viewModel.SelectedItem = null;

            var canExecute = _viewModel.OpenItemCommand.CanExecute(null);

            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void CanOpenItem_ReturnsTrue_WhenItemSelected()
        {
            _viewModel.SelectedItem = new Item { Name = "test.txt" };

            var canExecute = _viewModel.OpenItemCommand.CanExecute(null);

            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void CurrentPath_RaisesPropertyChanged_WhenSet()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.CurrentPath))
                    propertyChangedRaised = true;
            };

            _viewModel.CurrentPath = @"C:\NewPath";

            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(@"C:\NewPath", _viewModel.CurrentPath);
        }

        [TestMethod]
        public void FilterText_RaisesPropertyChanged_WhenSet()
        {
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.FilterText))
                    propertyChangedRaised = true;
            };

            _viewModel.FilterText = "*.txt";

            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual("*.txt", _viewModel.FilterText);
        }

        [TestMethod]
        public async Task LoadItems_CallsGetFilteredItemsAsync_WhenFilterTextIsSet()
        {
            var testItems = new List<Item>
            {
                new Item { Name = "document.txt", IsDirectory = false },
                new Item { Name = "image.jpg", IsDirectory = false }
            };
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, "txt"))
                .ReturnsAsync(testItems.Where(i => i.Name.Contains("txt")).ToList());

            _viewModel.FilterText = "txt";

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockFileSystemService.Verify(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, "txt"), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task LoadItems_CallsGetFilteredItemsAsync_WhenFilterTextIsEmpty()
        {
            var testItems = new List<Item>
            {
                new Item { Name = "File1.txt", IsDirectory = false },
                new Item { Name = "Folder1", IsDirectory = true }
            };
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, null))
                .ReturnsAsync(testItems);

            _viewModel.FilterText = "";

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockFileSystemService.Verify(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, null), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task LoadItems_CallsGetItemsAsync_WhenFilterTextIsWildcard()
        {
            var testItems = new List<Item>
            {
                new Item { Name = "File1.txt", IsDirectory = false },
                new Item { Name = "Folder1", IsDirectory = true }
            };
            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath,"*.*"))
                .ReturnsAsync(testItems);

            _viewModel.FilterText = "*.*";

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            _mockFileSystemService.Verify(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, "*.*"), Times.AtLeastOnce);
        }

        [TestMethod]
        public void SearchCommand_CanExecute_ReturnsTrue()
        {
            var canExecute = _viewModel.SearchCommand.CanExecute(null);

            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void SearchCommand_Execute_CallsFilterItems()
        {
            _viewModel.FilterText = "test";

            _viewModel.SearchCommand.Execute(null);

            Assert.IsNotNull(_viewModel.Items);
        }

        [TestMethod]
        public async Task LoadItems_PopulatesFilteredItems_WhenFilterApplied()
        {
            var allItems = new List<Item>
            {
                new Item { Name = "document.txt", IsDirectory = false },
                new Item { Name = "image.jpg", IsDirectory = false },
                new Item { Name = "notes.txt", IsDirectory = false }
            };
            var filteredItems = allItems.Where(i => i.Name.Contains("txt")).ToList();

            _mockFileSystemService.Setup(x => x.GetFilteredItemsAsync(_viewModel.CurrentPath, "txt"))
                .ReturnsAsync(filteredItems);

            _viewModel.FilterText = "txt";

            await Task.Run(() => _viewModel.LoadItems());
            await Task.Delay(100);

            Assert.AreEqual(2, _viewModel.Items.Count);
            Assert.IsTrue(_viewModel.Items.All(i => i.Name.Contains("txt")));
        }
    }
}
