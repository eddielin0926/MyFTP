using Microsoft.Toolkit.Mvvm.Input;
using MyFTP.Collections;
using MyFTP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Threading;
using FluentFTP;

namespace MyFTP.ViewModels.Commands
{
    class AsyncUploadFolderCommand : IAsyncRelayCommand
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;
        public AsyncUploadFolderCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var folder = await ftpListItemView.WeakMessenger.Send<RequestOpenFolderMessage>();
                if (folder != null)
                {
                    var remotePath = string.Format("{0}/{1}", ftpListItemView.FullName, folder.Name);
                    ftpListItemView.TransferService.EnqueueUpload(ftpListItemView.Client, remotePath, folder, ftpListItemView.Guid);
                }
            }
            catch (Exception e)
            {
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
            }
        }
        public bool CanExecute(object parameter)
        {
            var canWritePermission = (ftpListItemView.OwnerPermissions & FtpPermission.Write) == FtpPermission.Write;
            var isDirectory = ftpListItemView.Type == FtpObjectType.Directory;
            var transferServiceExists = ftpListItemView.TransferService != null;

            return canWritePermission && ftpListItemView.IsDirectory && transferServiceExists;
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
