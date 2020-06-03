using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ujeby.UgUi.Controls
{
	public enum ContextMenuItemId
	{
		HeaderName = 0,

		Run,
		Save,
		SaveAs,
		Open,
		Import,
		Reset,

		Separator,

		Rename,
		Remove,
		Export,
		Collapse,

		Copy,
		Paste,
	}

	public enum ContextId
	{
		Workspace,
		Node,
		MultipleNodes,
	}

	/// <summary>
	/// Interaction logic for CustomContextMenu.xaml
	/// </summary>
	public partial class CustomContextMenu : UserControl
	{
		public Dictionary<ContextId, KeyValuePair<ContextMenuItemId, string>[]> Context = new Dictionary<ContextId, KeyValuePair<ContextMenuItemId, string>[]>
		{
			{
				ContextId.Workspace, new KeyValuePair<ContextMenuItemId, string>[]
				{
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.HeaderName, "Workspace"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Run, "Run"),
					//new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Separator, null),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Open, "Open Workspace"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Import, "Import Workspace"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Save, "Save Workspace" ),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.SaveAs, "Save Workspace As..." ),
					//new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Separator, null),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Paste, "Paste"),
					//new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Separator, null),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Reset, "Reset"),
				}
			},
			{
				ContextId.Node, new KeyValuePair<ContextMenuItemId, string>[]
				{
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.HeaderName, "Node"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Remove, "Remove"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Rename, "Rename"),
				}
			},
			{
				ContextId.MultipleNodes, new KeyValuePair<ContextMenuItemId, string>[]
				{
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.HeaderName, "Multiple Nodes"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Collapse, "Collapse"),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Export, "Export As..."),
					//new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Separator, null),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Copy, "Copy"),
					//new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Separator, null),
					new KeyValuePair<ContextMenuItemId, string>(ContextMenuItemId.Remove, "Remove"),
				}
			},
		};

		private Action<ContextId, ContextMenuItemId, object> MenuItemClicked;

		public CustomContextMenu(Action<ContextId, ContextMenuItemId, object> menuItemClicked)
		{
			InitializeComponent();

			MenuItemClicked = menuItemClicked;
		}

		private void ChangeMenu(KeyValuePair<ContextMenuItemId, string>[] items)
		{
			MenuList.Items.Clear();
			foreach (var item in items)
			{
				if (item.Key == ContextMenuItemId.HeaderName)
					Header.Content = item.Value;

				else if (item.Key == ContextMenuItemId.Separator)
					MenuList.Items.Add(new Separator());
				else
					MenuList.Items.Add(item);
			}

			MenuList.DisplayMemberPath = "Value";
		}

		private ContextId CurrentContextId;
		private object CurrentContextData;

		private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListViewItem;
			if (item != null)
			{
				MenuItemClicked(CurrentContextId, ((KeyValuePair<ContextMenuItemId, string>)item.Content).Key, CurrentContextData);
				Hide();
			}
		}

		internal void Show(ContextId id, FrameworkElement[] contextData, string headerNamePrefix = null)
		{
			ChangeMenu(Context[id]);

			if (headerNamePrefix != null)
				Header.Content = $"{ headerNamePrefix } { Header.Content as string }";

			CurrentContextId = id;
			CurrentContextData = contextData;

			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			this.Visibility = Visibility.Collapsed;
		}
	}
}
