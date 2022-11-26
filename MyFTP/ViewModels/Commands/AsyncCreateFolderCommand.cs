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

namespace MyFTP.ViewModels.Commands
{
    class AsyncCreateFolderCommand : IAsyncRelayCommand<string>
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;

        public AsyncCreateFolderCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(string folderName, CancellationToken token)
        {
            try
            {
                var remotePath = string.Format("{0}/{1}", ftpListItemView.FullName, folderName);
                if (await ftpListItemView.Client.CreateDirectoryAsync(remotePath, false, token))
                {
                    var item = await ftpListItemView.Client.GetObjectInfoAsync(remotePath, false, token);
                    items.AddItem(new FtpListItemViewModel(ftpListItemView.Client, item, ftpListItemView, ftpListItemView.TransferService, ftpListItemView.DialogService));
                }
            }
            catch (Exception e)
            {
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
            }
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

        public bool CanExecute(string parameter)
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
