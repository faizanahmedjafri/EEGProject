using Prism.Events;

namespace biopot.Services
{
    /// <summary>
    /// The interface provides methods for determining currently granted permissions and requesting them, if needed.
    /// </summary>
    public interface IPermissionsRequester
    {
        /// <summary>
        /// Checks whether a permission is currently granted or not.
        /// </summary>
        /// <param name="aPermissionTypes">The permission types to check.</param>
        /// <returns>true, if the permission is granted; otherwise, false.</returns>
        bool IsPermissionGranted(params PermissionType[] aPermissionTypes);
    }

    /// <summary>
    /// Supported permissions to request.
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// All important permissions, which are required for the app to function.
        /// </summary>
        AllImportant,

        /// <summary>
        /// Access to camera for barcode scanning.
        /// </summary>
        Camera,
    }

    /// <summary>
    /// The event is raised, when granted/revoked permissions have changed during application's lifetime.
    /// </summary>
    public class SystemPermissionsChangedEvent : PubSubEvent
    {
    }
}