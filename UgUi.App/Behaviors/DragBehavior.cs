using System.Windows;

namespace Ujeby.UgUi.Behaviors
{
	//public class DragBehavior : Behavior<FrameworkElement>
	//{
	//	private Point LastDragPosition;

	//	protected override void OnAttached()
	//	{
	//		var editorWindow = Application.Current.MainWindow as EditorWindow;

	//		AssociatedObject.MouseLeftButtonDown += (sender, e) =>
	//		{
	//			LastDragPosition = e.GetPosition(editorWindow);
	//			AssociatedObject.CaptureMouse();
	//		};

	//		AssociatedObject.MouseLeave += (sender, e) =>
	//		{
	//			AssociatedObject.ReleaseMouseCapture();
	//		};

	//		AssociatedObject.MouseUp += (sender, e) =>
	//		{
	//			AssociatedObject.ReleaseMouseCapture();
	//		};

	//		AssociatedObject.MouseMove += (sender, e) =>
	//		{
	//			if (AssociatedObject.IsMouseCaptured)
	//			{
	//				var element = (sender as FrameworkElement).Parent as FrameworkElement;
	//				while (element != null && !(element is NodeControl))
	//					element = element.Parent as FrameworkElement;

	//				if (element as NodeControl != null)
	//				{
	//					editorWindow.MoveControls(element as NodeControl, e.GetPosition(editorWindow) - LastDragPosition);
	//					LastDragPosition = e.GetPosition(editorWindow);
	//				}
	//			}
	//		};
	//	}
	//}
}
