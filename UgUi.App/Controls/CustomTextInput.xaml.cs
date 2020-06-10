using System;
using System.Collections.Generic;
using System.Linq;
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
	public class CustomTextInputViewModel
	{
		public string Value { get; set; }
		public string Name { get; set; } = "Value";
	}

	/// <summary>
	/// Interaction logic for CustomTextInput.xaml
	/// </summary>
	public partial class CustomTextInput : UserControl
	{
		// TODO UI INPUT CustomTextInput - show label/tooltip when no input is entered - possible button (file/folder dialog) ?

		public CustomTextInput()
		{
			InitializeComponent();
		}
	}
}
