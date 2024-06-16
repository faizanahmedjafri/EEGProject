using System;
using Xamarin.Forms;

namespace biopot.Controls
{
	[ContentProperty("Content")]
	public class StateCondition
	{
		public object Is { get; set; }
		public object IsNot { get; set; }
		public View Content { get; set; }

		public AnimationBase Appearing { get; set; }
		public AnimationBase Disappearing { get; set; }
	}

	public abstract class AnimationBase
	{
		public abstract void Apply(View view);
	}

	public class FadeOutAnimation : AnimationBase
	{
		public override void Apply(View view)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				view.FadeTo(0, 1);
			});
		}
	}

	public class FadeInAnimation : AnimationBase
	{
		public override void Apply(View view)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				view.FadeTo(1);
			});
		}
	}
}