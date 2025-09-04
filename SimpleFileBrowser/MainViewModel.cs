using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SimpleFileBrowser
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Item> _items;
        private Item _selectedItem;
        private string _currentPath;
        private string _filterText;

        private readonly IFileSystemService _fileSystemService;
        private readonly IDialogService _dialogService;

        public MainViewModel(IFileSystemService fileSystemService, IDialogService dialogService)
        {
            _fileSystemService = fileSystemService;
            _dialogService = dialogService;

            CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            LoadItems();
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand GoCommand => new RelayCommand(_ => LoadItems());

        public ICommand OpenItemCommand => new RelayCommand(OpenItem, CanOpenItem);

        public ICommand SearchCommand => new RelayCommand(_ => LoadItems());

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged();
                    LoadItems();
                }
            }
        }

        public async void LoadItems()
        {
            if (string.IsNullOrWhiteSpace(CurrentPath))
                return;

            try
            {
                var items = await _fileSystemService.GetFilteredItemsAsync(CurrentPath, FilterText);

                Items = new ObservableCollection<Item>(items);
            }
            catch (UnauthorizedAccessException)
            {
                _dialogService.ShowWarning("Access denied to this folder");
                Items = new ObservableCollection<Item>();
            }
            catch (DirectoryNotFoundException)
            {
                _dialogService.ShowWarning("Folder not found");
                Items = new ObservableCollection<Item>();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading folder: {ex.Message}");
                Items = new ObservableCollection<Item>();
            }
        }

        private bool CanOpenItem(object arg)
        {
            var item = arg as Item ?? SelectedItem;
            return item != null;
        }

        private void OpenItem(object arg)
        {
            var item = arg as Item ?? SelectedItem;
            if (item == null) return;

            try
            {
                if (item.IsDirectory)
                {
                    CurrentPath = item.FullPath;
                    LoadItems();
                }
                else
                {
                    _fileSystemService.OpenFile(item.FullPath);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Unable to open: {ex.Message}");
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
