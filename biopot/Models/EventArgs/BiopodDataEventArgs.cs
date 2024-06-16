using System;
using System.Collections.Generic;

namespace biopot.Models
{
    public class BiopodDataEventArgs : EventArgs
    {
        private readonly IList<byte[]> _fullData;

        public BiopodDataEventArgs(IList<byte[]> fullData)
        {
            _fullData = fullData;
        }

        public IList<byte[]> FullData
        {
            get { return _fullData; }
        }
    }
}
