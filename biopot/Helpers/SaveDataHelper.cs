using System;
using biopot.ViewModels;

namespace biopot.Helpers
{
    public static class SaveDataHelper
    {
        public static string PrepareFileName(SessionViewModel session, DateTime dateTime)
        {
            string fileName = string.Empty;
            fileName += session.FileName == string.Empty ? "BioPot_" : $"{session.FileName}_";
            fileName += session.IsDateInName ? $"{dateTime.ToString("yyyyMMdd")}_" : string.Empty;
            fileName += session.IsTimeInName ? $"{dateTime.ToString("HH.mm.ss")}_" : string.Empty;

            return fileName;
        }
    }
}