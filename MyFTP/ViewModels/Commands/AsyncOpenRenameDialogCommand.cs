using FluentFTP;
using Microsoft.Toolkit.Mvvm.Input;
using System;
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
using Windows.Storage;

namespace MyFTP.ViewModels.Commands
{
    class AsyncOpenRenameDialogCommand : IAsyncRelayCommand
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;

        public AsyncOpenRenameDialogCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                ftpListItemView.IsRenameDialogOpen = true;
                ftpListItemView.OpenRenameDialogCommand.NotifyCanExecuteChanged();
                await ftpListItemView.DialogService.OpenRenameDialogAsync(ftpListItemView.RenameCommand, ftpListItemView.Name);
            }
            catch (Exception e)
            {
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
            }
            finally
            {
                ftpListItemView.IsRenameDialogOpen = false;
                ftpListItemView.OpenRenameDialogCommand.NotifyCanExecuteChanged();
            }
        }
        public bool CanExecute(CancellationToken token)
        {
            var canWritePermission = (ftpListItemView.OwnerPermissions & FtpPermission.Write) == FtpPermission.Write;
            var dialogServiceExists = ftpListItemView.DialogService != null;
            var renameDialogIsClosed = !ftpListItemView.IsRenameDialogOpen;
            return canWritePermission && dialogServiceExists && renameDialogIsClosed;
        }
        public Task ExecutionTask => throw new NotImplementedException();

        public bool CanBeCanceled => throw new NotImplementedException();

        public bool IsCancellationRequested => throw new NotImplementedException();

        public bool IsRunning => throw new NotImplementedException();

        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
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
