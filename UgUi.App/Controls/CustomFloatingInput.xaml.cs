using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ujeby.UgUi.Helpers;

namespace Ujeby.UgUi.Controls
{
	public class CustomFloatingInputViewModel : ObservableObject
	{
		private double value;
		public double Value
		{
			get { return value; }
			set
			{
				this.value = Math.Max(Min, Math.Min(Max, value));
								
				OnPropertyChanged();
				OnPropertyChanged(nameof(ValueBarWidth));
			}
		}

		public double ValueBarMaxWidth { get; set; }
		public double ValueBarWidth { get { return ValueBarMaxWidth * ((Value - Min) / (Max - Min)); } }

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

		public double Min { get; set; }
		public double Max { get; set; }
	}

	/// <summary>
	/// Interaction logic for CustomFloatingInput.xaml
	/// </summary>
	public partial class CustomFloatingInput : UserControl
	{
		public CustomFloatingInputViewModel ViewModel { get { return DataContext as CustomFloatingInputViewModel; } }

		public CustomFloatingInput()
		{
			InitializeComponent();

			this.DataContext = new CustomFloatingInputViewModel()
			{
				ValueBarMaxWidth = this.Width - (MainBorder.BorderThickness.Left + MainBorder.BorderThickness.Right),
				Min = 0.0,
				Max = 1.0,
				Name = "Floating",
				Value = 0.5,
			};
		}

		/// <summary>
		/// number of pixes on each side of input, which represent Min / Max value
		/// </summary>
		public double DeadZone { get; set; } = 10;

		private void MouseChangedValue(Point mousePosition)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				if (mousePosition.X > (this.ActualWidth - DeadZone))
					ViewModel.Value = ViewModel.Max;
				else if (mousePosition.X < DeadZone)
					ViewModel.Value = ViewModel.Min;
				else
					ViewModel.Value = ViewModel.Min + ((mousePosition.X - DeadZone) / ( this.ActualWidth - (2 * DeadZone))) * (ViewModel.Max - ViewModel.Min);
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
