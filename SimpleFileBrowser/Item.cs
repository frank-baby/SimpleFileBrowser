using System;

namespace SimpleFileBrowser
{
    public class Item
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public string Size { get; set; }
        public DateTime DateModified { get; set; }
        public string Type => IsDirectory ? "Folder" : "File";
    }
}
