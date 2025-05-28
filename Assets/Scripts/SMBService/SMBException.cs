using System;

namespace Project.Networking.SMB
{
    /// <summary>
    /// SMB関連のエラーコードを定義
    /// </summary>
    public enum ErrorCode
    {
        Unknown,            // 原因不明・マッピングできない

        Connection,         // 接続
        Timeout,            // 応答が返らなかった
        Authentication,     // 資格情報の不一致（ユーザー/パス/トークン）

        FileOperation,          // 
        Permission,         // 認証は成功しているが、操作が拒否された
        NotFound,           // パスや共有が存在しない
        Conflict,           // ディレクトリが空でない、共有違反など
        Resource,           // クレジット不足、容量不足、ハンドル枯渇
        Unsupported,        // サーバ未対応、未実装、バージョン不整合
    }


    public sealed class SMBException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public SMBException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public SMBException(ErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return $"SMBException: {ErrorCode} - {Message}";
        }
    }
}