using FluentFTP;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using MyFTP.Utils;
using MyFTP.Collections;
using MyFTP.Services;

namespace MyFTP.ViewModels.Commands
{
    class AsyncRenameCommand : IAsyncRelayCommand<string>
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;

        public AsyncRenameCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(string newItemName, CancellationToken token)
        {
            try
            {
                ftpListItemView.IsRenaming = true;
                ftpListItemView.RenameCommand.NotifyCanExecuteChanged();
                var newRemotePath = ftpListItemView.FullName.Substring(0, ftpListItemView.FullName.Length - ftpListItemView.Name.Length) + newItemName;
                if (await ftpListItemView.Client.DirectoryExistsAsync(newRemotePath, token) || await ftpListItemView.Client.FileExistsAsync(newRemotePath))
                {
                    // "This name is already used by a directory or file"
                    var message = ftpListItemView.GetLocalized("NameAlreadyUsed");
                    throw new FtpException(message);
                }
                await ftpListItemView.Client.RenameAsync(ftpListItemView.FullName, newRemotePath, token: token);
                ftpListItemView.Name = newItemName;
                // OnPropertyChanged(nameof(ftpListItemView.FullName)); // TODO
            }
            catch (Exception e)
            {
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
                throw;
            }
            finally
            {
                ftpListItemView.IsRenaming = false;
                ftpListItemView.RenameCommand.NotifyCanExecuteChanged();
            }
        }
        public bool CanExecute(string itemName)
        {
            var canWritePermission = (ftpListItemView.OwnerPermissions & FtpPermission.Write) == FtpPermission.Write;
            var nameIsNoEmpty = !string.IsNullOrWhiteSpace(itemName);
            var nameIsValidPath = ftpListItemView.Type == FtpObjectType.Directory
                ? itemName?.IndexOfAny(Path.GetInvalidPathChars()) == -1
                : itemName?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            var notEqualsToCurrent = itemName != ftpListItemView.Name;
            var isNotRenaming = !ftpListItemView.IsRenaming;

            return canWritePermission && nameIsNoEmpty && nameIsValidPath && notEqualsToCurrent && isNotRenaming;
        }
        public Task ExecutionTask => throw new NotImplementedException();

        public bool CanBeCanceled => throw new NotImplementedException();

        public bool IsCancellationRequested => throw new NotImplementedException();

        public bool IsRunning => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(string parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(string parameter)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(object parameter)
        {
            throw new NotImplementedException();
        }

        public void NotifyCanExecuteChanged()
        {
            throw new NotImplementedException();
        }
    }
}
