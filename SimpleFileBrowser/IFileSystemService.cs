using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleFileBrowser
{
    public interface IFileSystemService
    {
        void OpenFile(string path);

        Task<IEnumerable<Item>> GetFilteredItemsAsync(string path, string searchPattern = null);
    }
}
