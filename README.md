# SimpleFileBrowser

A WPF file browser application built with C# .NET Framework 4.8 using the MVVM design pattern.

## Features

- **Browse Directories**: Navigate through folders and view files
- **Real-time Filtering**: Auto-filter files and folders as you type
- **Partial String Matching**: Search supports substring matching
- **File Operations**: Double-click to open files with default applications or navigate into directories
- **Keyboard Support**: Press Enter to open selected items
- **Context Menu**: Right-click for additional actions
- **Responsive UI**: Clean, modern interface with proper error handling

## Architecture

### MVVM Pattern

- **Model**: `Item.cs` - Represents files and directories
- **View**: `MainWindow.xaml` - WPF user interface
- **ViewModel**: `MainViewModel.cs` - Business logic and data binding

### Services

- **IFileSystemService**: File and directory operations
- **IDialogService**: User notifications and dialogs
