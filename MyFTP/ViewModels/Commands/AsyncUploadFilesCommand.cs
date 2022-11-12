using Microsoft.Toolkit.Mvvm.Input;
using MyFTP.Collections;
using MyFTP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using FluentFTP;

namespace MyFTP.ViewModels.Commands
{
    class AsyncUploadFilesCommand : IAsyncRelayCommand
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;
        public AsyncUploadFilesCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var files = await ftpListItemView.WeakMessenger.Send<RequestOpenFilesMessage>();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        bool result = true;
                        var remotePath = string.Format("{0}/{1}", ftpListItemView.FullName, file.Name);
                        if (ftpListItemView.DialogService != null && await ftpListItemView.Client.GetObjectInfoAsync(remotePath, token: token) is FtpListItem current)
                        {
                            result = await ftpListItemView.DialogService.AskForReplaceAsync(file, new FtpListItemViewModel(ftpListItemView.Client, current, ftpListItemView, null, null));
                        }
                        if (result)
                            ftpListItemView.TransferService.EnqueueUpload(ftpListItemView.Client, remotePath, file, ftpListItemView.Guid);
                    }
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
