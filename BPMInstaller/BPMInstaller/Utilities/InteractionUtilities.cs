﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace BPMInstaller.UI.Desktop.Utilities
{
    public static class InteractionUtilities
    {
        public static string? ShowFileSystemDialog(bool isDirectory, string? previousValue)
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

            return dlg.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dlg?.FileName) ?
                 dlg.FileName :
                 previousValue;
        }

        public static bool ShowConfirmationButton(string header, string mesage)
        {
            return MessageBox.Show(header, mesage, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}