using System;
using System.Collections.Generic;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.Content;
using biopot.Services;

namespace biopot.Droid.Services
{
    /// <summary>
    /// The class represents Android-specific implementation of a permission requesting.
    /// </summary>
    public class DroidPermissionsRequester : IPermissionsRequester
    {
        private readonly Context iContext;

        /// <summary>
        /// PermissionType to Android Specific Permission Map
        /// </summary>
        private readonly IDictionary<PermissionType, string> _permissionsMapping =
            new Dictionary<PermissionType, string>
            {
                {PermissionType.Camera, Manifest.Permission.Camera},
            };

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public DroidPermissionsRequester(Context aContext)
        {
            iContext = aContext;
        }

        /// <summary>
        /// From IPermissionsRequester
        /// </summary>
        public bool IsPermissionGranted(params PermissionType[] aPermissionTypes)
        {
            bool granted = true;
            foreach (var permissionType in aPermissionTypes)
            {
                switch (permissionType)
                {
                    case PermissionType.Camera:
                    {
                        var permissionName = _permissionsMapping[permissionType];
                        var permission = ContextCompat.CheckSelfPermission(iContext, permissionName);
                        granted &= permission == Permission.Granted;
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(permissionType), permissionType, null);
                }
            }

            return granted;
        }
    }
}