using FluentFTP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MyFTP.Services;
using MyFTP.Utils;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace MyFTP.ViewModels
{
	class FtpInfo
    {
		public string Host = null;
		public string Username = null;
		public string Password = null;
		public int Port = 0;
	}
	public class LoginViewModel : BindableItem
	{
		#region fields		
		private ObservableCollection<FtpHostSettingsViewModel> _savedCredentialsList;
		private string _host;
		private int _port;
		private string _username;
		private string _password;
		private bool _saveCredentials;
		private readonly ISettings settings;
		private bool _isBusy;
		#endregion

		#region properties		
		public ReadOnlyObservableCollection<FtpHostSettingsViewModel> SavedCredentialsList { get; }
		public string Host { get => _host; set => Set(ref _host, value); }
		public int Port { get => _port; set => Set(ref _port, value); }
		public string Username { get => _username; set => Set(ref _username, value); }
		public string Password { get => _password; set => Set(ref _password, value); }
		public bool SaveCredentials { get => _saveCredentials; set => Set(ref _saveCredentials, value); }
		public bool CanSaveCredentials => settings != null;
		public IAsyncRelayCommand LoginCommand { get; }
		#endregion

		#region constructor		
		public LoginViewModel(ISettings settings) : base(DispatcherQueue.GetForCurrentThread())
		{
			this.settings = settings;

			_savedCredentialsList = new ObservableCollection<FtpHostSettingsViewModel>();
			SavedCredentialsList = new ReadOnlyObservableCollection<FtpHostSettingsViewModel>(_savedCredentialsList);

			_saveCredentials = true;
			Port = 21;

			LoginCommand = new AsyncRelayCommand<FtpHostSettingsViewModel>(LoginCommandAsync, CanLogin);

			Task.Run(LoadSavedHostsAsync);
		}
		#endregion

		#region methods
		private bool CanLogin(FtpHostSettingsViewModel args) => !_isBusy;

		private async Task LoginCommandAsync(FtpHostSettingsViewModel arg, CancellationToken token)
		{
			ILogger logger = null;
			FtpClient client = null;
			FtpInfo ftpInfo = null;

			try
			{

				ftpInfo = SetLoginInfo(arg);
				logger = LoggerFactory.CreateLogger($"{ftpInfo.Host}:{ftpInfo.Port}:{ftpInfo.Username}");
				client = await ConnectServer(ftpInfo, logger, token);

				if (SaveCredentials && arg == null)
				{
					var ftpHostSettings = await FtpHostSettings.GetOrCreateAsync(ftpInfo.Host, ftpInfo.Port, ftpInfo.Username);
					if (!string.IsNullOrWhiteSpace(ftpInfo.Password))
						ftpHostSettings.SavePasswordOnLocker(ftpInfo.Password);
				}

				// Create FTPItemViewModel
				var rootViewModel = await CreateFtpItemViewModel(client, logger);

				// Send to view
				WeakReferenceMessenger.Default.Send<FtpListItemViewModel>(rootViewModel);
			}
			catch (Exception ex)
			{
				logger?.WriteLine(ex.ToString());
				logger?.Dispose();
				throw;
			}
			finally
			{
				_isBusy = false;
				LoginCommand.NotifyCanExecuteChanged();
			}
		}

		private FtpInfo SetLoginInfo(FtpHostSettingsViewModel arg)
        {
			FtpInfo ftpInfo = new FtpInfo();
			_isBusy = true;
			LoginCommand.NotifyCanExecuteChanged();

			if (arg == null)
			{
				ftpInfo.Host = Host;
				ftpInfo.Username = Username;
				ftpInfo.Password = Password;
				ftpInfo.Port = Port;
			}
			else
			{
				ftpInfo.Host = arg.Host;
				ftpInfo.Username = arg.Username;

				var credential = arg.Item.GetCredentialFromLocker();
				if (credential != null)
				{
					credential.RetrievePassword();
					ftpInfo.Password = credential.Password;
				}
				ftpInfo.Port = arg.Port;
			}
			return ftpInfo;
		}
        private async Task<FtpClient> ConnectServer(FtpInfo ftpInfo, ILogger logger, CancellationToken token)
        {
            FtpClient client = null;

            if (string.IsNullOrWhiteSpace(ftpInfo.Username) && string.IsNullOrWhiteSpace(ftpInfo.Password))
            {
                // Anonymous login
                client = new FtpClient(ftpInfo.Host);
                client.Port = Port;
            }
            else
            {
                client = new FtpClient(ftpInfo.Host, ftpInfo.Port, new NetworkCredential(ftpInfo.Username, ftpInfo.Password));
            }

            if (logger != null)
            {
                client.OnLogEvent += (s, e) => logger.WriteLine(e);
            }

			await client.ConnectAsync(token);

			return client;
        }
		private async Task<FtpListItemViewModel> CreateFtpItemViewModel(FtpClient client, ILogger logger)
        {
			// Load root item
			var root = await client.GetObjectInfoAsync("/");

			if (root is null)
			{
				var d = default(DateTime);
				root = new FtpListItem("", "/", -1, true, d)
				{
					FullName = "/",
					Type = FtpObjectType.Directory
				};
			}
			var transferService = App.Current.Services.GetService<ITransferItemService>();
			var dialogService = App.Current.Services.GetService<IDialogService>();

			return new FtpListItemViewModel(client, root, null, transferService, dialogService, logger);
		}

		public void Delete(FtpHostSettingsViewModel item)
		{
			_savedCredentialsList.Remove(item);
		}

		private async Task LoadSavedHostsAsync()
		{
			var hosts = await FtpHostSettings.GetAllAsync();
			await AccessUIAsync(() => _savedCredentialsList.Clear());
			foreach (var (_, host) in hosts)
			{
				await AccessUIAsync(() => _savedCredentialsList.Add(new FtpHostSettingsViewModel(host)));
			}
		}
		#endregion
	}
}