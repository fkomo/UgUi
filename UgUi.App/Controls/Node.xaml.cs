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
using System.Windows.Media.Imaging;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Controls
{
	/// <summary>
	/// Interaction logic for NodeControl.xaml
	/// </summary>
	public partial class Node : UserControl
	{
		// TODO UI NODE arrow icon/symbol on left side of header - toggle collapse action
		// TODO UI NODE resizable nodes (textbox input, bitmap, ...) - ResizeBehavior ?

		// Create a custom routed event by first registering a RoutedEventID
		// This event uses the bubbling routing strategy
		public static readonly RoutedEvent ImpulseEvent = EventManager.RegisterRoutedEvent("Impulse", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Node));

		// Provide CLR accessors for the event
		public event RoutedEventHandler Impulse
		{
			add { AddHandler(ImpulseEvent, value); }
			remove { RemoveHandler(ImpulseEvent, value); }
		}

		// This method raises the Tap event
		void RaiseImpulseEvent()
		{
			var newEventArgs = new RoutedEventArgs(Node.ImpulseEvent);
			RaiseEvent(newEventArgs);
		}

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
		public const string DefaultOutputAnchorName = "OutputAnchorArray";

		public const string HeaderElementName = "NodeHeader";
		public const string HeaderTitleElementName = "NodeTitle";

		public const int AnchorSize = 16;
		public const int DecimalPrecision = 4;

		public const byte ControlTransparency = 0xee;

		const int LOG_MAX_INPUT_LENGTH = 64;
		const int LOG_MAX_OUTPUT_LENGTH = 64;

		public static Color InputBorder = Color.FromArgb(ControlTransparency, 0x20, 0x20, 0x20);
		public static Color InputBackground = Color.FromArgb(0xff, 0x30, 0x30, 0x30);
		public static Color TextForeground = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
		public static Color TextForegroundHilighted = Colors.White;
		public static Effect LabelEffect = new DropShadowEffect { BlurRadius = 4, Color = Colors.Black, ShadowDepth = 0 };

		#endregion

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
			CustomNodeName.Visibility = Visibility.Collapsed;

			MainPanel.DataContext = NodeInstance;

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

			// add rows for all input properties
			for (var i = 0; i < inputProperties.Count(); i++)
				MainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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
				MainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			MainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(AnchorSize / 2) });    // bottom

			#endregion

			#region header

			CustomName = AttributeHelper.GetValue<NodeInfoAttribute, string>(NodeInstance.GetType(), nameof(NodeInfoAttribute.DisplayName)) ?? NodeInstance.GetType().Name;
			var colorValues = BitConverter.GetBytes(CustomName.GetHashCode());
			CustomColor = Color.FromArgb(0xff, colorValues[1], colorValues[2], colorValues[3]);

			NodeHeader.Background = new LinearGradientBrush(CustomColor, Color.Multiply(CustomColor, (float)0.5), new Point(0, 1), new Point(1, 0));
			NodeTitle.Text = CustomName;

			#endregion

			#region border

			var border = new Border
			{
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1, 0, 1, 1)
			};
			Grid.SetRow(border, 1);
			Grid.SetColumnSpan(border, MainPanel.ColumnDefinitions.Count);
			Grid.SetRowSpan(border, MainPanel.RowDefinitions.Count - 1);
			MainPanel.Children.Add(border);

			#endregion

			var firstRow = 2;
			var inputRows = new bool[MainPanel.RowDefinitions.Count - 3];
			var outputRows = new bool[MainPanel.RowDefinitions.Count - 3];

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
					MainPanel.Children.Add(anchor);

					#endregion
				}

				#region label

				var label = new Label
				{
					Foreground = new SolidColorBrush(TextForeground),
					VerticalAlignment = VerticalAlignment.Center,
					Name = LabelPrefix + inputAnchorProperty,
					FontFamily = new FontFamily("Consolas"),
					Effect = LabelEffect,
					Content = inputDisplayName,
				};

				Grid.SetRow(label, row);
				Grid.SetColumn(label, 1);
				if (inputAnchor && anchorOnly && outputAnchor)
				{
					label.HorizontalAlignment = HorizontalAlignment.Right;
					Grid.SetColumnSpan(label, 3);
				}
				else
					label.HorizontalAlignment = HorizontalAlignment.Left;
				MainPanel.Children.Add(label);

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
						MainPanel.Children.Add(colorElement);
					}
					// TODO UI INPUT CustomCheckBox
					//else if (inputProperty.PropertyType == typeof(bool))
					//{
					//	var checkBox = new CheckBox
					//	{
					//		//Background = new SolidColorBrush(InputBackground),
					//		//Foreground = new SolidColorBrush(TextForeground),
					//		BorderThickness = new Thickness(1),
					//		Name = InputValuePrefix + inputName,
					//		FontFamily = new FontFamily("Consolas"),
					//		HorizontalContentAlignment = HorizontalAlignment.Center,
					//		VerticalContentAlignment = VerticalAlignment.Center,
					//		VerticalAlignment = VerticalAlignment.Center,
					//		HorizontalAlignment = HorizontalAlignment.Center,
					//	};

					//	var binding = new Binding(inputProperty.Name)
					//	{
					//		Mode = BindingMode.OneWay,
					//	};
					//	checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);

					//	Grid.SetRow(checkBox, row);
					//	Grid.SetColumn(checkBox, 2);
					//	Grid.SetColumnSpan(checkBox, mainPanel.ColumnDefinitions.Count - 1);
					//	mainPanel.Children.Add(checkBox);
					//}
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
						MainPanel.Children.Add(textbox);
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
					Grid.SetColumn(anchor, MainPanel.ColumnDefinitions.Count - 1);
					MainPanel.Children.Add(anchor);

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
						var imageBorder = new Border
						{
							BorderThickness = new Thickness(1),
							BorderBrush = new SolidColorBrush(InputBorder),
						};
						var image = new Image();
						imageBorder.Child = image;

						image.SetBinding(Image.SourceProperty, new Binding(outputProperty.Name) { Mode = BindingMode.OneWay });

						var widthBinding = AttributeHelper.GetValue<ImageBindingsAttribute, string>(outputProperty, nameof(ImageBindingsAttribute.Width));
						var heightBinding = AttributeHelper.GetValue<ImageBindingsAttribute, string>(outputProperty, nameof(ImageBindingsAttribute.Height));
						if (!string.IsNullOrEmpty(widthBinding) && !string.IsNullOrEmpty(heightBinding))
						{
							image.SetBinding(Image.WidthProperty, new Binding(widthBinding) { Mode = BindingMode.OneWay });
							image.SetBinding(Image.HeightProperty, new Binding(heightBinding) { Mode = BindingMode.OneWay });
						}

						Grid.SetRow(imageBorder, row);
						Grid.SetColumn(imageBorder, 1);
						Grid.SetColumnSpan(imageBorder, MainPanel.ColumnDefinitions.Count - 2);
						MainPanel.Children.Add(imageBorder);
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
						MainPanel.Children.Add(textbox);
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
					Effect = LabelEffect,
					Content = outputDisplayName,
				};

				Grid.SetRow(label, row);
				if (anchorOnly)
				{
					Grid.SetColumn(label, 0);
					Grid.SetColumnSpan(label, MainPanel.ColumnDefinitions.Count - 1);
				}
				else
					Grid.SetColumn(label, MainPanel.ColumnDefinitions.Count - 2);

				MainPanel.Children.Add(label);

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
					Grid.SetColumn(anchor, MainPanel.ColumnDefinitions.Count - 1);
					MainPanel.Children.Add(anchor);

					#endregion
				}

				outputRows[row - firstRow] = true;
			}

			//MainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
			//var customDouble = new CustomNumberInput();
			//Grid.SetRow(customDouble, MainPanel.RowDefinitions.Count - 1);
			//Grid.SetColumn(customDouble, 1);
			//Grid.SetColumnSpan(customDouble, 2);
			//MainPanel.Children.Add(customDouble);

			//MainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = 30 });
			//var customText = new CustomTextInput();
			//Grid.SetRow(customText, MainPanel.RowDefinitions.Count - 1);
			//Grid.SetColumn(customText, 1);
			//Grid.SetColumnSpan(customText, 2);
			//MainPanel.Children.Add(customText);
		}

		internal void SetCustomName(string name)
		{
			CustomNodeName.Text = name;

			if (string.IsNullOrEmpty(CustomNodeName.Text))
				CustomNodeName.Visibility = Visibility.Collapsed;
			else
				CustomNodeName.Visibility = Visibility.Visible;
		}

		internal void RenameBegin()
		{
			CustomNodeName.Visibility = Visibility.Visible;
			CustomNodeName.IsEnabled = true;
			CustomNodeName.Focus();
			CustomNodeName.SelectionStart = 0;
		}

		private void CustomNodeName_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				CustomNodeName.IsEnabled = false;
				SetCustomName(CustomNodeName.Text);
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
			if (LastTransactionId == transactionId || (ConnectionsFrom.Count != 0 && ConnectionsFrom.All(c => c.TransactionId == transactionId)))
				return LastTransactionId;

			try
			{
				var stopWatch = new Stopwatch();

				foreach (var connection in ConnectionsFrom)
				{
					if (connection.TransactionId == transactionId)
						continue;

					// generate output from left node
					(connection.Left as Node).Execute(transactionId);

					// get left output value
					var leftNode = connection.Left.NodeInstance;
					var leftValue = leftNode
						.GetType()
						.GetProperty(connection.LeftPropertyName)
						.GetValue(leftNode);

					// set right input value
					var rightProperty = NodeInstance?.GetType().GetProperty(connection.RightPropertyName);
					if (rightProperty != null)
						rightProperty.SetValue(NodeInstance, Common.Tools.Convert.ChangeType(leftValue, rightProperty.PropertyType));

					connection.TransactionId = transactionId;
				}

				if (NodeInstance != null && LastTransactionId != transactionId)
				{
					try
					{
						stopWatch.Reset();
						stopWatch.Start();

						NodeInstance.Execute();
						RaiseImpulseEvent();

						stopWatch.Stop();

						// log node input & output	
						var nodeName = AttributeHelper.GetValue<NodeInfoAttribute, string>(NodeInstance.GetType(), nameof(NodeInfoAttribute.DisplayName)) ?? NodeInstance.GetType().Name;
						var nodeInputs = (NodeInstance as ILoggable)?.GetInputs()
							?.Select(i => i == null ? null : (i.Length < LOG_MAX_INPUT_LENGTH ? i : (i.Substring(0, LOG_MAX_INPUT_LENGTH) + "...")).Replace(Environment.NewLine, "\\r\\n"));
						var nodeOutputs = (NodeInstance as ILoggable)?.GetOutputs()
							?.Select(i => i == null ? null : (i.Length < LOG_MAX_OUTPUT_LENGTH ? i : (i.Substring(0, LOG_MAX_OUTPUT_LENGTH) + "...")).Replace(Environment.NewLine, "\\r\\n"));
						Log.WriteLine($"{ CustomNodeName.Text }:{ nodeName }('{ string.Join("', '", nodeInputs ?? new string[] { }) }')=['{ string.Join("', '", nodeOutputs ?? new string[] { }) }'] | { stopWatch.ElapsedMilliseconds }ms");

						LastTransactionId = transactionId;
					}
					catch (Exception ex)
					{
						Log.WriteLine(ex.ToString());
					}
				}

				foreach (var outputConnection in ConnectionsTo)
					outputConnection.Right.Execute(transactionId);
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
				return new Point(anchorName.StartsWith(InputAnchorPrefix) ? 0 : NodeHeader.ActualWidth, NodeHeader.ActualHeight / 2);
			}
			else
			{
				var anchor = GetElementByName(anchorName) as FrameworkElement;
				if (anchor == null)
					throw new FriendlyException($"Anchor:{ anchorName } not found");

				var anchorPosition = anchor.TranslatePoint(new Point(), this);
				anchorPosition.X += anchor.ActualWidth * 0.5;
				anchorPosition.Y += anchor.ActualHeight * 0.5;

				var scaleX = (this.RenderTransform as ScaleTransform)?.ScaleX;
				var scaleY = (this.RenderTransform as ScaleTransform)?.ScaleY;
				if (scaleX.HasValue && scaleY.HasValue)
				{
					anchorPosition.X *= scaleX.Value;
					anchorPosition.Y *= scaleY.Value;
				}

				return anchorPosition;
			}
		}

		#region connectivity

		public virtual void AddConnectionTo(Connection connection)
		{
			ConnectionsTo.Add(connection);
		}

		public virtual void AddConnectionFrom(Connection connection, bool execute = true)
		{
			ConnectionsFrom.Add(connection);
			EnableInputElement(connection.RightAnchorName, false);

			if (execute)
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
			if (Collapsed)
				topLeft.Y += CustomNodeName.ActualHeight;

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
			if (/*Mouse.LeftButton == MouseButtonState.Pressed || */Selected)
				return;

			Hilight(true);
		}

		protected void ControlMouseLeave(object sender, MouseEventArgs e)
		{
			if (/*Mouse.LeftButton == MouseButtonState.Pressed || */Selected)
				return;

			Hilight(false);
		}

		public void Hilight(bool hilight)
		{
			if (hilight && !Hilighted)
			{
				(MainPanel.Effect as DropShadowEffect).Color = Color.Multiply(CustomColor, (float)0.8);
				NodeTitle.Foreground = new SolidColorBrush(Colors.White);
			}
			else if (!hilight && Hilighted)
			{
				(MainPanel.Effect as DropShadowEffect).Color = Colors.Black;
				NodeTitle.Foreground = new SolidColorBrush(Colors.Black);
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

				Cursor = Cursors.Hand;
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

			Cursor = Cursors.Arrow;
		}

		protected void HilightOutputAnchorOn(object sender, MouseEventArgs e)
		{
			if (Workspace.NewConnection != null)
				return;

			(sender as Rectangle).Fill = new SolidColorBrush(Colors.White);

			var label = GetElementByName((sender as Rectangle).Name.Replace(OutputAnchorPrefix, LabelPrefix)) as Label;
			if (label != null)
				label.Foreground = new SolidColorBrush(Colors.White);

			Cursor = Cursors.Hand;
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

			Cursor = Cursors.Arrow;
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

		public bool Collapsed { get { return SizeBeforeCollapse.HasValue; } }
		private Size? SizeBeforeCollapse { get; set; } = null;

		public void ToggleCollapse()
		{
			if (Collapsed)
			{
				MainPanel.Width = SizeBeforeCollapse.Value.Width;
				MainPanel.Height = SizeBeforeCollapse.Value.Height;

				SizeBeforeCollapse = null;
			}
			else
			{
				SizeBeforeCollapse = new Size(MainPanel.ActualWidth, MainPanel.ActualHeight);
				MainPanel.Height = NodeHeader.ActualHeight;
			}

			// TODO BUG input elements inside node are resized after node is collapsed

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
			return LogicalTreeHelper.FindLogicalNode(MainPanel, name) as DependencyObject;
		}
	}
}
