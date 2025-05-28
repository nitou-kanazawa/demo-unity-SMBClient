import Foundation

// MARK: -

enum SMBError : Int32, Error {
    case connectionFailed   = 1
    case loginFailed        = 2
    case listFailed       = 3
    case unknown          = -1
    
    var code: Int32 { rawValue }
    
    // 文字列に変換する
    var localizedDescription: String {
        switch self {
        case .connectionFailed:
            return "SMB connection failed."
        case .loginFailed:
            return "SMB login failed."
        case .listFailed:
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
