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

		SaveWorkspaceAs,
		OpenWorkspace,
		ImportWorkspace,
		Clear,

		Separator,

		Rename,
		Remove,
		SaveSelectionAs,
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
		public Dictionary<ContextId, Dictionary<ContextMenuItemId, string>> Context = new Dictionary<ContextId, Dictionary<ContextMenuItemId, string>>
		{
			{
				ContextId.Workspace, new Dictionary<ContextMenuItemId, string>
				{
					{ ContextMenuItemId.HeaderName, "Workspace" },
					{ ContextMenuItemId.SaveWorkspaceAs, "Save Workspace As..." },
					{ ContextMenuItemId.OpenWorkspace, "Open Workspace" },
					{ ContextMenuItemId.ImportWorkspace, "Import Workspace" },
					{ ContextMenuItemId.Separator, null },
					{ ContextMenuItemId.Clear, "Clear" },
				}
			},
			{
				ContextId.Node, new Dictionary<ContextMenuItemId, string>
				{
					{ ContextMenuItemId.HeaderName, "Node" },
					{ ContextMenuItemId.Remove, "Remove" },
					{ ContextMenuItemId.Rename, "Rename" },
				}
			},
			{
				ContextId.MultipleNodes, new Dictionary<ContextMenuItemId, string>
				{
					{ ContextMenuItemId.HeaderName, "Multiple Nodes" },
					{ ContextMenuItemId.Remove, "Remove" },
					{ ContextMenuItemId.Separator, null },
					{ ContextMenuItemId.SaveSelectionAs, "Save Selection As..." },
				}
			},
		};

		private Action<ContextId, ContextMenuItemId, object> MenuItemClicked;

		public CustomContextMenu(Action<ContextId, ContextMenuItemId, object> menuItemClicked)
		{
			InitializeComponent();

			MenuItemClicked = menuItemClicked;
		}

		private void ChangeMenu(Dictionary<ContextMenuItemId, string> items)
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
