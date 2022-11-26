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
    class AsyncRefreshCommand : IAsyncRelayCommand
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel>items;

        public AsyncRefreshCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(CancellationToken token = default)
        {
            ftpListItemView.IsLoading = true;
            this.NotifyCanExecuteChanged();
            try
            {
                if (ftpListItemView.Type != FtpObjectType.Directory)
                    throw new NotSupportedException();
                // Load the root permission manually
                var result = await ftpListItemView.Client.GetListingAsync(ftpListItemView.FullName, token);

                items.Clear();
                foreach (var item in result)
                {
                    items.AddItem(new FtpListItemViewModel(ftpListItemView.Client, item, ftpListItemView, ftpListItemView.TransferService, ftpListItemView.DialogService));
                }
                ftpListItemView.IsLoaded = true;
            }
            catch (Exception e)
            {
                ftpListItemView.IsLoaded = false;
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
            }
            finally
            {
                ftpListItemView.IsLoading = false;
                this.NotifyCanExecuteChanged();
            }
        }
        public bool CanExecute(object parameter)
        {
            var isNotLoading = !ftpListItemView.IsLoading;
            var isDirectory = ftpListItemView.Type == FtpObjectType.Directory;

            return isNotLoading && ftpListItemView.IsDirectory;
        }
        public Task ExecuteAsync(object parameter)
        {
            throw new NotImplementedException();
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

        public void NotifyCanExecuteChanged()
        {
            throw new NotImplementedException();
        }
    }
}
