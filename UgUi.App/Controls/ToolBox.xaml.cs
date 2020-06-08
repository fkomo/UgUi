using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ujeby.Common.Tools;

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

			UpdateElementList(Workspace.ToolBoxStorage.Keys
				.Select(k => k)
				.ToArray());
		}

		private void UpdateElementList(string[] elements)
		{
			if (ElementList?.Items == null)
				return;

			var lastGroup = null as string;

			ElementList?.Items?.Clear();
			foreach (var element in elements)
			{
				var groupName = element.Split('.').First();
				if (lastGroup != null && groupName != lastGroup)
					ElementList.Items.Add(new Separator());

				ElementList.Items.Add(element);

				lastGroup = groupName;
			}
		}

		private Func<string, Point, Node> CreateTool;

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
			Search.Text = null;
			this.Visibility = Visibility.Visible;
		}

		internal void Hide()
		{
			Search.Text = null;
			this.Visibility = Visibility.Collapsed;
		}

		private void Search_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateElementList(Workspace.ToolBoxStorage.Keys
				.Where(k => string.IsNullOrEmpty(Search.Text) || k.ToLower().Contains(Search.Text.ToLower()))
				.ToArray());
		}

		// TODO TOOLBOX allow up/down cursor in ElementList while changing Search TextBox.Text
		// TODO TOOLBOX show tooltip for Search when no value is entered
	}
}
