using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace biopot.Extensions
{
	public static class XamarinFormsExtensions
	{
		public static T GetInternalField<T>(this BindableObject element, string propertyKeyName) where T : class
		{
			// reflection stinks, but hey, what can you do?
			var pi = element.GetType().GetField(propertyKeyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			var key = (T)pi?.GetValue(element);

			return key;
		}

        /// <summary>
        ///     Executes a command with a parameter, if it allows.
        /// </summary>
        /// <param name="aCommand">The command to execute.</param>
        /// <param name="aParameter">The parameter to pass. It can be <c>null</c>.</param>
        public static void ExecuteIfCan(this ICommand aCommand, object aParameter = null)
        {
            if (aCommand.CanExecute(aParameter))
            {
                aCommand.Execute(aParameter);
            }
        }
    }
}
