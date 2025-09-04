namespace SimpleFileBrowser
{
    public interface IDialogService
    {
        void ShowError(string message);

        void ShowWarning(string message);

        void ShowInformation(string message);
    }
}
