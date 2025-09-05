using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFileBrowser
{
    public class FileSystemService : IFileSystemService
    {
        public void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(path);
            }
        }

        public async Task<IEnumerable<Item>> GetFilteredItemsAsync(string path, string searchPattern = null)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();

            return await Task.Run(() => GetItems(path, searchPattern));
        }

        private IEnumerable<Item> GetItems(string path, string searchPattern)
        {
            var items = new List<Item>();

            items.AddRange(GetDirectoryItems(path, searchPattern));
            items.AddRange(GetFileItems(path, searchPattern));

            return items;
        }

        private IEnumerable<Item> GetDirectoryItems(string path, string searchPattern)
        {
            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);

                if (string.IsNullOrEmpty(searchPattern) ||
                    dirInfo.Name.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return CreateDirectoryItem(dirInfo);
                }
            }
        }

        private IEnumerable<Item> GetFileItems(string path, string searchPattern)
        {
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (string.IsNullOrEmpty(searchPattern) ||
                    fileInfo.Name.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return CreateFileItem(fileInfo);
                }
            }
        }

        private static Item CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            return new Item
            {
                Name = dirInfo.Name,
                FullPath = dirInfo.FullName,
                DateModified = dirInfo.LastWriteTime,
                IsDirectory = true
            };
        }

        private static Item CreateFileItem(FileInfo fileInfo)
        {
            return new Item
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.FullName,
                DateModified = fileInfo.LastWriteTime,
                IsDirectory = false,
                Size = FormatFileSize(fileInfo.Length)
            };
        }

        private static string FormatFileSize(long bytes)
        {
            return bytes < 1024 ? $"{bytes} B" : $"{bytes / 1024} KB";
        }
    }
}
