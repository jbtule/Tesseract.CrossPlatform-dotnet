using System;
using System.Collections.Generic;
using System.Text;

namespace Tesseract
{
    [Serializable]
    public class LeptonicaException : Exception
    {
        public LeptonicaException() { }
        public LeptonicaException(string message) : base(message) { }
        public LeptonicaException(string message, Exception inner) : base(message, inner) { }
    }
}
