using System.Windows;
using System.Windows.Input;

namespace ExportMapAddIn
{
	/// <summary>
	/// Interaction logic for ExportMapDialog.xaml
	/// </summary>
	internal partial class ExportMapDialog : Window
	{
		private readonly ExportMapTool _exportMapTool;

		public ExportMapDialog(ExportMapTool exportMapTool)
		{
			InitializeComponent();
			_exportMapTool = exportMapTool;
			PrintService = exportMapTool.PrintService ?? ExportMapTool.DefaultPrintService;
			OkCommand = new DelegateCommand(Ok);
			DataContext = this;
		}

		#region PrintService DP
		public string PrintService
		{
			get { return (string)GetValue(PrintServiceProperty); }
			set { SetValue(PrintServiceProperty, value); }
		}

		public static readonly DependencyProperty PrintServiceProperty =
			DependencyProperty.Register("PrintService", typeof(string), typeof(ExportMapDialog), null); 
		#endregion

		#region OkCommand
		public ICommand OkCommand { get; private set; }

		private void Ok(object parameter)
		{
			_exportMapTool.PrintService = PrintService;
			DialogResult = true;
			Close();
		}

		#endregion
	}
}
