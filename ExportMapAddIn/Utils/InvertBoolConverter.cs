﻿using System;
using System.Windows.Data;

namespace ExportMapAddIn
{
	/// <summary>
	/// Inverts a bool value.
	/// </summary>
	public class InvertBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var b = (bool)value;
			return !b;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var b = (bool)value;
			return !b;
		}
	}
}
