using System;
using biopot.Controls;

namespace biopot
{
	public class Constants
	{
        public const string ERROR_ID_OUT_OF_MEMORY = "ErrorId_OutOfMemory";
        public const string ERROR_ID_OTHER = "ErrorId_Other";
        public const string ERROR_ID_PERMISSION_MEMORY = "ErrorId_Permission_Memory";
        public const int    MAXIMUM_FOLDER_NAME_LENGTH = 100;
        public const int    MAXIMUM_FILE_NAME_LENGTH = 100;
        public const int    MAXIMUM_BT_CHAR_VAL_LENGTH = 2;
        public const int    DISCONNECTION_TIMER_TIME = 4000;//H.H. changed from 15000


        //saving current biopot status .... user name , patient name , ..... 
        public static bool FirstTimeLoad = true; 
        public static bool ForceReconnection = false;
        public static bool ForceRecordingFile= false;

        //add class for current status 
        /*
         public static class Biopot_Status
         {
           
            public static bool ForceReconnection = false;
            public static bool ForceRecordingFile= false;
            
         }
             */



        public static class StorageKeys
		{
			public const string CONNECTED_DEVICE = "ConnectedDevice";
			public const string PATIENT_DETAIL = "PatientDetailsViewModel";
			public const string SESSION_DETAIL = "SessionViewModel";
			public const string USER_DETAIL = "UserDetailsViewModel";
			public const string OTHER_DETAIL = "OtherDetailsViewModel";
		}

        public static class Files
        {
            public const int MAX_FILE_SIZE = 104857600;
            public const string FILE_TYPE_BINARY = "FileTypeBinary";
            public const string FILE_TYPE_TEXT = "FileTypeText";
            public const string FILE_TYPE = FILE_TYPE_BINARY;
        }

        public static class NavigationParamsKeys
        {
            public const string DEVICE_NAME = "DeviceName";
            public const string CHART_ID = "ChartId";
            public const string SCALE_VALUES = "scaleValues";
            public const string X_SCALE_VALUE = "XScaleValue";
            public const string DEVICE_TYPE = "DeviceType";
            public const string IS_BIO_IMPEDANCE = "IsBioImpedancePresent";
            public const string IS_ACCELEROMETER = "IsAccelerometerPresent";
            public const string ACC_MODE = "AccelerometerMode";
            public const string FILTERED_IDS = "filteredIds";
            public const string SENSOR_CONNECTION_LIST = "SensorConnectionList";
            public const string NAV_FROM_FILE_BROWSER = "NavFromFileBrowser";
            public const string DISCONNECTED_DEVICE = "DISCONNECTED_DEVICE";
            public const string DISCONNECTED_BLUETOOTH = "DISCONNECTED_BLUETOOTH";
            public const string NAV_FROM_IMPEDANCE = "NavFromImpedance";
            public const string NAV_BACK_TO_SCREEN = "NavBackToScreen";
        }

        public static class Logs
        {
            public const string LOG_FOLDER_NAME = "LOG";
            public const string LOG_FILE_NAME = "LOG_FILE";
            public const string NAVIGATION = "NAVIGATION";
            public const string ERRORS = "ERRORS";
            public const string DATA_ENTERED = "DATA_ENTERED";
            public const string EVENT = "EVENT";
            public const string ALERT = "ALERT";
            public const string DEVICE_INFO = "DEVICE_INFO";
            public const string LOG_DATA_FORMAT = "MM/dd/yy HH:mm:ss";
            public const string APP_NAME = "BioPot";
        }

        public static class Charts
        {
            public const int MaximumBufferedSeconds = 30;

            public const int ChartXAxisMinValue = 1; // seconds
            public const int ChartXAxisMaxValue = 30; // seconds
            public const int ChartXAxisDefaultValue = 10; // seconds

            public const int ChartYAxisMinValue = 5; // uV
            public const int ChartYAxisMaxValue = 6800; // uV
            public const int ChartYAxisDefaultValue = 300; // uV

            public const int ChartAccYAxisMinValue = 1; // uV
            public const int ChartAccYAxisMaxValue = 16; // uV
            public const int ChartAccYAxisDefaultValue = 16; // uV

            public static readonly IPickerStepInterpolator XAxisStepInterpolator = StaticPickerStep.One;

            public static readonly IPickerStepInterpolator YAxisStepInterpolator = new RangePickerStep
            {
                {0, 5}, // [0uV..100uV) = 5uV
                {100, 50}, // [100uV..1000uV) = 50uV
                {1000, 100}, // [1000uV..4000uV) = 100uV
                {4000, 200}, // [4000uV..) = 200uV
            };

            public static readonly IPickerStepInterpolator AccYAxisStepInterpolator = StaticPickerStep.One;
        }
    }
}
