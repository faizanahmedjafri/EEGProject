using System;
using System.Collections.Generic;

namespace biopot.Models
{
    public class ImpedanceDataEventArgs : EventArgs
    {
        public ImpedanceDataEventArgs(IReadOnlyDictionary<int, double> aFullData)
        {
            FullData = aFullData;
        }

        /// <summary>
        /// Gets the impedance data per channel ID (starts from 1).
        /// </summary>
        public IReadOnlyDictionary<int, double> FullData { get; }
    }
}