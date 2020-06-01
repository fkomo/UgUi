using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Ujeby.UgUi.Controls
{
	public class Connection
	{
		public const int PathThickness = 2;
		public const int BlurRadius = 4;

		private List<UIElement> UIElements { get; set; } = new List<UIElement>();

		public string LeftAnchorName { get; set; }
		public string LeftPropertyName { get { return LeftAnchorName?.Replace(Node.OutputAnchorPrefix, string.Empty); } }

		public string RightAnchorName { get; set; }
		public string RightPropertyName { get { return RightAnchorName?.Replace(Node.InputAnchorPrefix, string.Empty); } }

		public Node Left { get; set; }
		public Node Right { get; set; }

		private Color Color { get { return Left.CustomColor; } }

		public Connection(Node left, Node right, string leftAnchorName, string rightAnchorName)
		{
			Left = left;
			Right = right;

			LeftAnchorName = leftAnchorName;
			RightAnchorName = rightAnchorName;

			CreateShape(Color);
        }

		public override string ToString()
		{
			return $"Connection({Left?.Name}#{LeftAnchorName}({LeftPropertyName}) -> {Right?.Name}#{RightAnchorName}({RightPropertyName}))";
		}

		private void CreateShape(Color color)
		{
			UIElements.Clear();

			var newElement1 = new Path()
			{
				Stroke = new SolidColorBrush(color),
				StrokeThickness = PathThickness,
				Data = new PathGeometry(),
				Effect = new DropShadowEffect()
				{
					Color = color,
					Direction = 0,
					BlurRadius = BlurRadius,
                    ShadowDepth = 0
				}
			};
			UIElements.Add(newElement1);

			// TODO UI multiple lines between nodes
			//var newElement2 = new Path()
			//{
			//	Stroke = new SolidColorBrush(color),
			//	StrokeThickness = PathThickness,
			//	Data = new PathGeometry(),
			//	Effect = new DropShadowEffect()
			//	{
			//		Color = color,
			//		Direction = 0,
			//		BlurRadius = BlurRadius,
			//		ShadowDepth = 0
			//	}
			//};
			//UIElements.Add(newElement2);

			//var newElement3 = new Path()
			//{
			//	Stroke = new SolidColorBrush(color),
			//	StrokeThickness = PathThickness,
			//	Data = new PathGeometry(),
			//	Effect = new DropShadowEffect()
			//	{
			//		Color = color,
			//		Direction = 0,
			//		BlurRadius = BlurRadius,
			//		ShadowDepth = 0
			//	}
			//};
			//UIElements.Add(newElement3);

			//var newElement4 = new Path()
			//{
			//	Stroke = new SolidColorBrush(color),
			//	StrokeThickness = PathThickness,
			//	Data = new PathGeometry(),
			//	Effect = new DropShadowEffect()
			//	{
			//		Color = color,
			//		Direction = 0,
			//		BlurRadius = BlurRadius,
			//		ShadowDepth = 0
			//	}
			//};
			//UIElements.Add(newElement4);
		}

		public void Update(Point? from, Point? to/*, int fromCount, int toCount*/)
		{
			if (!from.HasValue && !to.HasValue)
				return;

			switch (UIElements.Count)
			{
				case 1:
					{
						UpdateBezierLine(UIElements.Single() as Path, from, to);
					}
					break;

				case 2:
					{
						var diff = Node.AnchorSize / 2.0 - Node.AnchorSize / 3.0;

						var from1 = from;
						var from2 = from;
						var to1 = to;
						var to2 = to;

						if (from.HasValue)
						{
							from1 = new Point(from.Value.X, from.Value.Y - diff);
							from2 = new Point(from.Value.X, from.Value.Y + diff);
						}

						if (to.HasValue)
						{
							to1 = new Point(to.Value.X, to.Value.Y - diff);
							to2 = new Point(to.Value.X, to.Value.Y + diff);
						}

						UpdateBezierLine(UIElements[0] as Path, from1, to1);
						UpdateBezierLine(UIElements[1] as Path, from2, to2);
					}
					break;

				case 3:
					{
						var diff = Node.AnchorSize / 4.0;

						var from1 = from;
						var from2 = from;
						var to1 = to;
						var to2 = to;

						if (from.HasValue)
						{
							from1 = new Point(from.Value.X, from.Value.Y - diff);
							from2 = new Point(from.Value.X, from.Value.Y + diff);
						}

						if (to.HasValue)
						{
							to1 = new Point(to.Value.X, to.Value.Y - diff);
							to2 = new Point(to.Value.X, to.Value.Y + diff);
						}

						UpdateBezierLine(UIElements[0] as Path, from1, to1);
						UpdateBezierLine(UIElements[1] as Path, from, to);
						UpdateBezierLine(UIElements[2] as Path, from2, to2);
					}
					break;

				case 4:
					{
						var diff = Node.AnchorSize / 8.0;

						var from1 = from;
						var from2 = from;
						var from3 = from;
						var from4 = from;
						var to1 = to;
						var to2 = to;
						var to3 = to;
						var to4 = to;

						if (from.HasValue)
						{
							from1 = new Point(from.Value.X, from.Value.Y - 3 * diff);
							from2 = new Point(from.Value.X, from.Value.Y - diff);
							from3 = new Point(from.Value.X, from.Value.Y + diff);
							from4 = new Point(from.Value.X, from.Value.Y + 3 * diff);
						}

						if (to.HasValue)
						{
							to1 = new Point(to.Value.X, to.Value.Y - 3 * diff);
							to2 = new Point(to.Value.X, to.Value.Y - diff);
							to3 = new Point(to.Value.X, to.Value.Y + diff);
							to4 = new Point(to.Value.X, to.Value.Y + 3 * diff);
						}

						UpdateBezierLine(UIElements[0] as Path, from1, to1);
						UpdateBezierLine(UIElements[1] as Path, from2, to2);
						UpdateBezierLine(UIElements[2] as Path, from3, to3);
						UpdateBezierLine(UIElements[3] as Path, from4, to4);
					}
					break;

				default:
					throw new NotImplementedException($"UIElements.Count: { UIElements.Count }");
			}
		}

		public void UpdateBezierLine(Path uiElement, Point? from, Point? to)
		{
			var pathGeometry = uiElement.Data as PathGeometry;
			var pathFigure = pathGeometry.Figures.SingleOrDefault();

			from = from ?? pathFigure?.StartPoint;
			if (!from.HasValue)
				return;

			to = to ?? (pathFigure?.Segments.FirstOrDefault(s => s is BezierSegment) as BezierSegment)?.Point3;
			if (!to.HasValue)
				return;

			pathGeometry.Figures.Clear();

			//if (from.Value.X > to.Value.X)
			//{
			//	var diff = new Point(Math.Abs((to.Value.X - from.Value.X) / 5.0), Math.Abs((to.Value.Y - from.Value.Y) / 5.0));
			//	var yCenter = (to.Value.Y + from.Value.Y) / 2;

			//	var pathSegments = new PathSegment[]
			//	{
			//		new BezierSegment(new Point(from.Value.X + diff.X, from.Value.Y + diff.Y), new Point(from.Value.X + diff.X, yCenter - diff.Y), new Point(from.Value.X, yCenter), true),
			//		new BezierSegment(new Point(from.Value.X - diff.X, yCenter + diff.Y), new Point(to.Value.X + diff.X, yCenter - diff.Y), new Point(to.Value.X, yCenter), true),
			//		new BezierSegment(new Point(to.Value.X - diff.X, yCenter + diff.Y), new Point(to.Value.X - diff.X, to.Value.Y - diff.Y), to.Value, true),
			//	};

			//	pathGeometry.Figures.Add(new PathFigure(from.Value, pathSegments, false));
			//}
			//else
			{
				var b1 = new Point((from.Value.X + to.Value.X) / 2, from.Value.Y);
				var b2 = new Point((from.Value.X + to.Value.X) / 2, to.Value.Y);

				pathGeometry.Figures.Add(new PathFigure(from.Value, new PathSegment[] { new BezierSegment(b1, b2, to.Value, true) }, false));
			}
		}

		internal void AddToUICollection(UIElementCollection collection)
		{
			foreach (var uiElement in UIElements)
			{
				var index = collection.Add(uiElement);
				Canvas.SetZIndex(uiElement, -index);
			}
		}

		internal void RemoveFromUICollection(UIElementCollection collection)
		{
			foreach (var uiElement in UIElements)
				collection.Remove(uiElement);

			UIElements.Clear();
		}

		internal void Hilight(Color color)
		{
			foreach (var uiElement in UIElements)
			{
				if ((uiElement as Path).Stroke == null)
					(uiElement as Path).Stroke = new SolidColorBrush(color);
				else
					((uiElement as Path).Stroke as SolidColorBrush).Color = color;

				if (uiElement.Effect == null)
					uiElement.Effect = new DropShadowEffect()
					{
						Color = color,
						Direction = 0,
						BlurRadius = 4,
						ShadowDepth = 0
					};
				else
					(uiElement.Effect as DropShadowEffect).Color = color;
			}
		}

		internal bool HasUIElement(UIElement uiElement)
		{
			return UIElements.Any(e => e == uiElement);
		}

		internal void Scale(double scale, Point center)
		{
			// TODO UI SCALE connections - or maybe just update start/end point and make line thinner ?

			foreach (var uiElement in UIElements)
			{
				var pathFigure = ((uiElement as Path).Data as PathGeometry).Figures.Single();

				var start = pathFigure.StartPoint;
				var end = (pathFigure.Segments.First(s => s is BezierSegment) as BezierSegment).Point3;
				var midPoint = new Point((start.X + end.X) * 0.5, (start.Y + end.Y) * 0.5);

				uiElement.RenderTransform = new ScaleTransform(scale, scale, center.X, center.Y);
			}
		}
	}
}
