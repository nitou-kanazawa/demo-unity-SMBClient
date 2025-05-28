using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Project.Networking.SMB
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SMBConnectionInfo
    {
        public string IpAddress { get; }
        public string Username { get; }
        public string Password { get; }
        public string ShareName { get; }


        public SMBConnectionInfo(IPAddress ipAddress, string username, string password, string shareName)
        {
            // Validate IP address (must not be null or IPAddress.None)
            if (ipAddress == null || ipAddress.Equals(IPAddress.None))
                throw new ArgumentException("Invalid IP address.", nameof(ipAddress));

            // Validate username (must not be empty or whitespace)
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));

            // Validate password (must not be empty or whitespace)
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            // Validate share name (must not be empty and contain only valid characters)
            if (string.IsNullOrWhiteSpace(shareName))
                throw new ArgumentException("Share name cannot be empty.", nameof(shareName));
            if (!Regex.IsMatch(shareName, @"^[\w\-]+$"))
                throw new ArgumentException("Share name can only contain alphanumeric characters, underscores, and hyphens.", nameof(shareName));

            IpAddress = ipAddress.ToString();
            Username = username;
            Password = password;
            ShareName = shareName;
        }

        public override string ToString()
        {
#if UNITY_EDITOR
            return $"smb:\n" +
                   $"IP: {IpAddress}\n" +
                   $"Share: {ShareName}\n" +
                   $"User: {Username}\n" +
                   $"Pass: {Password ?? "(null)"}";
#else
    return $"smb://{Username}:{(string.IsNullOrEmpty(Password) ? "*****" : "*****")}@{IpAddress}/{ShareName}";
#endif

        }
    }

}