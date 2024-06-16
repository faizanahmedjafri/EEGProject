using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace biopot.Controls
{
	[ContentProperty("Conditions")]
	public class StateContainer : ContentView
	{
		private FadeOutAnimation _disappearingAnimation;
		private FadeInAnimation _appearingAnimation;

		public List<StateCondition> Conditions { get; set; } = new List<StateCondition>();

		public static readonly BindableProperty StateProperty = BindableProperty.Create<StateContainer, object>(x => x.State, null, propertyChanged: StateChanged);

		public static void Init()
		{
			//for linker
		}
		public StateContainer()
		{
			_disappearingAnimation = new FadeOutAnimation();
			_appearingAnimation = new FadeInAnimation();
		}

		private static void StateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (newValue == null) return;

			var parent = bindable as StateContainer;
			parent?.ChooseStateProperty(newValue);
		}

		public object State
		{
			get { return this.GetValue(StateProperty); }
			set { this.SetValue(StateProperty, value); }
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			base.LayoutChildren(x, y, width, height);
			ChooseStateProperty(State);
		}

		private void ChooseStateProperty(object newValue)
		{
			if (newValue == null)
			{
				return;
			}

			foreach (StateCondition stateCondition in Conditions)
			{
				if (stateCondition.Is != null)
				{
					var splitIs = stateCondition.Is.ToString().Split(',');
					foreach (var conditionStr in splitIs)
					{
						if (conditionStr.Equals(newValue.ToString()))
						{
							if (this.Content != null)
							{
								stateCondition.Disappearing = _disappearingAnimation;
								stateCondition.Disappearing?.Apply(this.Content);
							}
							this.Content = stateCondition.Content;
							stateCondition.Appearing = _appearingAnimation;
							stateCondition.Appearing?.Apply(this.Content);
						}
					}
				}
				else if (stateCondition.IsNot != null)
				{
					if (!stateCondition.IsNot.ToString().Equals(newValue.ToString()))
					{
						this.Content = stateCondition.Content;
					}
				}
			}
		}
	}
}