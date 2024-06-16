using Xamarin.Forms;

namespace biopot.Effects
{
    /// <summary>
    ///     The class provides some common functionality and constants to support platform effects.
    /// </summary>
    public static class Effects
    {
        public const string ResolutionGroupName = "biopot";

        /// <summary>
        ///     Creates a full name for resolution and passing to base constructor of <see cref="RoutingEffect" />.
        /// </summary>
        /// <param name="effectName">The effect name, like <c>BouncingListViewEffect</c>.</param>
        /// <returns>Resolution name, like <c>Tangoe.BouncingListViewEffect</c></returns>
        public static string NameForResolution(string effectName)
        {
            return $"{ResolutionGroupName}.{effectName}";
        }

        /// <summary>
        ///     Creates a full name for resolution and passing to base constructor of <see cref="RoutingEffect" />,
        ///     based on the type and its name.
        /// </summary>
        /// <returns>Resolution name, like <c>Tangoe.BouncingListViewEffect</c></returns>
        public static string NameForResolution<T>() where T : RoutingEffect
        {
            return NameForResolution(Name<T>());
        }

        /// <summary>
        /// Creates a name of a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string Name<T>() where T : RoutingEffect
        {
            return typeof(T).Name;
        }
    }
}