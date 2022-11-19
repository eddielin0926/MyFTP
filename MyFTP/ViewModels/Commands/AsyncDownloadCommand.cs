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
    class AsyncDownloadCommand : IAsyncRelayCommand<IEnumerable<FtpListItemViewModel>>
    {
        private FtpListItemViewModel ftpListItemView;
        private readonly IObservableSortedCollection<FtpListItemViewModel> items;

        public AsyncDownloadCommand(FtpListItemViewModel ftpListItemViewModel, IObservableSortedCollection<FtpListItemViewModel> items)
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
                    var folder = await ftpListItemView.WeakMessenger.Send<RequestOpenFolderMessage>();
                    if (folder != null)
                    {
                        foreach (var item in arg)
                        {
                            if (item.Type == FtpObjectType.Directory)
                            {
                                var _foder = await folder.CreateFolderAsync(item.Name, CreationCollisionOption.OpenIfExists);
                                ftpListItemView.TransferService.EnqueueDownload(item.Client, item.FullName, _foder);
                            }
                            else
                            {
                                var _file = await folder.CreateFileAsync(item.Name, CreationCollisionOption.ReplaceExisting);
                                ftpListItemView.TransferService.EnqueueDownload(item.Client, item.FullName, _file);
                            }
                        }
                    }
                }
                else if (ftpListItemView.Type == FtpObjectType.Directory)
                {
                    var folder = await ftpListItemView.WeakMessenger.Send<RequestOpenFolderMessage>();
                    if (folder != null)
                    {
                        ftpListItemView.TransferService.EnqueueDownload(ftpListItemView.Client, ftpListItemView.FullName, folder);
                    }
                }
                else
                {
                    var file = await ftpListItemView.WeakMessenger.Send<RequestSaveFileMessage>(new RequestSaveFileMessage() { FileNameSuggestion = ftpListItemView.Name });
                    if (file != null)
                    {
                        ftpListItemView.TransferService.EnqueueDownload(ftpListItemView.Client, ftpListItemView.FullName, file);
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
            var transferServiceExists = ftpListItemView.TransferService != null;
            var containsItem = arg?.Any() == true;
            return transferServiceExists && (containsItem || arg is null);
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
