using Microsoft.Win32;
using System;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace ExportMapAddIn
{
	/// <summary>
	/// Interaction logic for ExportMapResult.xaml
	/// </summary>
	internal partial class ExportMapResult : Window
	{
		private readonly string _mapName;

		public ExportMapResult(Uri uri, string mapName)
		{
			InitializeComponent();
			Url = uri.ToString();
			_mapName = mapName;
			SaveCommand = new DelegateCommand(Save);
			OpenCommand = new DelegateCommand(Open);
			DataContext = this;
		}


		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		public static readonly DependencyProperty UrlProperty =
			DependencyProperty.Register("Url", typeof(string), typeof(ExportMapResult), null);

		#region IsBusy DP

		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		public static readonly DependencyProperty IsBusyProperty =
			DependencyProperty.Register("IsBusy", typeof(bool), typeof(ExportMapResult), null);

		#endregion

		#region SaveCommand
		public ICommand SaveCommand { get; private set; }

		private async void Save(object parameter)
		{
			var ind = Url.LastIndexOf('.');
			string ext = ind >= 0 ? Url.Substring(ind) : null;

			var dlg = new SaveFileDialog
				          {
					          FileName = _mapName ?? "Export Map", // Default file name
							  DefaultExt = ext, // Default file extension
					          Filter = ext != null && ext.StartsWith(".pdf")
						                   ? "PDF File|*.pdf"
						                   : "Image File|*.jpg;*.jpeg;*.gif;*.png;*.eps;*.svg"
				          };

			// Show save file dialog box
			bool? result = dlg.ShowDialog();

			// Process save file dialog box results
			if (result == true)
			{
				// Save document
				string fileName = dlg.FileName;
				try
				{
					IsBusy = true;
					var webClient = new WebClient();
					await webClient.DownloadFileTaskAsync(Url, fileName);
					DialogResult = true;
					Close();
				}
				catch (Exception)
				{
				}
				finally
				{
					IsBusy = false;
				}
			}
		}

		#endregion

		#region OpenCommand
		public ICommand OpenCommand { get; private set; }

		private void Open(object parameter)
		{
			System.Diagnostics.Process.Start(Url);
			DialogResult = true;
			Close();
		}
		#endregion
	}
}
