#if UNITY_EDITOR
using UnityEngine;
using SMBLibrary;

namespace Project.Networking.SMB.Windows
{

    // [REF] https://github.com/TalAloni/SMBLibrary/blob/master/SMBLibrary/Enums/NTStatus.cs

    public static class ErrorMapper
    {
        public static ErrorCode FromNtStatus(NTStatus status) => status switch
        {
            // Connection
            NTStatus.STATUS_IO_TIMEOUT => ErrorCode.Timeout,

            // 認証
            NTStatus.STATUS_LOGON_FAILURE or 
            NTStatus.STATUS_WRONG_PASSWORD or
            NTStatus.STATUS_PASSWORD_EXPIRED or
            NTStatus.STATUS_PASSWORD_MUST_CHANGE or
            NTStatus.STATUS_ACCOUNT_DISABLED or
            NTStatus.STATUS_ACCOUNT_RESTRICTION or
            NTStatus.STATUS_INVALID_LOGON_HOURS => ErrorCode.Authentication,

            // 操作資格
            NTStatus.STATUS_ACCESS_DENIED => ErrorCode.Permission,

            // 
            NTStatus.STATUS_BAD_NETWORK_NAME or 
            NTStatus.STATUS_OBJECT_NAME_NOT_FOUND => ErrorCode.NotFound,

            // 
            NTStatus.STATUS_SHARING_VIOLATION or 
            NTStatus.STATUS_DIRECTORY_NOT_EMPTY => ErrorCode.Conflict,

            // 
            NTStatus.STATUS_DISK_FULL or 
            NTStatus.STATUS_INSUFFICIENT_RESOURCES => ErrorCode.Resource,

            NTStatus.STATUS_NOT_SUPPORTED => ErrorCode.Unsupported,

            _ => ErrorCode.Unknown,
        };

    }

}
#endif
