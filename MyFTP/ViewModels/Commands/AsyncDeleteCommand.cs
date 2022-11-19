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
    class AsyncDeleteCommand : IAsyncRelayCommand<IEnumerable<FtpListItemViewModel>>
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;

        public AsyncDeleteCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
        {
            this.ftpListItemView = ftpListItemViewModel;
            this.items = items;
        }
        public async Task ExecuteAsync(IEnumerable<FtpListItemViewModel> arg, CancellationToken token)
        {
            try
            {
                if (arg != null && arg.Any())
                {
                    arg = arg.ToList(); // Force linq execution
                                        // Delete collection of items
                    if (await ftpListItemView.DialogService.AskForDeleteAsync(arg))
                    {
                        foreach (var item in arg)
                        {
                            try
                            {
                                if (item.Type == FtpObjectType.Directory)
                                    await item.Client.DeleteDirectoryAsync(item.FullName, token);
                                else
                                    await item.Client.DeleteFileAsync(item.FullName, token);
                                if (item.Parent != null)
                                    item.Parent.RemoveItem(item);
                            }
                            catch (Exception e)
                            {
                                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
                            }
                        }
                    }
                }
                else // Delete self
                {
                    if (await ftpListItemView.DialogService.AskForDeleteAsync(new FtpListItemViewModel[] { ftpListItemView }))
                    {
                        if (ftpListItemView.Type == FtpObjectType.Directory)
                            await ftpListItemView.Client.DeleteDirectoryAsync(ftpListItemView.FullName, token);
                        else
                            await ftpListItemView.Client.DeleteFileAsync(ftpListItemView.FullName, token);
                        if (ftpListItemView.Parent != null)
                            ftpListItemView.Parent.RemoveItem(ftpListItemView);
                    }
                }
            }
            catch (Exception e)
            {
                ftpListItemView.WeakMessenger.Send<ErrorMessage>(new ErrorMessage(e));
            }
        }
        public bool CanExecute(IEnumerable<FtpListItemViewModel> arg)
        {
            var canWritePermission = (ftpListItemView.OwnerPermissions & FtpPermission.Write) == FtpPermission.Write;
            var dialogServiceExists = ftpListItemView.DialogService != null;
            var containsItem = arg?.Any() == true;
            return canWritePermission && dialogServiceExists && (containsItem || arg is null);
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

        public void Execute(IEnumerable<FtpListItemViewModel> parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IEnumerable<FtpListItemViewModel> parameter)
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
