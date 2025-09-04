using System;

public interface IFileSystemService
{
    Task<IEnumerable<Item>> GetItemsAsync(string path);

    void OpenFile(string path);
}
