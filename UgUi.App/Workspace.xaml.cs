using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Controls;
using System.Linq;
using System.Windows.Threading;
using System.ComponentModel;
using Ujeby.UgUi.Core;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using Ujeby.UgUi.Nodes;
using System.Windows.Media.Animation;

namespace Ujeby.UgUi
{
	/// <summary>
	/// Interaction logic for Workspace.xaml
	/// </summary>
	public partial class Workspace : Window
	{
		// TODO WORKSPACE undo / redo action
		// TODO WORKSPACE add multiple nodes to group / package

		/// <summary>
		/// connection that is currently created
		/// </summary>
		public static Connection NewConnection { get; set; } = null;

		/// <summary>
		/// node that is currently moved with mouse
		/// </summary>

		private Point GridOffset = new Point(0, 0);
		private Point? WorkspaceDragStart = null;

		private const int GridStep = 20;
		private const int GridMajorStep = 100;
		private readonly SolidColorBrush GridMinorBrush = new SolidColorBrush(Color.FromRgb(0x35, 0x35, 0x35));
		private readonly SolidColorBrush GridMajorBrush = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x3a));

		private List<Line> VerticalLines = new List<Line>();
		private List<Line> HorizontalLines = new List<Line>();

		private static string WorkspaceFile { get; set; } = null;

		private const double ScaleStep = 1.1;
		private const double MinScale = 0.5;
		private const double MaxScale = 1.0;
		public double Scale { get; private set; } = 1.0;

		private bool MessagesBoxCollapsed { get; set; } = false;
		private Size? MessagesBoxBeforeCollapse { get; set; } = null;

		private bool MessagesBoxResizing { get; set; } = false;
		private Point MessagesBoxResizingLast { get; set; }

		private static string UserDataFolder
		{
			get
			{
				var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				var userDataFolder = System.IO.Path.Combine(roaming, "Ujeby", "UgUi" + (Debugger.IsAttached ? "-debug" : null));

				if (!System.IO.Directory.Exists(userDataFolder))
					System.IO.Directory.CreateDirectory(userDataFolder);

				return userDataFolder;
			}
		}

		private static List<KeyValuePair<Point, string>> NodeClipboard { get; set; } = new List<KeyValuePair<Point, string>>();

		/// <summary>
		/// dictionary of all possible nodes
		/// </summary>
		public static SortedDictionary<string, Type> ToolBoxStorage { get; set; } = new SortedDictionary<string, Type>();

		/// <summary>
		/// all nodes
		/// </summary>
		public static List<Node> Nodes { get; internal set; } = new List<Node>();

		/// <summary>
		/// all connections between nodes
		/// </summary>
		public static List<Connection> Connections { get; set; } = new List<Connection>();

		public static ToolBox ToolBox { get; set; }
		public static CustomContextMenu WorkspaceContextMenu { get; set; }

		public Workspace()
		{
			// add all types from Ujeby.UgUi.Nodes with NodeInfo attribute to ToolBoxStorage
			ToolBoxStorage.Clear();
			foreach (var nodeType in Assembly.LoadFrom("Ujeby.UgUi.Nodes.dll").GetTypes()
				.Where(t => t.Namespace.StartsWith("Ujeby.UgUi.Nodes.") && t.CustomAttributes.Any(a => a.AttributeType == typeof(NodeInfoAttribute)))
				.Where(t => !t.CustomAttributes.Single(a => a.AttributeType == typeof(NodeInfoAttribute)).NamedArguments.Any(na => na.MemberName == nameof(NodeInfoAttribute.Abstract))))
			{
				var groupName = nodeType.Namespace.Substring(nodeType.Namespace.LastIndexOf(".") + 1);
				var elementName = AttributeHelper.GetValue<NodeInfoAttribute, string>(nodeType, nameof(NodeInfoAttribute.DisplayName)) ?? nodeType.Name;
				ToolBoxStorage.Add($"{ groupName }.{ elementName }", nodeType);
			}

			InitializeComponent();

			Log.LogFileName = "ujeby-gui.log";
			Log.LogFolder = UserDataFolder;
			Log.LogLineAdded = WriteLineToOutput;
		}

		public static void HideCustomWindows()
		{
			WorkspaceContextMenu?.Hide();
			ToolBox?.Hide();
		}

		public static bool CheckConnectionInProgress(Node right, string rightAnchorName)
		{
			// connection to itself
			if (NewConnection.Left == right)
				return false;

			// input anchor cant have multiple connections
			if (Connections.Any(c => c.Right == right && c.RightAnchorName == rightAnchorName))
				return false;

			// size type check
			//if (ConnectionInProgress.LeftAnchorName.StartsWith($"{ OutputAnchorPrefix }Size") && !rightAnchorName.StartsWith($"{ InputAnchorPrefix }Size"))
			//	return false;

			// array type check
			//if (ConnectionInProgress.LeftAnchorName.StartsWith($"{ OutputAnchorPrefix }Array") && !rightAnchorName.StartsWith($"{ InputAnchorPrefix }Array"))
			//	return false;

			return true;
		}

		internal bool Save(string currentWorkspaceFile, FrameworkElement[] nodesToSave, out string newFile)
		{
			newFile = null;

			var nodes = new List<UgUiFile.Node>();
			foreach (Node toSave in nodesToSave)
				nodes.Add(new UgUiFile.Node
				{
					Id = toSave.Id,
					TypeName = toSave.NodeInstance.GetType().FullName,
					Position = toSave.WorkspacePosition,
					Data = (toSave.NodeInstance as ISerializableNode)?.SerializeData(),
					Name = toSave.CustomNodeName.Text,
				});

			var connections = new List<UgUiFile.Connection>();
			foreach (var connection in Connections)
			{
				if (nodes.Any(o => o.Id == connection.Left.Id) && nodes.Any(o => o.Id == connection.Right.Id))
				{
					connections.Add(new UgUiFile.Connection
					{
						LeftId = connection.Left.Id,
						RightId = connection.Right.Id,
						LeftAnchorName = connection.LeftAnchorName,
						RightAnchorName = connection.RightAnchorName,
					});
				}
			}

			try
			{
				if (string.IsNullOrEmpty(currentWorkspaceFile))
				{
					var dlg = new SaveFileDialog()
					{
						AddExtension = true,
						DefaultExt = UgUiFile.Extension,
						Filter = $"UgUi workspaces ({ UgUiFile.Extension })|*{ UgUiFile.Extension }",
						InitialDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
					};

					if (dlg.ShowDialog() == true)
						currentWorkspaceFile = dlg.FileName;
				}

				if (!string.IsNullOrEmpty(currentWorkspaceFile))
				{
					File.WriteAllText(currentWorkspaceFile, Utils.Serialize(new UgUiFile
					{
						Connections = connections.ToArray(),
						Nodes = nodes.ToArray(),
					}), System.Text.Encoding.UTF8);
					Log.WriteLine($"Workspace '{ currentWorkspaceFile }' saved successfully");

					newFile = currentWorkspaceFile;

					return true;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}

			return false;
		}

		internal string Open(string currentWorkspaceFile, Point position)
		{
			try
			{
				var dlg = new OpenFileDialog()
				{
					DefaultExt = UgUiFile.Extension,
					Filter = $"UgUi workspaces ({ UgUiFile.Extension })|*{ UgUiFile.Extension }",
					InitialDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
				};
				if (dlg.ShowDialog() == true)
				{
					var file = Utils.Deserialize<UgUiFile>(File.ReadAllText(dlg.FileName, System.Text.Encoding.UTF8));
					if (file == null || file.Nodes == null || file.Nodes.Length < 1)
					{
						Log.WriteLine($"Workspace '{ dlg.FileName }' is empty");
						return currentWorkspaceFile;
					}

					SelectAllNodes(false);

					// fix identifiers - if id is found in workspace generate new one
					foreach (var node in file.Nodes)
					{
						if (Nodes.Any(n => n.Id == node.Id))
						{
							var oldId = node.Id;
							node.Id = Guid.NewGuid();

							foreach (var connection in file.Connections)
							{
								if (connection.LeftId == oldId)
									connection.LeftId = node.Id;

								if (connection.RightId == oldId)
									connection.RightId = node.Id;
							}
						}
					}

					// center of all nodes
					var center = new Point(file.Nodes.Average(o => o.Position.X), file.Nodes.Average(o => o.Position.Y));

					// add nodes
					foreach (var node in file.Nodes)
					{
						var newNode = AddNodeToWorkspace(node.TypeName, new Point(node.Position.X - center.X + position.X, node.Position.Y - center.Y + position.Y));
						if (newNode == null)
						{
							Log.WriteLine($"Unknown node '{ node.TypeName }'");
							continue;
						}

						newNode.Id = node.Id;
						newNode.SetCustomName(node.Name);

						if (!string.IsNullOrEmpty(node.Data))
							(newNode.NodeInstance as ISerializableNode).DeserializeData(node.Data);

						// select new node
						newNode.Select(true);
					}

					// add connections
					foreach (var connection in file.Connections)
					{
						var leftControl = Nodes.SingleOrDefault(c => c.Id == connection.LeftId);
						var rightControl = Nodes.SingleOrDefault(c => c.Id == connection.RightId);

						if (leftControl != null && rightControl != null)
						{
							var leftNode = file.Nodes.Single(o => o.Id == connection.LeftId);
							var leftAnchorRelativePosition = leftControl.GetRelativeAnchorPosition(connection.LeftAnchorName);
							var fromPosition = new Point(leftNode.Position.X - center.X + position.X + leftAnchorRelativePosition.X, leftNode.Position.Y - center.Y + position.Y + leftAnchorRelativePosition.Y);

							var rightNode = file.Nodes.Single(o => o.Id == connection.RightId);
							var rightAnchorRelativePosition = rightControl.GetRelativeAnchorPosition(connection.RightAnchorName);
							var toPosition = new Point(rightNode.Position.X - center.X + position.X + rightAnchorRelativePosition.X, rightNode.Position.Y - center.Y + position.Y + rightAnchorRelativePosition.Y);

							var newConnection = new Connection(leftControl, rightControl, connection.LeftAnchorName, connection.RightAnchorName);
							newConnection.Update(fromPosition, toPosition);

							AddConnection(newConnection, true);

							newConnection.Right.ResetInputAnchorColor(newConnection.RightAnchorName);
						}
					}

					Log.WriteLine($"Workspace '{ dlg.FileName }' loaded [{ file.Nodes.Length } nodes, { file.Connections.Length } connections]");

					// TODO CORE new simulation after imported / opened new workspace ?

					return dlg.FileName;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}

			return currentWorkspaceFile;
		}

		private void SelectAllNodes(bool select = true)
		{
			foreach (var node in Nodes)
				node.Select(select);
		}

		private bool SaveCurrent()
		{
			if (Nodes.Count < 1)
				return true;

			var result = MessageBox.Show($"Do you want to save changes { (WorkspaceFile != null ? $"to '{ WorkspaceFile }' " : "") }?", "Ujeby.gUi", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				if (!Save(WorkspaceFile, Nodes.Select(c => c as FrameworkElement).ToArray(), out string newFile))
					return false;
			}
			else if (result == MessageBoxResult.Cancel)
				return false;

			return true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				// collapse messages box
				ToggleMessagesBoxCollapse();

				DrawGrid();

				// create and hide toolbox
				ToolBox = new ToolBox(AddNodeToWorkspace)
				{
					Visibility = Visibility.Collapsed
				};
				WorkspaceCanvas.Children.Add(ToolBox);
				Canvas.SetZIndex(ToolBox, int.MaxValue);

				// create and context menu
				WorkspaceContextMenu = new CustomContextMenu(ContextMenuItemClicked)
				{
					Visibility = Visibility.Collapsed
				};
				WorkspaceCanvas.Children.Add(WorkspaceContextMenu);
				Canvas.SetZIndex(WorkspaceContextMenu, int.MaxValue);

				SetTitle();

				//MessagesBoxBorder.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			try
			{
				DrawGrid();
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			// TODO WORKSPACE save before closing
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				// run
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.R)
					NewSimulation();

				// select all
				else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.A)
				{
					foreach (var element in WorkspaceCanvas.Children)
						(element as Node)?.Select(true);
				}

				// toggle collapse selected
				else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.T)
				{
					foreach (var element in WorkspaceCanvas.Children)
						if ((element as Node)?.Selected == true)
							(element as Node)?.ToggleCollapse();
				}

				else if (e.Key == Key.Escape)
				{
					HideCustomWindows();
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		/// <summary>
		/// draw background workspace grid (minor / major lines)
		/// </summary>
		private void DrawGrid()
		{
			// remove old lines
			foreach (var line in HorizontalLines)
				WorkspaceCanvas.Children.Remove(line);
			foreach (var line in VerticalLines)
				WorkspaceCanvas.Children.Remove(line);
			VerticalLines.Clear();
			HorizontalLines.Clear();

			// TODO WORKSPACE scale this is not accurate enough
			var step = (GridStep * Scale);

			for (var x = 0.0; x < ActualWidth; x += step)
			{
				var xFinal = x + (int)GridOffset.X % step;
				var major = (xFinal - (int)GridOffset.X) % GridMajorStep == 0;

				var line = new Line
				{
					Stroke = major ? GridMajorBrush : GridMinorBrush,
					StrokeThickness = 1,
					X1 = xFinal,
					Y1 = 0,
					X2 = xFinal,
					Y2 = ActualHeight,
					SnapsToDevicePixels = true,
				};

				line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
				Canvas.SetZIndex(line, major ? int.MinValue + 1 : int.MinValue);
				WorkspaceCanvas.Children.Add(line);
				VerticalLines.Add(line);
			}

			for (var y = 0.0; y < ActualHeight; y += step)
			{
				var yFinal = y + (int)GridOffset.Y % step;
				var major = (yFinal - (int)GridOffset.Y) % GridMajorStep == 0;

				var line = new Line
				{
					Stroke = major ? GridMajorBrush : GridMinorBrush,
					StrokeThickness = 1,
					X1 = 0,
					Y1 = yFinal,
					X2 = ActualWidth,
					Y2 = yFinal,
					SnapsToDevicePixels = true,
				};

				line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
				Canvas.SetZIndex(line, major ? int.MinValue + 1 : int.MinValue);
				WorkspaceCanvas.Children.Add(line);
				HorizontalLines.Add(line);
			}
		}

		private void WriteLineToOutput(string line)
		{
			MessagesBox.Text += line + Environment.NewLine;
		}

		private void ContextMenuItemClicked(ContextId contextId, ContextMenuItemId menuItemId, object contextData)
		{
			try
			{
				if (menuItemId == ContextMenuItemId.Run)
				{
					NewSimulation();
				}
				else if (menuItemId == ContextMenuItemId.Remove)
				{
					foreach (var control in contextData as FrameworkElement[])
						RemoveNodeFromWorkspace(control as Node);

					// TODO CORE new simulation after removed node
					//NewSimulation();
				}
				else if (menuItemId == ContextMenuItemId.Reset)
				{
					if (SaveCurrent())
						Reset();
				}
				else if (menuItemId == ContextMenuItemId.Collapse)
				{
					foreach (var node in (contextData as FrameworkElement[]).Select(n => n as Node))
						node.ToggleCollapse();
				}
				else if (menuItemId == ContextMenuItemId.Save)
				{
					if (Save(WorkspaceFile, Nodes.Select(c => c as FrameworkElement).ToArray(), out string newFile))
						SetTitle(newFile);
				}
				else if (menuItemId == ContextMenuItemId.SaveAs)
				{
					if (Save(null, Nodes.Select(c => c as FrameworkElement).ToArray(), out string newFile))
						SetTitle(newFile);
				}
				else if (menuItemId == ContextMenuItemId.Export)
				{
					Save(null, contextData as FrameworkElement[], out string newFile);
				}
				else if (menuItemId == ContextMenuItemId.Import)
				{
					Open(null, new Point(WorkspaceCanvas.ActualWidth / 2, WorkspaceCanvas.ActualHeight / 2));
				}
				else if (menuItemId == ContextMenuItemId.Open)
				{
					if (SaveCurrent())
					{
						RemoveAllNodesFromWorkspace();
						SetTitle();

						var fileName = Open(WorkspaceFile, new Point(WorkspaceCanvas.ActualWidth / 2, WorkspaceCanvas.ActualHeight / 2));
						SetTitle(fileName);
					}
				}
				else if (menuItemId == ContextMenuItemId.Rename)
				{
					var nodeToRename = (contextData as FrameworkElement[]).Single() as Node;
					nodeToRename.RenameBegin();
				}
				else if (menuItemId == ContextMenuItemId.Copy)
				{
					var toCopy = (contextData as FrameworkElement[]).Select(e => e as Node);

					var topLeft = new Point(toCopy.Min(n => n.TranslatePoint(new Point(), WorkspaceCanvas).X), toCopy.Min(n => n.TranslatePoint(new Point(), WorkspaceCanvas).Y));

					NodeClipboard.Clear();
					foreach (var oldNode in toCopy)
					{
						var position = oldNode.TranslatePoint(new Point(), WorkspaceCanvas);
						NodeClipboard.Add(new KeyValuePair<Point, string>(new Point(position.X - topLeft.X, position.Y - topLeft.Y), oldNode.NodeInstance.GetType().FullName));

						// TODO WORKSPACE copy connections to clipboard
					}
				}
				else if (menuItemId == ContextMenuItemId.Paste)
				{
					if (NodeClipboard.Count > 0)
					{
						SelectAllNodes(false);

						var mousePosition = Mouse.GetPosition(WorkspaceCanvas);
						foreach (var node in NodeClipboard)
						{
							var newNode = AddNodeToWorkspace(node.Value, new Point(mousePosition.X + node.Key.X, mousePosition.Y + node.Key.Y));
							newNode.Select(true);
						}

						// TODO WORKSPACE paste connections from clipboard

						UpdateScale();
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Reset()
		{
			// remove controls
			RemoveAllNodesFromWorkspace();

			// reset workspace name
			SetTitle();

			// clear clipboard
			//NodeClipboard.Clear();

			// reset grid
			Scale = 1.0;
			GridOffset = new Point(0, 0);
			DrawGrid();

			// clear messages
			MessagesBox.Text = string.Empty;
		}

		private void SetTitle(string workspaceFile = null)
		{
			WorkspaceFile = workspaceFile;

			this.Title = "Ujeby.gUi";
			if (!string.IsNullOrEmpty(workspaceFile))
				this.Title += $" [{ new FileInfo(workspaceFile).Name }]";
		}

		private void Workspace_MouseDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				HideCustomWindows();

				var mousePosition = e.GetPosition(WorkspaceCanvas);

				if (e.RightButton == MouseButtonState.Pressed)
					ShowContextMenu(mousePosition);

				else if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (Mouse.DirectlyOver == WorkspaceCanvas && e.ClickCount == 2)
						ShowToolbox(mousePosition);

					else
					{
						if (Mouse.DirectlyOver == WorkspaceCanvas)
							StartSelection(mousePosition);

						else
						{
							var anchor = Mouse.DirectlyOver as FrameworkElement;
							var elementControl = GetNodeFromAnchor(anchor);

							if (anchor.Name != null && anchor.Name.StartsWith(Node.OutputAnchorPrefix))
							{
								if (e.ClickCount == 2)
									CreateDefaultNode(elementControl as Node, anchor.Name);

								else
									StarDrawingNewConnection(mousePosition, elementControl as Node, anchor.Name);
							}
							else if (anchor.Name != null && anchor.Name.StartsWith(Node.InputAnchorPrefix) && Connections.Any(c => c.RightAnchorName == anchor.Name))
								StartMovingConnection(elementControl as Node, anchor.Name);

							else
								StartMovingNode(mousePosition, Mouse.DirectlyOver as FrameworkElement);
						}
					}
				}
				else if (e.MiddleButton == MouseButtonState.Pressed)
				{
					if (Mouse.DirectlyOver == WorkspaceCanvas)
					{
						// move whole canvas
						WorkspaceDragStart = mousePosition;
						Cursor = Cursors.SizeAll;

						// TODO WORKSPACE show small workspace map in corner while scaling
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Workspace_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				var mousePosition = e.GetPosition(WorkspaceCanvas);

				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (SelectionRectangle != null)
						UpdateSelection(mousePosition);

					else if (NewConnection != null)
						DrawNewConnection(mousePosition);

					else if (NodeMoving != null)
						UpdateMovingNode(mousePosition);
				}
				else if (e.MiddleButton == MouseButtonState.Pressed)
				{
					if (WorkspaceDragStart.HasValue)
					{
						// move whole canvas

						var offset = mousePosition - WorkspaceDragStart.Value;

						GridOffset.X += offset.X;
						GridOffset.Y += offset.Y;

						MoveWorkspace(offset);
						WorkspaceDragStart = mousePosition;
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Workspace_MouseUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					if (SelectionRectangle != null)
						StopSelection();

					else if (NewConnection != null)
						StopDrawingNewConnection();

					else if (NodeMoving != null)
						StopMovingNode();
				}

				if (e.MiddleButton == MouseButtonState.Released)
				{
					// stop moving whole canvas
					WorkspaceDragStart = null;
					Cursor = Cursors.Arrow;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void Workspace_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			try
			{
				if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					return;

				if (e.Delta > 0)
					Scale *= ScaleStep;
				else
					Scale /= ScaleStep;
				Scale = Math.Max(Math.Min(Scale, MaxScale), MinScale);

				UpdateScale();
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		/// <summary>
		/// show toolbox at given position
		/// </summary>
		/// <param name="mousePosition"></param>
		private void ShowToolbox(Point mousePosition)
		{
			Canvas.SetLeft(ToolBox, mousePosition.X);
			Canvas.SetTop(ToolBox, mousePosition.Y);

			ToolBox.Show();
		}

		/// <summary>
		/// show context menu at given position
		/// </summary>
		/// <param name="mousePosition"></param>
		private void ShowContextMenu(Point mousePosition)
		{
			var control = Mouse.DirectlyOver as FrameworkElement;
			if (control != WorkspaceCanvas)
			{
				while (control != null && control as Node == null)
					control = control.Parent as FrameworkElement;
			}

			if (control != null)
			{
				// get context
				var headerName = null as string;
				var nodes = Nodes.Where(c => c.Selected).Select(c => c as FrameworkElement).ToArray();
				var contextId = control == WorkspaceCanvas ? ContextId.Workspace : ContextId.Node;
				if (nodes.Length > 0)
					contextId = ContextId.MultipleNodes;
				else
				{
					nodes = new FrameworkElement[] { control };
					headerName = (control as Node)?.CustomName;
				}

				Canvas.SetLeft(WorkspaceContextMenu, mousePosition.X);
				Canvas.SetTop(WorkspaceContextMenu, mousePosition.Y);
				WorkspaceContextMenu.Show(contextId, nodes, headerName);
			}
		}

		private void CreateDefaultNode(Node node, string outputAnchorName)
		{
			// TODO WORKSPACE create default node based on output anchor property type (string / double / ...)

			Log.WriteLine($"CreateDefaultNode({ node.NodeInstance.GetType().Name }, { outputAnchorName })");
		}

		private void UpdateScale()
		{
			var scaleOrigin = new Point(WorkspaceCanvas.ActualWidth * 0.5 + GridOffset.X, WorkspaceCanvas.ActualHeight * 0.5 + GridOffset.Y);

			foreach (var node in Nodes)
				node.RenderTransform = new ScaleTransform(Scale, Scale, scaleOrigin.X - Canvas.GetLeft(node), scaleOrigin.Y - Canvas.GetTop(node));

			foreach (var node in Nodes)
				MoveNode(node, new Vector());

			DrawGrid();
		}

		private void Workspace_MouseLeave(object sender, MouseEventArgs e)
		{
			try
			{
				if (SelectionRectangle != null)
					StopSelection(true);

				if (NewConnection != null)
				{
					NewConnection.RemoveFromUICollection(WorkspaceCanvas.Children);
					NewConnection = null;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		#region node moving

		private Node NodeMoving { get; set; } = null;
		private Point NodeMovingStart { get; set; }

		/// <summary>
		/// start moving/daging node
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="node"></param>
		private void StartMovingNode(Point mousePosition, FrameworkElement nodeElement)
		{
			if (nodeElement?.Name == Node.HeaderElementName || nodeElement?.Name == Node.HeaderTitleElementName)
			{
				// start moving node
				while (nodeElement != null && nodeElement as Node == null)
					nodeElement = nodeElement.Parent as FrameworkElement;
				var node = nodeElement as Node;

				NodeMovingStart = mousePosition;
				NodeMoving = node;

				Cursor = Cursors.SizeAll;
			}
		}

		/// <summary>
		/// update moving node/nodes position
		/// </summary>
		private void UpdateMovingNode(Point mousePosition)
		{
			var delta = mousePosition - NodeMovingStart;

			MoveNode(NodeMoving, delta);

			if (NodeMoving.Selected)
			{
				// move other selected elements
				foreach (var control in WorkspaceCanvas.Children)
				{
					if (control == NodeMoving)
						continue;

					var otherElementControl = control as Node;
					if (otherElementControl != null && otherElementControl.Selected)
						MoveNode(otherElementControl, delta);
				}
			}

			NodeMovingStart = mousePosition;
		}

		/// <summary>
		/// stop moving node
		/// </summary>
		private void StopMovingNode()
		{
			NodeMoving = null;
			Cursor = Cursors.Arrow;
		}

		/// <summary>
		/// move multiple nodes
		/// </summary>
		/// <param name="elementControl"></param>
		/// <param name="delta"></param>
		private void MoveNode(Node elementControl, Vector delta)
		{
			var currentPosition = new Point(Canvas.GetLeft(elementControl), Canvas.GetTop(elementControl));
			Canvas.SetLeft(elementControl, currentPosition.X + delta.X);
			Canvas.SetTop(elementControl, currentPosition.Y + delta.Y);

			var transformedTopLeft = elementControl.TranslatePoint(new Point(), WorkspaceCanvas);

			elementControl.UpdateConnections(new Point(transformedTopLeft.X + delta.X, transformedTopLeft.Y + delta.Y));
		}

		#endregion

		#region new connection

		/// <summary>
		/// add new connection to Connections list
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="addToUI"></param>
		private void AddConnection(Connection connection, bool addToUI)
		{
			Connections.Add(connection);

			if (addToUI)
				connection.AddToUICollection(WorkspaceCanvas.Children);

			connection.Left.AddConnectionTo(connection);

			// TODO CORE new simulation after added connection ?
			connection.Right.AddConnectionFrom(connection, false);
		}

		/// <summary>
		/// start drawing new connection from source node and its output anchor
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="sourceNode"></param>
		/// <param name="anchorName"></param>
		private void StarDrawingNewConnection(Point mousePosition, Node sourceNode, string anchorName)
		{
			NewConnection = new Connection(sourceNode, null, anchorName, null);

			var userControlPosition = sourceNode.TranslatePoint(new Point(), WorkspaceCanvas);
			var anchorPosition = sourceNode.GetRelativeAnchorPosition(anchorName);

			NewConnection.Update(new Point(userControlPosition.X + anchorPosition.X, userControlPosition.Y + anchorPosition.Y), mousePosition);
			NewConnection.AddToUICollection(WorkspaceCanvas.Children);

			Cursor = Cursors.Hand;
		}

		/// <summary>
		/// update new connection
		/// </summary>
		private void DrawNewConnection(Point mousePosition)
		{
			NewConnection.Update(null, mousePosition);
		}

		/// <summary>
		/// start moving existing connection by dragging its ending anchor
		/// </summary>
		/// <param name="node"></param>
		/// <param name="anchorName"></param>
		private void StartMovingConnection(Node node, string anchorName)
		{
			NewConnection = Connections.SingleOrDefault(c => node == c.Right && c.RightAnchorName == anchorName);
			if (NewConnection != null)
			{
				Connections.Remove(NewConnection);

				NewConnection.Left.RemoveConnection(NewConnection);
				NewConnection.Right.RemoveConnection(NewConnection);

				NewConnection.Right = null;
				NewConnection.RightAnchorName = null;

				Cursor = Cursors.Hand;
			}
		}

		/// <summary>
		/// stop drawing new connection
		/// </summary>
		private void StopDrawingNewConnection()
		{
			var anchor = Mouse.DirectlyOver as FrameworkElement;
			var elementControl = GetNodeFromAnchor(anchor);

			if (anchor.Name != null && anchor.Name.StartsWith(Node.InputAnchorPrefix) && elementControl != null && CheckConnectionInProgress(elementControl, anchor.Name))
			{
				// finalize new connection
				NewConnection.Right = elementControl;
				NewConnection.RightAnchorName = anchor.Name;

				var userControlPosition = elementControl.TranslatePoint(new Point(), WorkspaceCanvas);
				var anchorPosition = elementControl.GetRelativeAnchorPosition(anchor.Name);

				NewConnection.Update(null, new Point(userControlPosition.X + anchorPosition.X, userControlPosition.Y + anchorPosition.Y));

				AddConnection(NewConnection, false);
			}
			else
			{
				// remove existing connection
				NewConnection.RemoveFromUICollection(WorkspaceCanvas.Children);

				// TODO CORE new simulation after removed connection ?
				//NewSimulation();
			}

			NewConnection = null;
			Cursor = Cursors.Arrow;
		}

		#endregion

		#region selection rectangle

		private Rectangle SelectionRectangle { get; set; } = null;
		private Point SelectionRectangleStart { get; set; }

		/// <summary>
		/// start drawing selection rectangle
		/// </summary>
		/// <param name="mousePosition"></param>
		private void StartSelection(Point mousePosition)
		{
			SelectionRectangleStart = mousePosition;
			SelectionRectangle = new Rectangle
			{
				Fill = new SolidColorBrush(Color.FromArgb(0x20, 0x4b, 0x81, 0xd8)),
				Stroke = new SolidColorBrush(Color.FromArgb(0x70, 0x4b, 0x81, 0xd8))
			};

			WorkspaceCanvas.Children.Add(SelectionRectangle);
			Canvas.SetLeft(SelectionRectangle, SelectionRectangleStart.X);
			Canvas.SetTop(SelectionRectangle, SelectionRectangleStart.Y);

			// deselect all nodes
			foreach (var element in WorkspaceCanvas.Children)
				(element as Node)?.Select(false);
		}

		/// <summary>
		/// update selection rectangle
		/// </summary>
		/// <param name="mousePosition"></param>
		private void UpdateSelection(Point mousePosition)
		{
			var newWidth = mousePosition.X - SelectionRectangleStart.X;
			var newHeight = mousePosition.Y - SelectionRectangleStart.Y;

			if (newWidth > 0)
				SelectionRectangle.Width = newWidth;
			else if (newWidth < 0)
			{
				SelectionRectangle.Width = -newWidth;
				Canvas.SetLeft(SelectionRectangle, mousePosition.X);
			}

			if (newHeight > 0)
				SelectionRectangle.Height = newHeight;
			else if (newHeight < 0)
			{
				SelectionRectangle.Height = -newHeight;
				Canvas.SetTop(SelectionRectangle, mousePosition.Y);
			}

			var selectionTopLeft = new Point(Canvas.GetLeft(SelectionRectangle), Canvas.GetTop(SelectionRectangle));
			var selectionBottomRight = new Point(selectionTopLeft.X + SelectionRectangle.ActualWidth, selectionTopLeft.Y + SelectionRectangle.ActualHeight);
			foreach (FrameworkElement element in WorkspaceCanvas.Children)
			{
				//var elementTopLeft = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
				var elementTopLeft = element.TranslatePoint(new Point(), WorkspaceCanvas);

				//var elementBottomRight = new Point(elementTopLeft.X + element.ActualWidth, elementTopLeft.Y + element.ActualHeight);
				var elementBottomRight = element.TranslatePoint(new Point(element.ActualWidth * Scale, element.ActualHeight * Scale), WorkspaceCanvas);

				(element as Node)?.Hilight(
					selectionTopLeft.X < elementTopLeft.X && elementBottomRight.X < selectionBottomRight.X &&
					selectionTopLeft.Y < elementTopLeft.Y && elementBottomRight.Y < selectionBottomRight.Y
					);
			}
		}

		/// <summary>
		/// stop drawing selection rectangle
		/// </summary>
		/// <param name="cancelled"></param>
		private void StopSelection(bool cancelled = false)
		{
			foreach (FrameworkElement element in WorkspaceCanvas.Children)
			{
				(element as Node)?.Select((element as Node).Hilighted && !cancelled);
				if (cancelled)
					(element as Node)?.Hilight(false);
			}

			WorkspaceCanvas.Children.Remove(SelectionRectangle);
			SelectionRectangle = null;
		}

		#endregion

		private void MoveWorkspace(Vector offset)
		{
			DrawGrid();

			foreach (var node in Nodes)
				MoveNode(node, offset);
		}

		private Node GetNodeFromAnchor(FrameworkElement anchor)
		{
			if (anchor == null)
				return null;

			if (anchor is Node)
				return anchor as Node;

			while (anchor?.Parent != null)
			{
				if (anchor is Node)
					return anchor as Node;

				anchor = anchor.Parent as FrameworkElement;
			}

			return null;
		}

		public void RemoveAllNodesFromWorkspace()
		{
			var toRemove = Nodes.ToArray();
			foreach (var control in toRemove)
				RemoveNodeFromWorkspace(control);
		}

		public void RemoveNodeFromWorkspace(Node node)
		{
			if (node == null)
				return;

			node.Remove();

			Nodes.Remove(node);
			WorkspaceCanvas.Children.Remove(node);
		}

		private Node AddNodeToWorkspace(string nodeType, Point position)
		{
			Type type = null;
			if (ToolBoxStorage.ContainsKey(nodeType))
				type = ToolBoxStorage[nodeType];
			else
				type = ToolBoxStorage.SingleOrDefault(f => f.Value.FullName == nodeType).Value;

			if (type == null)
				return null;

			var newNode = new Node(Activator.CreateInstance(type) as NodeBase);
			if (newNode == null)
				return null;

			Canvas.SetLeft(newNode, position.X);
			Canvas.SetTop(newNode, position.Y);

			WorkspaceCanvas.Children.Add(newNode);
			WorkspaceCanvas.UpdateLayout();
			Nodes.Add(newNode);

			return newNode;
		}

		#region simulation

		/// <summary>
		/// start new simulation
		/// </summary>
		private void NewSimulation()
		{
			MessagesBox.Text = null;

			var worker = new BackgroundWorker
			{
				WorkerReportsProgress = true
			};

			worker.DoWork += OnSimulation;
			worker.ProgressChanged += SimulationProgressChanged;

			worker.RunWorkerAsync();
		}

		/// <summary>
		/// simulation started
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSimulation(object sender, DoWorkEventArgs e)
		{
			try
			{
				var transactionId = Guid.NewGuid();

				Dispatcher.Invoke(() =>
				{
					Log.WriteLine($"{ transactionId.ToString("N") } Started ...");
				});

				Dispatcher.Invoke(() =>
				{
					StatusBarInformation.Content = $"{ DateTime.Now.ToLongTimeString() } Running ({ transactionId.ToString("N") }) ...";
				});

				Dispatcher.Invoke(() =>
				{
					for (var i = 0; i < WorkspaceCanvas.Children.Count; i++)
					{
						(WorkspaceCanvas.Children[i] as Node)?.Execute(transactionId);
						(sender as BackgroundWorker).ReportProgress(System.Convert.ToInt32(((double)(i + 1) / WorkspaceCanvas.Children.Count) * 100));
					}
				});

				Dispatcher.Invoke(() =>
				{
					Log.WriteLine($"{ transactionId.ToString("N") } ... Finished");
				});

				Dispatcher.Invoke(() =>
				{
					StatusBarInformation.Content = $"{ DateTime.Now.ToLongTimeString() } Ready ({ transactionId.ToString("N") })";
				});
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		/// <summary>
		/// simulation progress changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SimulationProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//StatusBarProgress.Value = e.ProgressPercentage;
			// TODO UI show simulation progress in statuis bar
		}

		#endregion

		#region messages box

		private void MessagesBoxHeader_MouseEnter(object sender, MouseEventArgs e)
		{
			try
			{
				MessagesBoxHeader.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x60, 0x80, 0x60));
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void MessagesBoxHeader_MouseLeave(object sender, MouseEventArgs e)
		{
			try
			{
				MessagesBoxHeader.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x50, 0x70, 0x50));
				MessagesBoxResizing = false;
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void MessagesBoxHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (e.ClickCount == 2)
					ToggleMessagesBoxCollapse();
				else
				{
					MessagesBoxResizing = true;
					MessagesBoxResizingLast = e.GetPosition(MainWindowGrid);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void MessagesBoxHeader_MouseUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				MessagesBoxResizing = false;
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void MessagesBoxHeader_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (MessagesBoxResizing)
				{
					var messagesBoxMinHeight = MessagesBoxHeader.ActualHeight + MessagesBoxBorder.BorderThickness.Top + MessagesBoxBorder.BorderThickness.Bottom;
					if ((MessagesBoxBorder.ActualHeight - (e.GetPosition(MainWindowGrid).Y - MessagesBoxResizingLast.Y)) > messagesBoxMinHeight)
						MessagesBoxBorder.Height = MessagesBoxBorder.ActualHeight - (e.GetPosition(MainWindowGrid).Y - MessagesBoxResizingLast.Y);

					MessagesBoxResizingLast = e.GetPosition(MainWindowGrid);

					MessagesBoxCollapsed = false;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void ToggleMessagesBoxCollapse()
		{
			if (MessagesBoxCollapsed)
			{
				MessagesBoxBorder.Height = MessagesBoxBeforeCollapse.Value.Height;
				MessagesBoxBeforeCollapse = null;
			}
			else
			{
				var messagesBoxMinHeight = MessagesBoxHeader.ActualHeight + MessagesBoxBorder.BorderThickness.Top + MessagesBoxBorder.BorderThickness.Bottom;

				MessagesBoxBeforeCollapse = new Size(MessagesBoxBorder.ActualWidth, MessagesBoxBorder.ActualHeight);
				MessagesBoxBorder.Height = messagesBoxMinHeight;
			}

			MessagesBoxCollapsed = !MessagesBoxCollapsed;
		}

		#endregion
	}
}
