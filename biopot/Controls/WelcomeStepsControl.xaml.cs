using System.Windows.Input;
using Xamarin.Forms;
using biopot.Enums;
using biopot.Extensions;
using biopot.Helpers;

namespace biopot.Controls
{
	public partial class WelcomeStepsControl : ContentView
	{
		public WelcomeStepsControl()
		{
			InitializeComponent();
		}

		public static readonly BindableProperty CurrentStepProperty =
			BindableProperty.Create(nameof(CurrentStep), typeof(EWelcomeSteps), typeof(WelcomeStepsControl), defaultValue: default(EWelcomeSteps),
			propertyChanged: OnCurrentStepChanged);

		public EWelcomeSteps CurrentStep
		{
		    get => (EWelcomeSteps)GetValue(CurrentStepProperty);
		    set
		    {
		        SetValue(CurrentStepProperty, value);
               
		    }
		}

	    public static readonly BindableProperty StepClickedCommandProperty =
	        BindableProperty.Create(nameof(StepClickedCommand), typeof(ICommand), typeof(WelcomeStepsControl));

	    public ICommand StepClickedCommand
        {
	        get => (ICommand)GetValue(StepClickedCommandProperty);
	        set => SetValue(StepClickedCommandProperty, value);
	    }

	    private ICommand _firstStepSelectedCommand;
	    public ICommand FirstStepSelectedCommand => _firstStepSelectedCommand ?? (_firstStepSelectedCommand = new Command(OnFirstStepSelectedCommand));

	    private ICommand _secondStepSelectedCommand;
	    public ICommand SecondStepSelectedCommand => _secondStepSelectedCommand ?? (_secondStepSelectedCommand = new Command(OnSecondStepSelectedCommand));

	    private ICommand _thirdStepSelectedCommand;
	    public ICommand ThirdStepSelectedCommand => _thirdStepSelectedCommand ?? (_thirdStepSelectedCommand = new Command(OnThirdStepSelectedCommand));

	    public bool IsFirstStepClickable => CurrentStep > EWelcomeSteps.First; 
	    public bool IsSecondStepClickable => CurrentStep > EWelcomeSteps.Second; 
	    public bool IsThirdStepClickable => CurrentStep > EWelcomeSteps.Third; 

        /// <summary>
        /// Handles first step selection.
        /// </summary>
	    private void OnFirstStepSelectedCommand()
	    {
	        StepClickedCommand?.ExecuteIfCan(1);
	    }

	    /// <summary>
	    /// Handles second step selection.
	    /// </summary>
        private void OnSecondStepSelectedCommand()
	    {
	        StepClickedCommand?.ExecuteIfCan(2);
	    }

	    /// <summary>
	    /// Handles third step selection.
	    /// </summary>
        private void OnThirdStepSelectedCommand()
	    {
	        StepClickedCommand?.ExecuteIfCan(3);
	    }

        /// <summary>
        /// Handles current step changes.
        /// </summary>
        /// <param name="aBindable"> The bindable control. </param>
        /// <param name="aOldValue"> The old value. </param>
        /// <param name="aNewValue"> The new value. </param>
        private static void OnCurrentStepChanged(BindableObject aBindable, object aOldValue, object aNewValue)
		{
            var control = (WelcomeStepsControl) aBindable;
		    EWelcomeSteps steps = (EWelcomeSteps) aNewValue;

		    control.OnPropertyChanged(nameof(IsFirstStepClickable));
		    control.OnPropertyChanged(nameof(IsSecondStepClickable));
		    control.OnPropertyChanged(nameof(IsThirdStepClickable));

            switch (steps)
		    {
                case EWelcomeSteps.First:
                    SetFirstStep(control);
                    break;
                case EWelcomeSteps.Second:
                    SetSecondStep(control);
                    break;
                case EWelcomeSteps.Third:
                    SetThirdStep(control);
                    break;
                default:
                    SetFinalStep(control);
                    break;
		    }
        }

        /// <summary>
        /// Sets first step as a current one.
        /// </summary>
        /// <param name="aControl"> The control. </param>
        private static void SetFirstStep(WelcomeStepsControl aControl)
        {
            var stepActiveColor = StyleManager.GetAppResource<Color>("stepsActiveColor");
            var stepNotActiveColor = StyleManager.GetAppResource<Color>("entrySeparatorColorDefault");

            aControl.CircleFirstStep.BackgroundColor = stepActiveColor;
            aControl.LabelFirstStep.IsVisible = true;
            aControl.ImageFirstStep.IsVisible = false;

            aControl.ConnectionLine1.BackgroundColor = stepNotActiveColor;
            aControl.ConnectionLine2.BackgroundColor = stepNotActiveColor;

            aControl.CircleSecondStep.BackgroundColor = stepNotActiveColor;
            aControl.LabelSecondStep.IsVisible = true;
            aControl.ImageSecondStep.IsVisible = false;

            aControl.CircleThirdStep.BackgroundColor = stepNotActiveColor;
            aControl.LabelThirdStep.IsVisible = true;
            aControl.ImageThirdStep.IsVisible = false;
        }

	    /// <summary>
	    /// Sets second step as a current one.
	    /// </summary>
	    /// <param name="aControl"> The control. </param>
        private static void SetSecondStep(WelcomeStepsControl aControl)
        {
            var stepActiveColor = StyleManager.GetAppResource<Color>("stepsActiveColor");
            var stepNotActiveColor = StyleManager.GetAppResource<Color>("entrySeparatorColorDefault");

            aControl.CircleFirstStep.BackgroundColor = stepActiveColor;
            aControl.LabelFirstStep.IsVisible = false;
            aControl.ImageFirstStep.IsVisible = true;

            aControl.ConnectionLine1.BackgroundColor = stepActiveColor;
            aControl.ConnectionLine2.BackgroundColor = stepNotActiveColor;

            aControl.CircleSecondStep.BackgroundColor = stepActiveColor;
            aControl.LabelSecondStep.IsVisible = true;
            aControl.ImageSecondStep.IsVisible = false;

            aControl.CircleThirdStep.BackgroundColor = stepNotActiveColor;
            aControl.LabelThirdStep.IsVisible = true;
            aControl.ImageThirdStep.IsVisible = false;
        }

	    /// <summary>
	    /// Sets third step as a current one.
	    /// </summary>
	    /// <param name="aControl"> The control. </param>
        private static void SetThirdStep(WelcomeStepsControl aControl)
        {
            var stepActiveColor = StyleManager.GetAppResource<Color>("stepsActiveColor");

            aControl.CircleFirstStep.BackgroundColor = stepActiveColor;
            aControl.LabelFirstStep.IsVisible = false;
            aControl.ImageFirstStep.IsVisible = true;

            aControl.ConnectionLine1.BackgroundColor = stepActiveColor;
            aControl.ConnectionLine2.BackgroundColor = stepActiveColor;

            aControl.CircleSecondStep.BackgroundColor = stepActiveColor;
            aControl.LabelSecondStep.IsVisible = false;
            aControl.ImageSecondStep.IsVisible = true;

            aControl.CircleThirdStep.BackgroundColor = stepActiveColor;
            aControl.LabelThirdStep.IsVisible = true;
            aControl.ImageThirdStep.IsVisible = false;
        }

	    /// <summary>
	    /// Sets all steps as passed.
	    /// </summary>
	    /// <param name="aControl"> The control. </param>
        private static void SetFinalStep(WelcomeStepsControl aControl)
        {
            var stepActiveColor = StyleManager.GetAppResource<Color>("stepsActiveColor");

            aControl.CircleFirstStep.BackgroundColor = stepActiveColor;
            aControl.LabelFirstStep.IsVisible = false;
            aControl.ImageFirstStep.IsVisible = true;

            aControl.ConnectionLine1.BackgroundColor = stepActiveColor;
            aControl.ConnectionLine2.BackgroundColor = stepActiveColor;

            aControl.CircleSecondStep.BackgroundColor = stepActiveColor;
            aControl.LabelSecondStep.IsVisible = false;
            aControl.ImageSecondStep.IsVisible = true;

            aControl.CircleThirdStep.BackgroundColor = stepActiveColor;
            aControl.LabelThirdStep.IsVisible = false;
            aControl.ImageThirdStep.IsVisible = true;
        }
    }
}
