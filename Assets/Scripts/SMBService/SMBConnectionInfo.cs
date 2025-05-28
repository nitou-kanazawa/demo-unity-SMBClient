using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Project.Networking.SMB {

    [Serializable]
    public sealed class SMBConnectionInfo {
        public string ipAddress;
        public string userName;
        public string password;
        public string shareName;

        public SMBConnectionInfo(IPAddress ipAddress, string userName, string password, string shareName) {
            // Validate IP address (must not be null or IPAddress.None)
            if (ipAddress == null || ipAddress.Equals(IPAddress.None))
                throw new ArgumentException("Invalid IP address.", nameof(ipAddress));

            // Validate username (must not be empty or whitespace)
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username cannot be empty.", nameof(userName));

            // Validate password (must not be empty or whitespace)
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            // Validate share name (must not be empty and contain only valid characters)
            if (string.IsNullOrWhiteSpace(shareName))
                throw new ArgumentException("Share name cannot be empty.", nameof(shareName));
            if (!Regex.IsMatch(shareName, @"^[\w\-]+$"))
                throw new ArgumentException("Share name can only contain alphanumeric characters, underscores, and hyphens.", nameof(shareName));

            this.ipAddress = ipAddress.ToString();
            this.userName = userName;
            this.password = password;
            this.shareName = shareName;
        }

        public override string ToString() {
#if UNITY_EDITOR
            return $"smb:\n" +
                   $"IP: {ipAddress}\n" +
                   $"Share: {shareName}\n" +
                   $"User: {userName}\n" +
                   $"Pass: {password ?? "(null)"}";
#else
        return $"smb://{userName}:{(string.IsNullOrEmpty(password) ? "*****" : "*****")}@{ipAddress}/{shareName}";
#endif
        }
    }

}