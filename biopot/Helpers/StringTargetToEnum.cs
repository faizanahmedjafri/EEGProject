using SharedCore.Enums;

namespace biopot.Helpers
{
    public static class StringTargetToEnum
    {
        public static ESaveDataWays ConvertStringTargetToEnum(string target)
        {
            switch(target)
            {
                case "Internal Memory":
                    {
                        return ESaveDataWays.Device;
                    }
                case "Internal SD Card":
                    {
                        return ESaveDataWays.SD;
                    }
                case "USB OTG Cable":
                    return ESaveDataWays.Usb;
            }
            return ESaveDataWays.Device;
        }
    }
}
