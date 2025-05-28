import Foundation

// MARK: -

enum SMBError: Error {
    case connectionFailed
    case loginFailed
    case fileListingFailed
    case unknown
    
    // 文字列に変換する
    var localizedDescription: String {
        switch self {
        case .connectionFailed:
            return "SMB connection failed."
        case .loginFailed:
            return "SMB login failed."
        case .fileListingFailed:
            return "Failed to list files in the directory."
        case .unknown:
            return "An unknown error occurred."
        }
    }
}

// エラーの詳細を含む構造体
struct ErrorInfo {
    var code: Int32
    var message: String
}
