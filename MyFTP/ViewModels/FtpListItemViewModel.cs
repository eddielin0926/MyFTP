using FluentFTP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MyFTP.Collections;
using MyFTP.Services;
using MyFTP.Utils;
using MyFTP.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils.Comparers;
using Windows.Storage;
using Windows.System;

namespace MyFTP.ViewModels
{
	public class FtpListItemViewModel : BindableItem, IDragTarget, IDropTarget
	{
		#region fields		
		private IObservableSortedCollection<FtpListItemViewModel> _items;
		private bool _isLoaded;
		private bool _isLoading;
		private bool _isRenameDialogOpen;
		private bool _isRenaming;
		private FtpPermission _ownerPermissions;
		private FtpPermission _groupPermissions;
		private FtpPermission _othersPermissions;
		private string _name;
		private FtpListItemViewModel _parent;
		private readonly FtpListItem _ftpItem;
		private readonly WeakReferenceMessenger _weakMessenger;
		private readonly ITransferItemService _transferService;
		private readonly IDialogService _dialogService;
		private readonly string _guid;
		#endregion

		#region constructor
		public FtpListItemViewModel(IFtpClient client,
							  FtpListItem item,
							  FtpListItemViewModel parent,
							  ITransferItemService transferService,
							  IDialogService dialogService,
							  ILogger logger = null)
		{
			Client = client ?? throw new ArgumentNullException(nameof(client));
			Parent = parent;

			_items = new ObservableSortedCollection<FtpListItemViewModel>(new FtpListItemComparer());
			Items = new ReadOnlyObservableCollection<FtpListItemViewModel>((ObservableCollection<FtpListItemViewModel>)_items);

			_isLoaded = _isLoading = false;

			RefreshCommand = new AsyncRefreshCommand(this, _items);
			UploadFilesCommand = new AsyncUploadFilesCommand(this, _items);
			UploadFolderCommand = new AsyncUploadFolderCommand(this, _items);
			DownloadCommand = new AsyncDownloadCommand(this, _items);
			DeleteCommand = new AsyncDeleteCommand(this, _items);
			OpenRenameDialogCommand = new AsyncOpenRenameDialogCommand(this, _items);
			RenameCommand = new AsyncRenameCommand(this, _items);
			CreateFolderCommand = new AsyncCreateFolderCommand(this, _items);


			Dispatcher = DispatcherQueue.GetForCurrentThread();

			_weakMessenger = WeakReferenceMessenger.Default;

			_transferService = transferService;
			_dialogService = dialogService;
			Logger = logger;
			_guid = System.Guid.NewGuid().ToString();
			_weakMessenger.Register<object, string>(this, _guid, UploadFinished);

			Client.EnableThreadSafeDataConnections = true;

			_ftpItem = item ?? throw new ArgumentNullException(nameof(item));

			if (item.Name == "/")
				Name = client.Host;
			else
				Name = item.Name;
			Type = item.Type;
			SubType = item.SubType;
			Size = item.Size;
			Modified = item.Modified;
			OwnerPermissions = item.OwnerPermissions;
			GroupPermissions = item.GroupPermissions;
			OthersPermissions = item.OthersPermissions;
		}
		#endregion

		#region properties
		public IFtpClient Client { get; }
		public string Name { get => _name; set => Set(ref _name, value); }
		public string FullName
		{
			get
			{
				if (Parent == null)
					return "/";
				else
					return Parent.FullName + "/" + Name;
			}
		}
		public FtpPermission OwnerPermissions { get => _ownerPermissions; set => Set(ref _ownerPermissions, value); }
		public FtpPermission GroupPermissions { get => _groupPermissions; set => Set(ref _groupPermissions, value); }
		public FtpPermission OthersPermissions { get => _othersPermissions; set => Set(ref _othersPermissions, value); }
		public FtpListItemViewModel Parent { get => _parent; set => Set(ref _parent, value); }
		public FtpObjectType Type { get; }
		public FtpObjectSubType SubType { get; }
		public bool IsDirectory => Type == FtpObjectType.Directory;
		public long Size { get; }
		public DateTime Modified { get; }
		public ReadOnlyObservableCollection<FtpListItemViewModel> Items { get; }
		public bool IsLoaded { get => _isLoaded; set => Set(ref _isLoaded, value); }
		public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }
		public bool IsRenameDialogOpen { get => _isRenameDialogOpen; set => Set(ref _isRenameDialogOpen, value); }
		public bool IsRenaming { get => _isRenaming; set => Set(ref _isRenaming, value); }
		public WeakReferenceMessenger WeakMessenger { get => _weakMessenger; }
		public ITransferItemService TransferService { get => _transferService; }
		public IDialogService DialogService { get => _dialogService; }
		public String Guid { get => _guid; }
		public ILogger Logger { get; }
		#endregion

		#region commands
		public IAsyncRelayCommand RefreshCommand { get; }
		public IAsyncRelayCommand UploadFilesCommand { get; }
		public IAsyncRelayCommand UploadFolderCommand { get; }
		public IAsyncRelayCommand<IEnumerable<FtpListItemViewModel>> DownloadCommand { get; }
		public IAsyncRelayCommand<IEnumerable<FtpListItemViewModel>> DeleteCommand { get; }
		public IAsyncRelayCommand OpenRenameDialogCommand { get; }
		public IAsyncRelayCommand<string> RenameCommand { get; }
		public IAsyncRelayCommand<string> CreateFolderCommand { get; }
		#endregion

		#region methods
		private async void UploadFinished(object recipient, object message)
		{
			try
			{
				if (message is ITransferItem transferItem)
				{
					var item = await Client.GetObjectInfoAsync(transferItem.RemotePath, false);
					if (item != null)
					{
						var search = _items
							.Select((_item, index) => (_item, index))
							.FirstOrDefault(x => x._item.FullName == transferItem.RemotePath);

						if (search == default)
						{
							await AccessUIAsync(() => _items.AddItem(new FtpListItemViewModel(Client, item, this, _transferService, _dialogService)));
						}
						else
						{
							await AccessUIAsync(() => _items[search.index] = new FtpListItemViewModel(Client, item, this, _transferService, _dialogService));
						}
					}
				}
			}
			catch { }
		}

		public async void DropItems(IEnumerable<IDragTarget> items)
		{
			int successCount = 0, errorCount = 0;
			foreach (var item in items.Cast<FtpListItemViewModel>())
			{
				try
				{
					var newRemotePath = string.Format("{0}/{1}", FullName, item.Name);
					bool hasSuccess = false;
					switch (item.Type)
					{
						case FtpObjectType.File:
							hasSuccess = await Client.MoveFileAsync(item.FullName, newRemotePath, FtpRemoteExists.Skip, default);
							break;

						case FtpObjectType.Directory:
							hasSuccess = await Client.MoveDirectoryAsync(item.FullName, newRemotePath, FtpRemoteExists.Skip, default);
							break;
					}

					if (hasSuccess)
					{
						successCount++;
						item.Parent?._items.RemoveItem(item);
						item.Parent = this;
						OnPropertyChanged(FullName);
						_items.AddItem(item);
					}
					else
					{
						errorCount++;
					}
				}
				catch
				{
					errorCount++;
				}
			}

			if (errorCount != 0)
			{
				// "Items cannot be moved"
				var message = string.Format("{0}: {1}", GetLocalized("ItemsCannotBeMoved"), errorCount);
				_weakMessenger.Send(new ErrorMessage(new FtpException(message)));
			}
		}

		public void DropItems(IReadOnlyList<IStorageItem> items)
		{
			foreach (var item in items)
			{
				var remotePath = string.Format("{0}/{1}", FullName, item.Name);
				if (item.IsOfType(StorageItemTypes.Folder))
				{
					_transferService.EnqueueUpload(Client, remotePath, (StorageFolder)item, _guid);
				}
				else
				{
					_transferService.EnqueueUpload(Client, remotePath, (StorageFile)item, _guid);
				}
			}
		}

		public bool IsDragItemSupported(IDragTarget item) => item.GetType() == typeof(FtpListItemViewModel) && item != this;

		public string GetLocalized(string resourceName)
		{
			var settings = App.Current.Services.GetService<ISettings>();
			if (settings == null)
				return "[Error: No ISettings service]";
			return settings.GetStringFromResource(resourceName, "Messages");
		}

		public void RemoveItem(FtpListItemViewModel item)
        {
			_items.RemoveItem(item);
        }
		#endregion
	}
}