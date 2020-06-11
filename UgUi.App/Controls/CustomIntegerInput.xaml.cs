using System.Windows.Controls;
using Ujeby.UgUi.Helpers;

namespace Ujeby.UgUi.Controls
{
	public class CustomIntegerInputViewModel : ObservableObject
	{
		private int value;
		public int Value
		{
			get { return value; }
			set
			{
				this.value = value;
				OnPropertyChanged();
			}
		}

		private string name;
		public string Name
		{
			get { return name; }
			set
			{
				this.name = value;
				OnPropertyChanged();
			}
		}
	}

	/// <summary>
	/// Interaction logic for CustomIntegerInput.xaml
	/// </summary>
	public partial class CustomIntegerInput : UserControl
	{
		public CustomIntegerInputViewModel ViewModel { get { return DataContext as CustomIntegerInputViewModel; } }

		public CustomIntegerInput()
		{
			InitializeComponent();

			this.DataContext = new CustomIntegerInputViewModel()
			{
				Value = 123,
				Name = "Integer"
			};
		}
	}
}
