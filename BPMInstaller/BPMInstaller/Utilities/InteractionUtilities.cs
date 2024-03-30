using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace BPMInstaller.UI.Desktop.Utilities
{
    public static class InteractionUtilities
    {
        public static (bool IsSelected, string Path) ShowFileSystemDialog(bool isDirectory, string? previousValue)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = isDirectory ? "Выбор директории" : "Выбора файла";
            dlg.IsFolderPicker = isDirectory;

            if (previousValue != null)
            {
                dlg.DefaultDirectory = previousValue;
            }

            dlg.AllowNonFileSystemItems = false;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;

            var isValueSelected = dlg.ShowDialog() == CommonFileDialogResult.Ok;

            var path = isValueSelected && !string.IsNullOrEmpty(dlg?.FileName) ?
                 dlg.FileName :
                 previousValue;

            return (isValueSelected, path);
        }

        public static bool ShowConfirmationButton(string header, string mesage)
        {
            return MessageBox.Show(mesage, header, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}
