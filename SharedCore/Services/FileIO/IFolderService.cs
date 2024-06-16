using SharedCore.Enums;
using System;

namespace SharedCore.Services
{
    public interface IFolderService
    {
        string GetFolderPath(ESaveDataWays way, string innerFolder);

        double GetAvailableSpaceMb(ESaveDataWays way);

        bool HasPermissions();
    }
}