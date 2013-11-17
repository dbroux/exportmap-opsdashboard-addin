using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Printing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.OperationsDashboard;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ExportMapAddIn
{
	/// <summary>
	/// A MapTool is an extension to ArcGIS Operations Dashboard which can be configured to appear in the toolbar 
	/// of the map widget, providing a custom map tool.
	/// </summary>
	[Export("ESRI.ArcGIS.OperationsDashboard.MapTool")]
	[ExportMetadata("DisplayName", "Export Map")]
	[ExportMetadata("Description", "Export a map as image.")]
	[ExportMetadata("ImagePath", "/ExportMapAddIn;component/Images/ExportMap.png")]
	[DataContract]
	public partial class ExportMapTool : UserControl, IMapTool
	{
		internal const string DefaultPrintService = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export Web Map Task";

		public ExportMapTool()
		{
			InitializeComponent();
			ExportMapCommand = new DelegateCommand(ExportMap, CanExportMap);
			CancelExportMapCommand = new DelegateCommand(CancelExportMap, CanCancelExportMap);
			DataContext = this;
		}

		public Map Map { get; set; }

		#region PrintService: serializable attribute
		private string _printService;

		[DataMember(Name = "printService")] // serializable attribute
		public string PrintService
		{
			get { return _printService; }
			set
			{
				if (_printService != value)
				{
					_printService = value;
					SetPrintTask();
				}
			}
		} 
		#endregion

		#region IsOpen DP

		public bool IsOpen
		{
			get { return (bool) GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}

		public static readonly DependencyProperty IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof (bool), typeof (ExportMapTool), new PropertyMetadata(false));

		#endregion

		#region IsBusy DP

		public bool IsBusy
		{
			get { return (bool) GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		public static readonly DependencyProperty IsBusyProperty =
			DependencyProperty.Register("IsBusy", typeof (bool), typeof (ExportMapTool), new PropertyMetadata(false, OnIsBusyChanged));

		static private void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ExportMapTool) d).OnIsBusyChanged();
		}
		private void OnIsBusyChanged()
		{
			((DelegateCommand)ExportMapCommand).RaiseCanExecuteChanged();
			((DelegateCommand)CancelExportMapCommand).RaiseCanExecuteChanged();
		}
		#endregion

		public string LayoutTemplate
		{
			get { return (string)GetValue(LayoutTemplateProperty); }
			set { SetValue(LayoutTemplateProperty, value); }
		}

		public static readonly DependencyProperty LayoutTemplateProperty =
			DependencyProperty.Register("LayoutTemplate", typeof(string), typeof(ExportMapTool), null);


		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		public static readonly DependencyProperty FormatProperty =
			DependencyProperty.Register("Format", typeof(string), typeof(ExportMapTool), null);


		public IEnumerable<string> LayoutTemplates
		{
			get { return (IEnumerable<string>)GetValue(LayoutTemplatesProperty); }
			set { SetValue(LayoutTemplatesProperty, value); }
		}

		public static readonly DependencyProperty LayoutTemplatesProperty =
			DependencyProperty.Register("LayoutTemplates", typeof(IEnumerable<string>), typeof(ExportMapTool), null);


		public IEnumerable<string> Formats
		{
			get { return (IEnumerable<string>)GetValue(FormatsProperty); }
			set { SetValue(FormatsProperty, value); }
		}

		public static readonly DependencyProperty FormatsProperty =
			DependencyProperty.Register("Formats", typeof(IEnumerable<string>), typeof(ExportMapTool), null);


		#region ExportMapCommand
		public ICommand ExportMapCommand { get; private set; }
		private void ExportMap(object parameter)
		{
			if (CanExportMap(parameter))
			{
				var printParameters = new PrintParameters(Map)
				{
					ExportOptions = new ESRI.ArcGIS.Client.Printing.ExportOptions
					{
						Dpi = 96,
						OutputSize = new Size(Map.ActualWidth, Map.ActualHeight)
					},
					LayoutTemplate = LayoutTemplate,
					Format = Format
				};
				_printTask.ExecuteAsync(printParameters);
				IsBusy = true;
			}
		}

		private bool CanExportMap(object parameter)
		{
			return PrintTask != null && !PrintTask.IsBusy;
		}
		#endregion

		#region CancelExportMapCommand
		public ICommand CancelExportMapCommand { get; private set; }
		private void CancelExportMap(object parameter)
		{
			if (PrintTask != null && PrintTask.IsBusy)
			{
				//PrintTask.CancelAsync();
				//IsBusy = false;
				SetPrintTask(); // create new print task to avoid error when the current print task completes. For now there is no way to know that a PrintTask has been cancelled.
			}
			IsOpen = false;
		}

		private bool CanCancelExportMap(object parameter)
		{
			return true; // PrintTask != null && PrintTask.IsBusy;
		}
		#endregion


		#region IMapTool

		/// <summary>
		/// The MapWidget property is set by the MapWidget that hosts the map tools. The application ensures that this property is set when the
		/// map widget containing this map tool is initialized.
		/// </summary>
		public MapWidget MapWidget { get; set; }

		/// <summary>
		/// OnActivated is called when the map tool is added to the toolbar of the map widget in which it is configured to appear. 
		/// Called when the operational view is opened, and also after a custom toolbar is reverted to the configured toolbar,
		/// and during toolbar configuration.
		/// </summary>
		public void OnActivated()
		{
			Map = MapWidget.Map;
			if (PrintTask == null)
				SetPrintTask();
		}

		private void SetPrintTask()
		{
			var printService = PrintService ?? DefaultPrintService;
			PrintTask = new PrintTask(printService) {DisableClientCaching = true};
		}

		/// <summary>
		///  OnDeactivated is called before the map tool is removed from the toolbar. Called when the operational view is closed,
		///  and also before a custom toolbar is installed, and during toolbar configuration.
		/// </summary>
		public void OnDeactivated()
		{
			Map = null;
			PrintTask = null;
		}

		/// <summary>
		///  Determines if a Configure button is shown for the map tool.
		///  Provides an opportunity to gather user-defined settings.
		/// </summary>
		/// <value>True if the Configure button should be shown, otherwise false.</value>
		public bool CanConfigure
		{
			get { return true; }
		}

		/// <summary>
		///  Provides functionality for the map tool to be configured by the end user through a dialog.
		///  Called when the user clicks the Configure button next to the map tool.
		/// </summary>
		/// <param name="owner">The application window which should be the owner of the dialog.</param>
		/// <returns>True if the user clicks ok, otherwise false.</returns>
		public bool Configure(Window owner)
		{
			var dialog = new ExportMapDialog(this) {Owner = owner};
			var result = dialog.ShowDialog();
			return result != null && (bool) result;
		}

		#endregion

		// Private methods/properties
		private void PrintTaskOnGetServiceInfoCompleted(object sender, ServiceInfoEventArgs serviceInfoEventArgs)
		{
			if (sender == PrintTask)
			{
				if (serviceInfoEventArgs.Error != null)
				{
					MessageBox.Show(string.Format("Error while getting service info: {0}", serviceInfoEventArgs.Error));
				}
				else if (serviceInfoEventArgs.ServiceInfo != null)
				{
					LayoutTemplates = serviceInfoEventArgs.ServiceInfo.LayoutTemplates;
					if (LayoutTemplates != null)
						LayoutTemplate = LayoutTemplates.FirstOrDefault();

					Formats = serviceInfoEventArgs.ServiceInfo.Formats;
					if (Formats != null)
						Format = Formats.FirstOrDefault();
				}
				IsBusy = false;
			}
		}

		private void PrintTaskOnPrintCompleted(object sender, PrintEventArgs e)
		{
			if (sender == PrintTask)
			{
				if (e.Error != null)
				{
					var message = string.Format("Error while exporting map: {0}", e.Error);
					if (e.Error is ServiceException && ((ServiceException) e.Error).Code == 400)
						message += "\n\nNote: You may have to change the print service task by configuring the export map tool.";
					MessageBox.Show(message);
				}
				else
				{
					var url = e.PrintResult.Url;
					var mapName = MapWidget.Caption;
					var dialog = new ExportMapResult(url, mapName);
					dialog.ShowDialog();
				}
				IsBusy = false;
			}
		}

		#region private PrintTask
		private PrintTask _printTask;

		private PrintTask PrintTask
		{
			get { return _printTask; }
			set
			{
				if (_printTask != value)
				{
					if (_printTask != null)
					{
						if (_printTask.IsBusy)
							_printTask.CancelAsync();
						_printTask.ExecuteCompleted -= PrintTaskOnPrintCompleted;
						_printTask.GetServiceInfoCompleted -= PrintTaskOnGetServiceInfoCompleted;
					}
					_printTask = value;
					if (_printTask != null)
					{
						_printTask.ExecuteCompleted += PrintTaskOnPrintCompleted;
						_printTask.GetServiceInfoCompleted += PrintTaskOnGetServiceInfoCompleted;
						_printTask.GetServiceInfoAsync();
						IsBusy = true;
					}
					else
						IsBusy = false;

					LayoutTemplates = null;
					Formats = null;
					LayoutTemplate = null;
					Format = null;
					((DelegateCommand)ExportMapCommand).RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

	}
}
