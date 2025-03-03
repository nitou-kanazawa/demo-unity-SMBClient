using System;

namespace Project.Networking.SMB {

    public class SMBException : Exception {
        public SMBException(string message) : base(message) { }
        public SMBException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class SMBConnectionEXception : SMBException {
        public SMBConnectionEXception(string message) : base(message) { }
        public SMBConnectionEXception(string message, Exception innerException) : base(message, innerException) { }
    }


}