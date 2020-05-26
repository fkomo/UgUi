using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ujeby.UgUi.Controls
{
	/// <summary>
	/// Interaction logic for ElementLookUp2.xaml
	/// </summary>
	public partial class ToolBox : UserControl
	{
		public ToolBox(Func<string, Point, Node> createTool)
		{
			InitializeComponent();

			CreateTool = createTool;

			Elements = Workspace.ToolBoxStorage.Keys
				.Select(k => k)
				.ToArray();

			var lastGroup = null as string;
			foreach (var element in Elements)
			{
				var groupName = element.Split('.').First();
				if (lastGroup != null && groupName != lastGroup)
					ElementList.Items.Add(new Separator());
	
				ElementList.Items.Add(element);

				lastGroup = groupName;
			}
		}

		private Func<string, Point, Node> CreateTool;
		private string[] Elements { get; set; }

		private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListViewItem;
			if (item != null)
			{
				// create tool
				CreateTool(item.Content as string, this.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)));

				Hide();
			}
		}

		internal void Show()
		{
			this.Visibility = Visibility.Visible;
		}

		internal void Hide()
		{
			this.Visibility = Visibility.Collapsed;
		}
	}
}
