using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Ujeby.Common.Tools;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Common;
using System.Windows.Media.Imaging;

namespace Ujeby.UgUi.Controls
{
	/// <summary>
	/// Interaction logic for NodeControl.xaml
	/// </summary>
	public partial class Node : UserControl
	{
		public Node(INode nodeInstance)
		{
			NodeInstance = nodeInstance;

			InitializeComponent();

			// generate control interface based on node type
			InitializeControl(NodeInstance.GetType());
		}

		#region constants

		public const string InputAnchorPrefix = "InputAnchor";
		public const string OutputAnchorPrefix = "OutputAnchor";
		public const string LabelPrefix = "Label";
		public const string InputValuePrefix = "Input";
		public const string OutputValuePrefix = "Output";
		public const string HeaderElementName = "Header";
		public const string MainPanelElementName = "MainPanel";
		public const string DefaultOutputAnchorName = "OutputAnchorArray";
		public const string TitleElmentName = "Title";

		public const int HeaderHeight = 24;
		public const int AnchorSize = 16;
		public const int DecimalPrecision = 4;

		const int LOG_MAX_INPUT_LENGTH = 64;
		const int LOG_MAX_OUTPUT_LENGTH = 64;

		public const int BlurRadius = 8;

		#endregion

		private static Color? HilightColor { get; set; }// = Color.FromArgb(0xff, 0xcc, 0xcc, 0xcc);

		public static byte ControlTransparency = 0xee;

		public static Color InputBorder = Color.FromArgb(ControlTransparency, 0x20, 0x20, 0x20);
		public static Color InputBackground = Color.FromArgb(0xff, 0x30, 0x30, 0x30);
		public static Color ControlBackground = Color.FromArgb(ControlTransparency, 0x40, 0x40, 0x40);
		public static Color HeaderBackground = Color.FromArgb(ControlTransparency, 0x50, 0x50, 0x50);
		public static Color TextForeground = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
		public static Color TextForegroundHilighted = Colors.White;

		/// <summary>
		/// last transaction id
		/// </summary>
		public Guid LastTransactionId { get; private set; } = Guid.Empty;

		/// <summary>
		/// unique id
		/// </summary>
		public Guid Id { get; set; } = Guid.NewGuid();

		public string CustomName { get; private set; }
		public Color CustomColor { get; private set; }

		public INode NodeInstance { get; protected set; } = null;

		/// <summary>
		/// creates control user interface based on node type
		/// </summary>
		/// <param name="mainColor"></param>
		protected void InitializeControl(Type nodeType)
		{
			var mainPanel = FindName(MainPanelElementName) as Grid;
			mainPanel.DataContext = NodeInstance;
			mainPanel.Background = new SolidColorBrush(ControlBackground);
			mainPanel.MouseEnter += ControlMouseEnter;
			mainPanel.MouseLeave += ControlMouseLeave;

			var ignoredProperties = nodeType.CustomAttributes
				.Where(a => a.AttributeType == typeof(IgnoredPropertyAttribute))
				.Select(ca => ca.NamedArguments.Single(na => na.MemberName == nameof(IgnoredPropertyAttribute.Name)).TypedValue.Value as string).ToArray();

			var inputProperties = nodeType.GetProperties()
				.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(InputAttribute)))
				.Where(pi => ignoredProperties == null || !ignoredProperties.Contains(pi.Name))
				.OrderBy(p => p.CustomAttributes.Single(a => a.AttributeType == typeof(InputAttribute)).NamedArguments.Single(a => a.MemberName == nameof(InputAttribute.Order)).TypedValue.Value);

			var outputProperties = nodeType.GetProperties()
				.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(OutputAttribute)))
				.Where(pi => ignoredProperties == null || !ignoredProperties.Contains(pi.Name));

			#region grid definition

			// columns
			mainPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AnchorSize / 2) });               // input anchor
			mainPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = AnchorSize / 4 });   // input label
			mainPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 64 });               // property value representation - TextBox for example
			mainPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = AnchorSize / 4 });   // output label
			mainPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AnchorSize / 2) });               // output anchor

			// rows
			mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                   // header
			mainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(AnchorSize / 2) });    // header divider

			// add rows for all input properties
			for (var i = 0; i < inputProperties.Count(); i++)
				mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			// number of input rows that have free output side
			var inputAnchorsOnly = inputProperties.Count(ip => AttributeHelper.GetValue<InputAttribute, bool>(ip, nameof(InputAttribute.AnchorOnly)) && !AttributeHelper.GetValue<InputAttribute, bool>(ip, nameof(InputAttribute.OutputAnchor)));
			// number of outputs that could be moved up next to inputs
			var outputAnchorsOnly = outputProperties.Count(op => AttributeHelper.GetValue<OutputAttribute, bool>(op, nameof(OutputAttribute.AnchorOnly)));
			// outputs that needs to be at bottom
			var outputLeft = outputProperties.Count() - outputAnchorsOnly;
			// number of output rows that should be added under input rows
			var outputRowsToAdd = outputLeft + Math.Max(0, outputAnchorsOnly - inputAnchorsOnly);

			// add rows for some output properties
			for (var i = 0; i < outputRowsToAdd; i++)
				mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			mainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(AnchorSize / 2) });    // bottom

			#endregion

			#region header

			CustomName = AttributeHelper.GetValue<NodeInfoAttribute, string>(NodeInstance.GetType(), nameof(NodeInfoAttribute.DisplayName)) ?? NodeInstance.GetType().Name;

			var colorValues = BitConverter.GetBytes(CustomName.GetHashCode());
			CustomColor = Color.FromArgb(0xff, colorValues[1], colorValues[2], colorValues[3]);

			var header = new Border
			{
				Background = new LinearGradientBrush(CustomColor, Color.Multiply(CustomColor, (float)0.75), new Point(0, 1), new Point(1, 0)),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				Height = HeaderHeight,
				Name = HeaderElementName,
			};
			header.MouseLeftButtonDown += Header_MouseLeftButtonDown;

			// title
			header.Child = new TextBlock
			{
				Name = TitleElmentName,
				Text = CustomName,
				//Foreground = new SolidColorBrush(Color.FromRgb(0xc0, 0xc0, 0xc0)),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				FontFamily = new FontFamily("Consolas"),
			};
			//header.Child = new DockPanel();

			//// title
			//(header.Child as DockPanel).Children.Add(
			//	new TextBlock
			//	{
			//		Text = (GetAttributeNamedValue(elementInfo, nameof(FunctionInfoAttribute.DisplayName)) as string ?? Function.GetType().Name),
			//		VerticalAlignment = VerticalAlignment.Center,
			//		HorizontalAlignment = HorizontalAlignment.Center,
			//		Margin = new Thickness(4, 0, 0, 0),
			//	});

			//// collapse button
			//var closeButton = new Rectangle
			//{
			//	Fill = new SolidColorBrush(mainColor),
			//	Stroke = new SolidColorBrush(Colors.Black),
			//	VerticalAlignment = VerticalAlignment.Center,
			//	HorizontalAlignment = HorizontalAlignment.Right,
			//	Margin = new Thickness(0, 0, 3, 0),
			//	Height = HeaderHeight - 8,
			//	Width = HeaderHeight - 8,
			//};
			//DockPanel.SetDock(closeButton, Dock.Right);
			//(header.Child as DockPanel).Children.Add(closeButton);

			//// close button
			//var collapseButton = new Rectangle
			//{
			//	Fill = new SolidColorBrush(mainColor),
			//	Stroke = new SolidColorBrush(Colors.Black),
			//	VerticalAlignment = VerticalAlignment.Center,
			//	HorizontalAlignment = HorizontalAlignment.Right,
			//	Margin = new Thickness(0, 0, 3, 0),
			//	Height = HeaderHeight - 8,
			//	Width = HeaderHeight - 8,
			//};
			//DockPanel.SetDock(collapseButton, Dock.Right);
			//(header.Child as DockPanel).Children.Add(collapseButton);

			// drag behavior
			//Interaction.GetBehaviors(header).Add(new DragBehavior());
			Grid.SetRow(header, 0);
			Grid.SetColumnSpan(header, mainPanel.ColumnDefinitions.Count);
			mainPanel.Children.Add(header);

			#endregion

			#region border

			var border = new Border
			{
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1, 0, 1, 1)
			};
			Grid.SetRow(border, 1);
			Grid.SetColumnSpan(border, mainPanel.ColumnDefinitions.Count);
			Grid.SetRowSpan(border, mainPanel.RowDefinitions.Count - 1);
			mainPanel.Children.Add(border);

			#endregion

			var firstRow = 2;
			var inputRows = new bool[mainPanel.RowDefinitions.Count - 3];
			var outputRows = new bool[mainPanel.RowDefinitions.Count - 3];

			foreach (var inputProperty in inputProperties)
			{
				var inputAttribute = inputProperty.CustomAttributes.Single(a => a.AttributeType == typeof(InputAttribute));

				var readOnly = AttributeHelper.GetValue<InputAttribute, bool>(inputProperty, nameof(InputAttribute.ReadOnly));
				var anchorOnly = AttributeHelper.GetValue<InputAttribute, bool>(inputProperty, nameof(InputAttribute.AnchorOnly));
				var inputAnchor = AttributeHelper.GetValue<InputAttribute, bool>(inputProperty, nameof(InputAttribute.InputAnchor));
				var outputAnchor = AttributeHelper.GetValue<InputAttribute, bool>(inputProperty, nameof(InputAttribute.OutputAnchor));
				var inputName = AttributeHelper.GetValue<InputAttribute, string>(inputProperty, nameof(InputAttribute.InputName)) ?? inputProperty.Name;
				var inputDisplayName = AttributeHelper.GetValue<InputAttribute, string>(inputProperty, nameof(InputAttribute.DisplayName)) ?? inputProperty.Name;
				var inputAnchorProperty = AttributeHelper.GetValue<InputAttribute, string>(inputProperty, nameof(InputAttribute.InputAnchorProperty)) ?? inputProperty.Name;
				var outputAnchorProperty = AttributeHelper.GetValue<InputAttribute, string>(inputProperty, nameof(InputAttribute.OutputAnchorProperty)) ?? inputProperty.Name;

				// next free row
				var row = firstRow;
				for (var i = 0; i < outputRows.Length; i++)
					if (!inputRows[i])
					{
						row += i;
						break;
					}

				if (inputAnchor)
				{
					#region input anchor

					var anchor = new Rectangle
					{
						Fill = new SolidColorBrush(InputBackground),
						Stroke = new SolidColorBrush(Colors.Black),
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Left,
						Margin = new Thickness(-AnchorSize / 2, 0, 0, 0),
						Width = AnchorSize,
						Height = AnchorSize,
						Name = InputAnchorPrefix + inputAnchorProperty,
						ToolTip = inputProperty.PropertyType.Name,
					};
					anchor.MouseEnter += HilightInputAnchorOn;
					anchor.MouseLeave += HilightInputAnchorOff;

					Grid.SetRow(anchor, row);
					Grid.SetColumn(anchor, 0);
					mainPanel.Children.Add(anchor);

					#endregion
				}

				#region label

				var label = new Label
				{
					Foreground = new SolidColorBrush(TextForeground),
					VerticalAlignment = VerticalAlignment.Center,
					Name = LabelPrefix + inputAnchorProperty,
					FontFamily = new FontFamily("Consolas"),
				};
				label.Content = inputDisplayName;

				Grid.SetRow(label, row);
				Grid.SetColumn(label, 1);
				if (inputAnchor && anchorOnly && outputAnchor)
				{
					label.HorizontalAlignment = HorizontalAlignment.Right;
					Grid.SetColumnSpan(label, 3);
				}
				else
					label.HorizontalAlignment = HorizontalAlignment.Left;
				mainPanel.Children.Add(label);

				#endregion

				if (!anchorOnly)
				{
					#region input element

					if (inputProperty.PropertyType == typeof(SolidColorBrush))
					{
						var colorElement = new TextBox
						{
							Background = new SolidColorBrush(Colors.Black),
							Foreground = new SolidColorBrush(Colors.White),
							BorderThickness = new Thickness(1),
							BorderBrush = new SolidColorBrush(InputBorder),
							Margin = new Thickness(0, 0, 0, 0),
							Name = InputValuePrefix + inputName,
							FontFamily = new FontFamily("Consolas"),
							HorizontalContentAlignment = HorizontalAlignment.Center,
							VerticalContentAlignment = VerticalAlignment.Center,
							VerticalAlignment = VerticalAlignment.Stretch,
							HorizontalAlignment = HorizontalAlignment.Stretch,
							IsReadOnly = true,
							Width = 128,
						};

						var binding = new Binding(inputProperty.Name)
						{
							Mode = BindingMode.OneWay,
						};
						colorElement.SetBinding(TextBox.TextProperty, binding);
						colorElement.SetBinding(TextBox.BackgroundProperty, binding);

						Grid.SetRow(colorElement, row);
						Grid.SetColumn(colorElement, 2);
						mainPanel.Children.Add(colorElement);
					}
					else
					{
						var textbox = new TextBox
						{
							Foreground = new SolidColorBrush(TextForeground),
							BorderBrush = new SolidColorBrush(InputBorder),
							Background = new SolidColorBrush(InputBackground),
							VerticalAlignment = VerticalAlignment.Center,
							TextAlignment = TextAlignment.Right,
							Margin = new Thickness(0, 0, 4, 0),
							Name = InputValuePrefix + inputName,
							IsReadOnly = readOnly,
							FontFamily = new FontFamily("Consolas"),
							TextWrapping = TextWrapping.Wrap,
							VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
							MaxHeight = 128,
							MaxWidth = 256,
						};

						var binding = new Binding(inputProperty.Name)
						{
							Mode = readOnly ? BindingMode.OneWay : BindingMode.TwoWay,
							ValidatesOnExceptions = true
						};

						if (inputProperty.PropertyType == typeof(double))
						{
							textbox.MinWidth = 64;
							binding.StringFormat = "{0:F" + DecimalPrecision + "}";
						}
						else if (inputProperty.PropertyType == typeof(int))
						{
							textbox.MinWidth = 64;
						}
						else if (inputProperty.PropertyType == typeof(string))
						{
							textbox.MinWidth = 128;
							textbox.TextAlignment = TextAlignment.Left;
							textbox.TextWrapping = TextWrapping.NoWrap;
						}

						textbox.SetBinding(TextBox.TextProperty, binding);

						Grid.SetRow(textbox, row);
						Grid.SetColumn(textbox, 2);
						mainPanel.Children.Add(textbox);
					}

					#endregion

					outputRows[row - firstRow] = true;
				}

				if (outputAnchor)
				{
					#region output anchor

					var anchor = new Rectangle
					{
						Fill = new SolidColorBrush(CustomColor),
						Stroke = new SolidColorBrush(Colors.Black),
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Right,
						Margin = new Thickness(0, 0, -AnchorSize / 2, 0),
						Width = AnchorSize,
						Height = AnchorSize,
						Name = OutputAnchorPrefix + outputAnchorProperty,
						ToolTip = inputProperty.PropertyType.Name,
					};
					anchor.MouseEnter += HilightOutputAnchorOn;
					anchor.MouseLeave += HilightOutputAnchorOff;

					Grid.SetRow(anchor, row);
					Grid.SetColumn(anchor, mainPanel.ColumnDefinitions.Count - 1);
					mainPanel.Children.Add(anchor);

					#endregion

					outputRows[row - firstRow] = true;
				}

				inputRows[row - firstRow] = true;
			}

			foreach (var outputProperty in outputProperties)
			{
				var outputAttribute = outputProperty.CustomAttributes.Single(a => a.AttributeType == typeof(OutputAttribute));

				var readOnly = AttributeHelper.GetValue<OutputAttribute, bool>(outputProperty, nameof(OutputAttribute.ReadOnly));
				var anchorOnly = AttributeHelper.GetValue<OutputAttribute, bool>(outputProperty, nameof(OutputAttribute.AnchorOnly));
				var noAnchor = AttributeHelper.GetValue<OutputAttribute, bool>(outputProperty, nameof(OutputAttribute.NoAnchor));
				var outputDisplayName = AttributeHelper.GetValue<OutputAttribute, string>(outputProperty, nameof(OutputAttribute.DisplayName)) ?? outputProperty.Name;
				var outputAnchorProperty = AttributeHelper.GetValue<OutputAttribute, string>(outputProperty, nameof(OutputAttribute.OutputAnchorProperty)) ?? outputProperty.Name;

				// next free row
				var row = firstRow;
				for (var i = 0; i < outputRows.Length; i++)
					if (!outputRows[i] && (anchorOnly || !inputRows[i]))
					{
						row += i;
						break;
					}

				if (!anchorOnly)
				{
					#region output element

					if (outputProperty.PropertyType == typeof(BitmapSource))
					{
						var image = new Image
						{
							Width = 256,
							Height = 256,
						};

						var binding = new Binding(outputProperty.Name)
						{
							Mode = BindingMode.OneWay
						};
						image.SetBinding(Image.SourceProperty, binding);

						var binding2 = new Binding("SizeX")
						{
							Mode = BindingMode.OneWay
						};
						image.SetBinding(Image.WidthProperty, binding2);

						var binding3 = new Binding("SizeY")
						{
							Mode = BindingMode.OneWay
						};
						image.SetBinding(Image.HeightProperty, binding3);

						Grid.SetRow(image, row);
						Grid.SetColumn(image, 1);
						Grid.SetColumnSpan(image, mainPanel.ColumnDefinitions.Count - 2);
						mainPanel.Children.Add(image);
					}
					else
					{
						var textbox = new TextBox
						{
							Foreground = new SolidColorBrush(TextForeground),
							BorderBrush = new SolidColorBrush(InputBorder),
							Background = new SolidColorBrush(InputBackground),
							VerticalAlignment = VerticalAlignment.Center,
							TextAlignment = outputProperty.PropertyType == typeof(string) ? TextAlignment.Left : TextAlignment.Right,
							Margin = new Thickness(0, 0, 4, 0),
							Name = OutputValuePrefix + outputAnchorProperty,
							FontFamily = new FontFamily("Consolas"),
							IsReadOnly = readOnly,

							TextWrapping = TextWrapping.Wrap,
							VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
							MaxHeight = 128,
							MaxWidth = 256,
						};

						var binding = new Binding(outputProperty.Name)
						{
							Mode = BindingMode.TwoWay,
							ValidatesOnExceptions = true
						};

						if (outputProperty.PropertyType == typeof(double))
							binding.StringFormat = "{0:F" + DecimalPrecision + "}";

						textbox.SetBinding(TextBox.TextProperty, binding);

						Grid.SetRow(textbox, row);
						Grid.SetColumn(textbox, 2);
						mainPanel.Children.Add(textbox);
					}

					#endregion
				}

				#region label

				var label = new Label
				{
					Foreground = new SolidColorBrush(TextForeground),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Right,
					Name = LabelPrefix + outputAnchorProperty,
					FontFamily = new FontFamily("Consolas"),
				};
				label.Content = outputDisplayName;

				Grid.SetRow(label, row);
				if (anchorOnly)
				{
					Grid.SetColumn(label, 0);
					Grid.SetColumnSpan(label, mainPanel.ColumnDefinitions.Count - 1);
				}
				else
					Grid.SetColumn(label, mainPanel.ColumnDefinitions.Count - 2);

				mainPanel.Children.Add(label);

				#endregion

				if (!noAnchor)
				{
					#region output anchor

					var anchor = new Rectangle
					{
						Fill = new SolidColorBrush(CustomColor),
						Stroke = new SolidColorBrush(Colors.Black),
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Right,
						Margin = new Thickness(0, 0, -8, 0),
						Width = AnchorSize,
						Height = AnchorSize,
						Name = OutputAnchorPrefix + outputAnchorProperty,
						ToolTip = outputProperty.PropertyType.Name,
					};
					anchor.MouseEnter += HilightOutputAnchorOn;
					anchor.MouseLeave += HilightOutputAnchorOff;

					Grid.SetRow(anchor, row);
					Grid.SetColumn(anchor, mainPanel.ColumnDefinitions.Count - 1);
					mainPanel.Children.Add(anchor);

					#endregion
				}

				outputRows[row - firstRow] = true;
			}
		}

		/// <summary>
		/// connections from other nodes [-> input]
		/// </summary>
		public List<Connection> ConnectionsFrom { get; protected set; } = new List<Connection>();

		/// <summary>
		/// connections to other nodes [output ->]
		/// </summary>
		public List<Connection> ConnectionsTo { get; protected set; } = new List<Connection>();

		/// <summary>
		/// position relative to application window
		/// </summary>
		public Point WorkspacePosition { get { return this.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)); } }

		public void Remove()
		{
			foreach (var connection in ConnectionsFrom)
			{
				// remove shapes
				connection.RemoveFromUICollection((Parent as Canvas).Children);
				// remove connection from connected element
				connection.Left.RemoveConnection(connection);
				// remove connection from global list
				Workspace.Connections.Remove(connection);
			}

			foreach (var connection in ConnectionsTo)
			{
				// remove shapes
				connection.RemoveFromUICollection((Parent as Canvas).Children);
				// remove connection from connected element
				connection.Right.RemoveConnection(connection);
				// remove connection from global list
				Workspace.Connections.Remove(connection);
			}

			// remove all connections
			ConnectionsFrom.Clear();
			ConnectionsTo.Clear();

			// remove user control
			(Parent as Canvas).Children.Remove(this);
		}

		public virtual Guid Execute(Guid transactionId)
		{
			if (LastTransactionId == transactionId)
				return LastTransactionId;

			LastTransactionId = transactionId;

			try
			{
				var stopWatch = new Stopwatch();

				foreach (var connection in ConnectionsFrom)
				{
					// generate output from left node
					(connection.Left as Node).Execute(LastTransactionId);
					var leftNode = connection.Left.NodeInstance;

					// get left output value
					var leftValue = leftNode
						.GetType()
						.GetProperty(connection.LeftPropertyName)
						.GetValue(leftNode);

					// set right input value
					var rightProperty = NodeInstance?.GetType().GetProperty(connection.RightPropertyName);
					if (rightProperty != null)
						rightProperty.SetValue(NodeInstance, Common.Convert.ChangeType(leftValue, rightProperty.PropertyType));
				}

				if (NodeInstance != null)
				{
					try
					{
						stopWatch.Reset();
						stopWatch.Start();

						// TODO start flash animation
						NodeInstance.Execute();
						// TODO stop flash animation

						stopWatch.Stop();

						// log node input & output	
						var nodeName = AttributeHelper.GetValue<NodeInfoAttribute, string>(NodeInstance.GetType(), nameof(NodeInfoAttribute.DisplayName)) ?? NodeInstance.GetType().Name;
						var nodeInputs = (NodeInstance as ILoggable)?.GetInputs()
							?.Select(i => i == null ? null : (i.Length < LOG_MAX_INPUT_LENGTH ? i : (i.Substring(0, LOG_MAX_INPUT_LENGTH) + "...")).Replace(Environment.NewLine, "\\r\\n"));
						var nodeOutputs = (NodeInstance as ILoggable)?.GetOutputs()
							?.Select(i => i == null ? null : (i.Length < LOG_MAX_OUTPUT_LENGTH ? i : (i.Substring(0, LOG_MAX_OUTPUT_LENGTH) + "...")).Replace(Environment.NewLine, "\\r\\n"));
						Log.WriteLine($"{ nodeName }('{ string.Join("', '", nodeInputs ?? new string[] { }) }')=['{ string.Join("', '", nodeOutputs ?? new string[] { }) }'] | { stopWatch.ElapsedMilliseconds }ms");
					}
					catch (Exception ex)
					{
						Log.WriteLine(ex.ToString());
					}
				}

				foreach (var outputConnection in ConnectionsTo)
					outputConnection.Right.Execute(LastTransactionId);
			}
			catch (FriendlyException ex)
			{
				Log.WriteLine(ex.Message);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}

			return LastTransactionId;
		}

		/// <summary>
		/// returns position of anchor relative to this control
		/// </summary>
		/// <param name="anchorName"></param>
		/// <returns></returns>
		public Point GetRelativeAnchorPosition(string anchorName)
		{
			if (Collapsed)
			{
				var header = GetElementByName(HeaderElementName) as FrameworkElement;
				return new Point(anchorName.StartsWith(InputAnchorPrefix) ? 0 : header.ActualWidth, header.ActualHeight / 2);
			}
			else
			{
				var anchor = GetElementByName(anchorName) as FrameworkElement;
				if (anchor == null)
					return new Point(0, 0);

				var anchorPosition = anchor.TranslatePoint(new Point(), this);
				anchorPosition.X += anchor.ActualWidth / 2;
				anchorPosition.Y += anchor.ActualHeight / 2;

				return anchorPosition;
			}
		}

		private TextBlock GetTitleElement()
		{
			return GetElementByName(TitleElmentName) as TextBlock;
		}

		#region connectivity

		public virtual void AddConnectionTo(Connection connection)
		{
			ConnectionsTo.Add(connection);
		}

		public virtual void AddConnectionFrom(Connection connection)
		{
			ConnectionsFrom.Add(connection);
			EnableInputElement(connection.RightAnchorName, false);

			Execute(Guid.NewGuid());
		}

		public virtual void RemoveConnection(Connection connection)
		{
			if (ConnectionsTo.Remove(connection))
				HilightOutputAnchorOffByName(connection.LeftAnchorName);

			if (ConnectionsFrom.Remove(connection))
			{
				// set right input value to null
				var rightProperty = NodeInstance.GetType().GetProperty(connection.RightPropertyName);
				if (rightProperty != null)
					rightProperty.SetValue(NodeInstance, null);

				EnableInputElement(connection.RightAnchorName);
				HilightInputAnchorOffByName(connection.RightAnchorName);
			}
		}

		public void UpdateConnections(Point topLeft)
		{
			foreach (var connection in ConnectionsTo)
			{
				var relativeAnchorPosition = GetRelativeAnchorPosition(connection.LeftAnchorName);
				connection.Update(new Point(topLeft.X + relativeAnchorPosition.X, topLeft.Y + relativeAnchorPosition.Y), null);
			}

			foreach (var connection in ConnectionsFrom)
			{
				var relativeAnchorPosition = GetRelativeAnchorPosition(connection.RightAnchorName);
				connection.Update(null, new Point(topLeft.X + relativeAnchorPosition.X, topLeft.Y + relativeAnchorPosition.Y));
			}
		}

		private void EnableInputElement(string anchorName, bool enable = true)
		{
			// NOTE for now, everything is enabled

			//var mainPanel = FindName(MainPanelElementName) as FrameworkElement;
			//for (var i = 0; i < VisualTreeHelper.GetChildrenCount(mainPanel); i++)
			//{
			//	var child = VisualTreeHelper.GetChild(mainPanel, i);
			//	if ((child as FrameworkElement).Name.StartsWith(InputValuePrefix + anchorName.Replace(InputAnchorPrefix, string.Empty)))
			//		(child as UIElement).IsEnabled = enable;
			//}
		}

		#endregion

		#region hilight

		public bool Hilighted { get; private set; }

		protected void ControlMouseEnter(object sender, MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed || Selected)
				return;

			Hilight(true);
		}

		protected void ControlMouseLeave(object sender, MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed || Selected)
				return;

			Hilight(false);
		}

		public void Hilight(bool hilight)
		{
			var title = GetTitleElement();

			if (hilight && !Hilighted)
			{
				Effect = new DropShadowEffect()
				{
					BlurRadius = BlurRadius,
					ShadowDepth = 0,
					Color = HilightColor.HasValue ? HilightColor.Value : CustomColor,
				};

				title.Foreground = new SolidColorBrush(Colors.White);
			}
			else if (!hilight && Hilighted)
			{
				Effect = new DropShadowEffect()
				{
					BlurRadius = BlurRadius,
					ShadowDepth = 0
				};

				title.Foreground = new SolidColorBrush(Colors.Black);
			}

			Hilighted = hilight;
		}

		/// <summary>
		/// resets input anchor background color based on connection
		/// </summary>
		/// <param name="name"></param>
		public void ResetInputAnchorColor(string name)
		{
			var anchor = GetElementByName(name);
			if (anchor == null)
				return;

			var inputColor = ConnectionsFrom.SingleOrDefault(c => c.RightAnchorName == name)?.Left?.CustomColor ?? InputBackground;

			// set anchor color
			(anchor as Rectangle).Fill = new SolidColorBrush(inputColor);
		}

		protected void HilightInputAnchorOn(object sender, MouseEventArgs e)
		{
			var anchorName = (sender as FrameworkElement).Name;
			var connection = ConnectionsFrom.SingleOrDefault(c => c.RightAnchorName == anchorName);

			// allow hilight only if connection is in progress and rightAnchor is valid, or if no connection is in progress and right anchor is not empty
			if ((Workspace.NewConnection != null && Workspace.CheckConnectionInProgress(this, anchorName)) || Workspace.NewConnection == null && connection != null)
			{
				(sender as Rectangle).Fill = new SolidColorBrush(Colors.White);

				var label = GetElementByName((sender as Rectangle).Name.Replace(InputAnchorPrefix, LabelPrefix)) as Label;
				if (label != null)
					label.Foreground = new SolidColorBrush(Colors.White);

				if (connection != null)
					connection.Hilight(Colors.White);
			}
		}

		protected void HilightInputAnchorOff(object sender, MouseEventArgs e)
		{
			HilightInputAnchorOffByName((sender as FrameworkElement).Name);
		}

		private void HilightInputAnchorOffByName(string name)
		{
			var anchor = GetElementByName(name);
			if (anchor == null)
				return;

			var inputColor = ConnectionsFrom.SingleOrDefault(c => c.RightAnchorName == name)?.Left?.CustomColor ?? InputBackground;

			// set anchor color
			(anchor as Rectangle).Fill = new SolidColorBrush(inputColor);

			var label = GetElementByName((anchor as Rectangle).Name.Replace(InputAnchorPrefix, LabelPrefix)) as Label;
			if (label != null)
				label.Foreground = new SolidColorBrush(TextForeground);

			// set connection color
			var connection = ConnectionsFrom.SingleOrDefault(c => c.RightAnchorName == name);
			if (connection != null)
				connection.Hilight(inputColor);
		}

		protected void HilightOutputAnchorOn(object sender, MouseEventArgs e)
		{
			if (Workspace.NewConnection != null)
				return;

			(sender as Rectangle).Fill = new SolidColorBrush(Colors.White);

			var label = GetElementByName((sender as Rectangle).Name.Replace(OutputAnchorPrefix, LabelPrefix)) as Label;
			if (label != null)
				label.Foreground = new SolidColorBrush(Colors.White);
		}

		protected void HilightOutputAnchorOff(object sender, MouseEventArgs e)
		{
			HilightOutputAnchorOffByName((sender as FrameworkElement).Name);
		}

		private void HilightOutputAnchorOffByName(string name)
		{
			var anchor = GetElementByName(name);
			if (anchor == null)
				return;

			(anchor as Rectangle).Fill = new SolidColorBrush(CustomColor);

			var label = GetElementByName((anchor as Rectangle).Name.Replace(OutputAnchorPrefix, LabelPrefix)) as Label;
			if (label != null)
				label.Foreground = new SolidColorBrush(TextForeground);
		}

		#endregion

		#region select

		public bool Selected { get; private set; }

		public void Select(bool select)
		{
			if (select && !Selected)
				Hilight(true);

			else if (!select && Selected)
				Hilight(false);

			Selected = select;
		}

		#endregion

		#region collapse

		public bool Collapsed { get { return BeforeCollapse.HasValue; } }
		private Size? BeforeCollapse { get; set; } = null;

		public void ToggleCollapse()
		{
			var mainPanel = FindName(MainPanelElementName) as FrameworkElement;

			if (Collapsed)
			{
				mainPanel.Width = BeforeCollapse.Value.Width;
				mainPanel.Height = BeforeCollapse.Value.Height;

				BeforeCollapse = null;
			}
			else
			{
				BeforeCollapse = new Size(mainPanel.ActualWidth, mainPanel.ActualHeight);
				mainPanel.Height = (GetElementByName(HeaderElementName) as FrameworkElement).Height;
			}

			UpdateConnections(new Point(Canvas.GetLeft(this), Canvas.GetTop(this)));
		}

		protected void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (e.ClickCount == 2)
					ToggleCollapse();
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
			}
		}

		#endregion

		private DependencyObject GetElementByName(string name)
		{
			var mainPanel = FindName(MainPanelElementName) as DependencyObject;
			if (mainPanel == null)
				return null;

			return LogicalTreeHelper.FindLogicalNode(mainPanel, name) as DependencyObject;
		}
	}
}
