import Foundation

// MARK: - List

@_cdecl("SwiftPlugin_GetFileNames")
public func getFileNames(
    configJson: UnsafePointer<CChar>,  // 追加: SMBConnectionConfigのJSON
    path: UnsafePointer<CChar>,
    instanceId: Int32,
    onSuccess: @escaping @Sendable Callback_withString,
    onError: @escaping @Sendable Callback_withIntString
) {
    let pathString = String(cString: path)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1, "Failed to decode SMB connection config".cString(using: .utf8)!)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let files = try await smbService.loadFiles(path: pathString)
            let fileList = files.joined(separator: ",")
            onSuccess(instanceId, fileList.cString(using: .utf8)!)
        }
        catch let error as SMBError {
            print("SMB Error: \(error.localizedDescription)")
            let errorInfo = ErrorInfo(code: Int32(error.hashValue), message: error.localizedDescription)
            onError(instanceId, errorInfo.code, errorInfo.message.cString(using: .utf8)!)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code, unknownError.message.cString(using: .utf8)!)
        }
    }
}


// MARK: - Download

@_cdecl("SwiftPlugin_DownloadFile")
public func downloadFile(
    configJson: UnsafePointer<CChar>,  // 追加: SMBConnectionConfigのJSON
    remotePath: UnsafePointer<CChar>,
    localPath: UnsafePointer<CChar>,
    instanceId: Int32,      // Instance id
    onSuccess: @escaping @Sendable Callback,
    onError: @escaping @Sendable Callback_withIntString)
{
    let remotePathString = String(cString: remotePath)
    let localPathString = String(cString: localPath)
    let configString = String(cString: configJson)
        
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1, "Failed to decode SMB connection config".cString(using: .utf8)!)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let localURL = URL(fileURLWithPath: localPathString)
            try await smbService.downloadFile(filePath: remotePathString, localURL: localURL)
            onSuccess(instanceId)
        }
        catch let error as SMBError {
            print("SMB Error: \(error.localizedDescription)")
            let errorInfo = ErrorInfo(code: Int32(error.hashValue), message: error.localizedDescription)
            onError(instanceId, errorInfo.code, errorInfo.message.cString(using: .utf8)!)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code, unknownError.message.cString(using: .utf8)!)
        }
    }
}


// MARK: - Upload

@_cdecl("SwiftPlugin_UploadFile")
public func uploadFile(
    configJson: UnsafePointer<CChar>,
    localPath: UnsafePointer<CChar>,
    remotePath: UnsafePointer<CChar>,
    instanceId: Int32,
    onSuccess: @escaping @Sendable Callback,
    onError: @escaping @Sendable Callback_withIntString)
{
    let localPathString = String(cString: localPath)
    let remotePathString = String(cString: remotePath)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1, "Failed to decode SMB connection config".cString(using: .utf8)!)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let localURL = URL(fileURLWithPath: localPathString)
            try await smbService.uploadFile(localURL: localURL, remotePath: remotePathString)
            onSuccess(instanceId)
        }
        catch let error as SMBError {
            print("SMB Error: \(error.localizedDescription)")
            let errorInfo = ErrorInfo(code: Int32(error.hashValue), message: error.localizedDescription)
            onError(instanceId, errorInfo.code, errorInfo.message.cString(using: .utf8)!)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code, unknownError.message.cString(using: .utf8)!)
        }
    }
}


// MARK: - Delete

@_cdecl("SwiftPlugin_DeleteFile")
public func deleteFile(
    configJson: UnsafePointer<CChar>,
    remotePath: UnsafePointer<CChar>,
    instanceId: Int32,
    onSuccess: @escaping @Sendable Callback,
    onError: @escaping @Sendable Callback_withIntString)
{
    let remotePathString = String(cString: remotePath)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1, "Failed to decode SMB connection config".cString(using: .utf8)!)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            try await smbService.deleteFile(remotePath: remotePathString)
            onSuccess(instanceId)
        }
        catch let error as SMBError {
            print("SMB Error: \(error.localizedDescription)")
            let errorInfo = ErrorInfo(code: Int32(error.hashValue), message: error.localizedDescription)
            onError(instanceId, errorInfo.code, errorInfo.message.cString(using: .utf8)!)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code, unknownError.message.cString(using: .utf8)!)
        }
    }
}
