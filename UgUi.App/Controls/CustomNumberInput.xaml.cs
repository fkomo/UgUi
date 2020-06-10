using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ujeby.UgUi.Controls
{
	public class CustomNumberInputViewModel
	{
		public double Value { get; set; } = 0.0;
		public string Name { get; set; } = "Value";
		public double Min { get; set; } = 0.0;
		public double Max { get; set; } = 1.0;
		public string Format { get; set; } = "F3";
	}

	/// <summary>
	/// Interaction logic for CustomNumberInput.xaml
	/// </summary>
	public partial class CustomNumberInput : UserControl
	{
		// TODO UI INPUT CustomNumberInput - int/double - if min+max is set show background as color progressbar (edit with mouse move)

		public CustomNumberInput()
		{
			InitializeComponent();
		}

		private void MouseChangedValue(Point mousePosition)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				ValueBar.Width = mousePosition.X;
				NumberValue.Text = (ValueBar.Width / this.ActualWidth).ToString("F3", CultureInfo.InvariantCulture);
			}
		}

		private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			MouseChangedValue(e.GetPosition(this));
		}

		private void UserControl_MouseMove(object sender, MouseEventArgs e)
		{
			MouseChangedValue(e.GetPosition(this));
		}
	}
}
