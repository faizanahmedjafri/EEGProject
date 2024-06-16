using System;
using Xamarin.Forms;

namespace biopot.Extensions
{
	public static class VisualElementExtensions
	{
		public static ViewPositionInfoModel GetPositionOnScreenAndSize(this VisualElement view)
		{
			// Initialize with the view's "local" coordinates with respect to its parent
			double screenCoordinateX = view.X;
			double screenCoordinateY = view.Y;
			Rectangle viewBounds = view.Bounds;
			Rectangle screenBounds = new Rectangle();

			// Get the view's parent (if it has one...)
			if (view.Parent.GetType() != typeof(App))
			{
				VisualElement parent = (VisualElement)view.Parent;


				// Loop through all parents
				while (parent != null)
				{
					// Add in the coordinates of the parent with respect to ITS parent
					screenCoordinateX += parent.X;
					screenCoordinateY += parent.Y;
					screenBounds = parent.Bounds;

					// If the parent of this parent isn't the app itself, get the parent's parent.
					if (parent.Parent.GetType() == typeof(App))
						parent = null;
					else
						parent = (VisualElement)parent.Parent;
				}
			}

			// Return the final coordinates...which are the global SCREEN coordinates of the view
			return new ViewPositionInfoModel()
			{
				X = screenCoordinateX,
				Y = screenCoordinateY,
				ViewBounds = viewBounds,
				ScreenBounds = screenBounds
			};
		}

		public class ViewPositionInfoModel
		{
			public double X { get; set; }
			public double Y { get; set; }
			public Rectangle ViewBounds { get; set; }
			public Rectangle ScreenBounds { get; set; }
		}
	}
}
