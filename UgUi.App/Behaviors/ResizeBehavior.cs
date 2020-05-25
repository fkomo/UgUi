using System.Windows;
using System.Windows.Controls;
using Ujeby.UgUi.Controls;

namespace Ujeby.UgUi.Behaviors
{
	//public class ResizeBehavior : Behavior<FrameworkElement>
	//{
	//	private Point LastResizePosition;

	//	protected override void OnAttached()
	//	{
	//		var editorWindow = Application.Current.MainWindow as EditorWindow;
			
	//		AssociatedObject.MouseMove += (sender, e) =>
	//		{
	//			if (AssociatedObject.IsMouseCaptured)
	//			{
	//				var element = sender as FrameworkElement;
	//				var mainElement = element.Parent as FrameworkElement;

	//				var delta = e.GetPosition(editorWindow) - LastResizePosition;

	//				if (mainElement.Width + delta.X > mainElement.MinWidth)
	//				{
	//					mainElement.Width += delta.X;
	//					LastResizePosition.X = e.GetPosition(editorWindow).X;
	//				}

	//				if (mainElement.Height + delta.Y > mainElement.MinWidth + 28 + 26)
	//				{
	//					mainElement.Height += delta.Y;
	//					LastResizePosition.Y = e.GetPosition(editorWindow).Y;
	//				}

	//				var elementControl = (element as FrameworkElement).Parent as FrameworkElement;
	//				while (elementControl != null && !(elementControl is NodeControl))
	//					elementControl = elementControl.Parent as FrameworkElement;

	//				(elementControl as NodeControl)?.UpdateConnections(new Point(Canvas.GetLeft(elementControl), Canvas.GetTop(elementControl)));
	//			}
	//		};

	//		AssociatedObject.MouseLeave += (sender, e) =>
	//		{
	//			AssociatedObject.ReleaseMouseCapture();
	//		};

	//		AssociatedObject.MouseUp += (sender, e) =>
	//		{
	//			AssociatedObject.ReleaseMouseCapture();
	//		};

	//		AssociatedObject.MouseDown += (sender, e) =>
	//		{
	//			if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
	//			{
	//				LastResizePosition = e.GetPosition(editorWindow);
	//				AssociatedObject.CaptureMouse();
	//			}
	//		};
	//	}
	//}
}
