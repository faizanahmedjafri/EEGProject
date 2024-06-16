using System;
using System.Linq;
using Xamarin.Forms;

namespace biopot.Controls
{
	/// <summary>
	/// Stack layout ex.
	/// </summary>
	internal class StackLayoutEx : StackLayout
	{
		/// <summary>
		/// Make sure we lay out so that we only use as much (or little) space as necessary for 
		/// each item
		/// </summary>
		/// <remarks>Implementors wishing to change the default behavior of a Layout should override this method. It is suggested to
		/// still call the base method and modify its calculated results.</remarks>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			base.LayoutChildren(x, y, width, height);

			var total = Children.Sum(t => t.Width);
			var parentWidth = (Parent as View).Width;

			if (total < parentWidth)
			{

				// We need more space
				var diff = (parentWidth - total) / Children.Count;

				var xoffset = 0.0;
				foreach (var child in Children)
				{
					child.Layout(new Rectangle(child.X + xoffset, child.Y, child.Width + diff, child.Height));
					xoffset += diff;
				}
			}
		}
	}

}
