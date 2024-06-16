namespace SharedCore.Enums
{
    public enum ConnectionLostErrors
    {
        ConnectionLost = 1000,
    }

    public enum SignalRetrievalErrors
    {
        DeviceNotConnected = 2000,
        NotSupportedCharacteristicOperation = 2001,
        CharacteristicReadException = 2002,
        InsufficientBufferLength = 2003,
        MissingBleService = 2004,
    }

    public enum DataSavingErrors
    {
        OutOfMemory = 3000,
        MissingPermissions = 3001,
        IoException = 3002,
        DevelopmentError = 3003,
    }
}
