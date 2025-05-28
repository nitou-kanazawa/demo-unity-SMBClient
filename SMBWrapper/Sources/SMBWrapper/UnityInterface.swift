import Foundation

// MARK: - List

@_cdecl("SwiftPlugin_GetFileNames")
public func getFileNames(
    configJson: UnsafePointer<CChar>,  // 追加: SMBConnectionConfigのJSON
    path: UnsafePointer<CChar>,
    instanceId: Int32,
    onSuccess: @escaping @Sendable Callback_withString,
    onError: @escaping @Sendable Callback_withInt
) {
    let pathString = String(cString: path)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let files = try await smbService.loadFiles(path: pathString)
            let fileList = files.joined(separator: ",")
            onSuccess(instanceId, fileList.cString(using: .utf8)!)
        }
        catch let smbError as SMBError {
            print("SMB Error: \(smbError.localizedDescription)")
            onError(instanceId, smbError.code)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            onError(instanceId, unknownError.code)
            print("Unknown Error: \(unknownError.message)")
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
    onError: @escaping @Sendable Callback_withInt)
{
    let remotePathString = String(cString: remotePath)
    let localPathString = String(cString: localPath)
    let configString = String(cString: configJson)
        
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let localURL = URL(fileURLWithPath: localPathString)
            try await smbService.downloadFile(filePath: remotePathString, localURL: localURL)
            onSuccess(instanceId)
        }
        catch let smbError as SMBError {
            print("SMB Error: \(smbError.localizedDescription)")
            onError(instanceId, smbError.code)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code)
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
    onError: @escaping @Sendable Callback_withInt)
{
    let localPathString = String(cString: localPath)
    let remotePathString = String(cString: remotePath)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            let localURL = URL(fileURLWithPath: localPathString)
            try await smbService.uploadFile(localURL: localURL, remotePath: remotePathString)
            onSuccess(instanceId)
        }
        catch let smbError as SMBError {
            print("SMB Error: \(smbError.localizedDescription)")
            onError(instanceId, smbError.code)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code)
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
    onError: @escaping @Sendable Callback_withInt)
{
    let remotePathString = String(cString: remotePath)
    let configString = String(cString: configJson)
    
    // configJsonをデコードしてSMBConnectionConfigを作成
    guard let config = decodeSMBConnectionConfig(from: configString) else {
        print("Failed to decode SMB connection config")
        onError(instanceId, -1)
        return
    }
    
    Task {
        do {
            let smbService = SMBClientWrapper(setting: config)
            
            try await smbService.deleteFile(remotePath: remotePathString)
            onSuccess(instanceId)
        }
        catch let smbError as SMBError {
            print("SMB Error: \(smbError.localizedDescription)")
            onError(instanceId, smbError.code)
        }
        catch let error {
            let unknownError = ErrorInfo(code: -1, message: "An unknown error | \(error.localizedDescription)")
            print("Unknown Error: \(unknownError.message)")
            onError(instanceId, unknownError.code)
        }
    }
}
