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
		///  selection rectangle around nodes
		/// </summary>
		public static Rectangle SelectionRectangle { get; set; } = null;
		public static Point SelectionRectangleStart { get; set; }

		/// <summary>
		/// connection that is currently created
		/// </summary>
		public static Connection NewConnection { get; set; } = null;

		/// <summary>
		/// node that is currently moved with mouse
		/// </summary>
		public static Node NodeDragged { get; set; } = null;
		public static Point NodeDragStart { get; set; }

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
			ToolBoxStorage.Clear();
			AddNodesToToolBox("Ujeby.UgUi.Nodes.");

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

		/// <summary>
		/// add all types from sourceNamespace with FunctionInfo attribute to ToolBoxStorage
		/// </summary>
		/// <param name="sourceNamespace"></param>
		private static void AddNodesToToolBox(string sourceNamespace)
		{
			var nodeTypes = Assembly.LoadFrom("Ujeby.UgUi.Nodes.dll").GetTypes()
				.Where(t => t.Namespace.StartsWith(sourceNamespace) && t.CustomAttributes.Any(a => a.AttributeType == typeof(NodeInfoAttribute)))
				.Where(t => !t.CustomAttributes.Single(a => a.AttributeType == typeof(NodeInfoAttribute)).NamedArguments.Any(na => na.MemberName == nameof(NodeInfoAttribute.Abstract)))
				.ToArray();

			foreach (var nodeType in nodeTypes)
			{
				var groupName = nodeType.Namespace.Substring(nodeType.Namespace.LastIndexOf(".") + 1);
				var elementName = AttributeHelper.GetValue<NodeInfoAttribute, string>(nodeType, nameof(NodeInfoAttribute.DisplayName)) ?? nodeType.Name;

				ToolBoxStorage.Add($"{ groupName }.{ elementName }", nodeType);
			}
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

		internal static bool Save(string currentWorkspaceFile, FrameworkElement[] nodesToSave, out string newFile)
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
						var newNode = AddNodeControl(node.TypeName, new Point(node.Position.X - center.X + position.X, node.Position.Y - center.Y + position.Y));
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

		private static bool SaveCurrent()
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
				InitializeControlsTreeView();

				// collapse messages box
				ToggleMessagesBoxCollapse();

				DrawGrid();

				// create and hide toolbox
				ToolBox = new ToolBox(AddNodeControl)
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

				ToolBoxBorder.Visibility = Visibility.Collapsed;
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
						RemoveControl(control as Node);

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
						RemoveAllControls();
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
							var newNode = AddNodeControl(node.Value, new Point(mousePosition.X + node.Key.X, mousePosition.Y + node.Key.Y));
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
			RemoveAllControls();

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
				{
					// show context menu

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
				else if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (Mouse.DirectlyOver == WorkspaceCanvas && e.ClickCount == 2)
					{
						// show toolbox
						if (Mouse.DirectlyOver == WorkspaceCanvas)
						{
							Canvas.SetLeft(ToolBox, mousePosition.X);
							Canvas.SetTop(ToolBox, mousePosition.Y);

							ToolBox.Show();
						}
					}
					else
					{
						if (Mouse.DirectlyOver == WorkspaceCanvas)
						{
							// start drawing selection rectangle
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
						else
						{
							var anchor = Mouse.DirectlyOver as FrameworkElement;
							var elementControl = GetElementControl(anchor);

							if (anchor.Name != null && anchor.Name.StartsWith(Node.OutputAnchorPrefix))
							{
								if (e.ClickCount == 2)
								{
									CreateDefaultNode(elementControl as Node, anchor.Name);
								}
								else
								{
									// start drawing new connection
									NewConnection = new Connection(elementControl, null, anchor.Name, null);

									var userControlPosition = elementControl.TranslatePoint(new Point(), WorkspaceCanvas);
									var anchorPosition = elementControl.GetRelativeAnchorPosition(anchor.Name);

									NewConnection.Update(new Point(userControlPosition.X + anchorPosition.X, userControlPosition.Y + anchorPosition.Y), mousePosition);
									NewConnection.AddToUICollection(WorkspaceCanvas.Children);

									Cursor = Cursors.Hand;
								}
							}
							else if (anchor.Name != null && anchor.Name.StartsWith(Node.InputAnchorPrefix) && Connections.Any(c => c.RightAnchorName == anchor.Name))
							{
								// move existing connection
								NewConnection = Connections.SingleOrDefault(c => elementControl == c.Right && c.RightAnchorName == anchor.Name);
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
							else
							{
								var element = Mouse.DirectlyOver as FrameworkElement;
								if (element.Name == Node.HeaderElementName || element.Name == Node.HeaderTitleElementName)
								{
									// start moving node
									while (element != null && element as Node == null)
										element = element.Parent as FrameworkElement;
									var node = element as Node;

									NodeDragStart = mousePosition;
									NodeDragged = node;

									Cursor = Cursors.SizeAll;
								}
							}
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

		private void CreateDefaultNode(Node node, string outputAnchorName)
		{
			// TODO WORKSPACE create default node based on output anchor property type (string / double / ...)

			Log.WriteLine($"CreateDefaultNode({ node.NodeInstance.GetType().Name }, { outputAnchorName })");
		}

		private void Workspace_MouseUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					if (SelectionRectangle != null)
					{
						// stop drawing selection region

						foreach (FrameworkElement element in WorkspaceCanvas.Children)
							(element as Node)?.Select((element as Node).Hilighted);

						WorkspaceCanvas.Children.Remove(SelectionRectangle);
						SelectionRectangle = null;
					}
					else if (NewConnection != null)
					{
						var anchor = Mouse.DirectlyOver as FrameworkElement;
						var elementControl = GetElementControl(anchor);

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
					else if (NodeDragged != null)
					{
						// stop moving node
						NodeDragged = null;
						Cursor = Cursors.Arrow;
					}
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

		private void Workspace_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				var mousePosition = e.GetPosition(WorkspaceCanvas);

				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (SelectionRectangle != null)
					{
						// update selection region

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
					else if (NewConnection != null)
						// update current connection
						NewConnection.Update(null, mousePosition);

					else if (NodeDragged != null)
					{
						// move node / selected nodes

						MoveControls(NodeDragged, mousePosition - NodeDragStart);
						NodeDragStart = mousePosition;
					}
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

		private void UpdateScale()
		{
			var scaleOrigin = new Point(WorkspaceCanvas.ActualWidth * 0.5 + GridOffset.X, WorkspaceCanvas.ActualHeight * 0.5 + GridOffset.Y);

			foreach (var node in Nodes)
				node.RenderTransform = new ScaleTransform(Scale, Scale, scaleOrigin.X - Canvas.GetLeft(node), scaleOrigin.Y - Canvas.GetTop(node));

			foreach (var node in Nodes)
				MoveControl(node, new Vector());

			DrawGrid();
		}

		private void Workspace_MouseLeave(object sender, MouseEventArgs e)
		{
			try
			{
				if (SelectionRectangle != null)
				{
					WorkspaceCanvas.Children.Remove(SelectionRectangle);
					SelectionRectangle = null;

					// TODO WORKSPACE deselect nodes that were selected with this selection rectangle
				}

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

		private void Workspace_Drop(object sender, DragEventArgs e)
		{
			try
			{
				if (e.Data.GetDataPresent(DataFormats.StringFormat))
					AddNodeControl((string)e.Data.GetData(DataFormats.StringFormat), e.GetPosition(WorkspaceCanvas));
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.Message);
			}
		}

		private void MoveWorkspace(Vector offset)
		{
			DrawGrid();

			foreach (var node in Nodes)
				MoveControl(node, offset);
		}

		private Node GetElementControl(FrameworkElement element)
		{
			if (element == null)
				return null;

			if (element is Node)
				return element as Node;

			while (element?.Parent != null)
			{
				if (element is Node)
					return element as Node;

				element = element.Parent as FrameworkElement;
			}

			return null;
		}

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

		private void SimulationProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//StatusBarProgress.Value = e.ProgressPercentage;
		}

		public void RemoveAllControls()
		{
			var toRemove = Nodes.ToArray();
			foreach (var control in toRemove)
				RemoveControl(control);
		}

		public void RemoveControl(Node control)
		{
			if (control == null)
				return;

			control.Remove();

			Nodes.Remove(control);
			WorkspaceCanvas.Children.Remove(control);
		}

		private void AddConnection(Connection connection, bool addToUI)
		{
			Connections.Add(connection);

			if (addToUI)
				connection.AddToUICollection(WorkspaceCanvas.Children);

			connection.Left.AddConnectionTo(connection);

			// TODO CORE new simulation after added connection ?
			connection.Right.AddConnectionFrom(connection, false);
		}

		private Node AddNodeControl(string nodeType, Point position)
		{
			Type type = null;
			if (ToolBoxStorage.ContainsKey(nodeType))
				type = ToolBoxStorage[nodeType];
			else
				type = ToolBoxStorage.SingleOrDefault(f => f.Value.FullName == nodeType).Value;

			if (type == null)
				return null;

			var control = new Node(Activator.CreateInstance(type) as NodeBase);
			if (control == null)
				return null;

			Canvas.SetLeft(control, position.X);
			Canvas.SetTop(control, position.Y);

			WorkspaceCanvas.Children.Add(control);
			WorkspaceCanvas.UpdateLayout();
			Nodes.Add(control);

			return control;
		}

		/// <summary>
		/// Initialize ControlsTreeView with functions from Editor.FunctionsContainer
		/// </summary>
		private void InitializeControlsTreeView()
		{
			if (ToolBoxStorage.Count < 1)
				return;

			var parentTreeViewItem = null as TreeViewItem;
			foreach (var element in ToolBoxStorage)
			{
				var groupName = element.Key.Split('.').First();
				var elementName = element.Key.Split('.').Last();

				if (parentTreeViewItem == null || groupName != parentTreeViewItem.Header as string)
				{
					parentTreeViewItem = new TreeViewItem()
					{
						Header = groupName,
						Foreground = new SolidColorBrush(Node.TextForegroundHilighted),
						IsExpanded = true
					};
					ControlsTreeView.Items.Add(parentTreeViewItem);
				}

				var newItem = new TreeViewItem()
				{
					Header = elementName,
					Foreground = new SolidColorBrush(Node.TextForeground),
				};
				newItem.MouseMove += ControlsTreeView_MouseMove;

				parentTreeViewItem.Items.Add(newItem);
			}
		}

		private void ControlsTreeView_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					var item = sender as TreeViewItem;

					// element full name = Namespace.Name
					var data = $"{ (item.Parent as TreeViewItem).Header as string }.{ item.Header as string }";

					DragDrop.DoDragDrop(item, data, DragDropEffects.Move);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

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

		private void MoveControls(Node elementControl, Vector delta)
		{
			MoveControl(elementControl, delta);

			if (elementControl.Selected)
			{
				// move other selected elements
				foreach (var control in WorkspaceCanvas.Children)
				{
					if (control == elementControl)
						continue;

					var otherElementControl = control as Node;
					if (otherElementControl != null && otherElementControl.Selected)
						MoveControl(otherElementControl, delta);
				}
			}
		}

		private void MoveControl(Node elementControl, Vector delta)
		{
			var currentPosition = new Point(Canvas.GetLeft(elementControl), Canvas.GetTop(elementControl));
			Canvas.SetLeft(elementControl, currentPosition.X + delta.X);
			Canvas.SetTop(elementControl, currentPosition.Y + delta.Y);

			var transformedTopLeft = elementControl.TranslatePoint(new Point(), WorkspaceCanvas);

			elementControl.UpdateConnections(new Point(transformedTopLeft.X + delta.X, transformedTopLeft.Y + delta.Y));
		}

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

		private void ToolBoxHeader_MouseEnter(object sender, MouseEventArgs e)
		{
			try
			{
				ToolBoxHeader.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x60, 0x60, 0x80));
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		private void ToolBoxHeader_MouseLeave(object sender, MouseEventArgs e)
		{
			try
			{
				ToolBoxHeader.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x50, 0x50, 0x70));
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}
	}
}
