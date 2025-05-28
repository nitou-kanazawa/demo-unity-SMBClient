import SMBClient
import Foundation

// MARK: -

public class SMBClientWrapper {
    private let client: SMBClient
    private let settings: SMBConnectionConfig
    
    
    // MARK: - Public Method
    
    init(setting: SMBConnectionConfig) {
        settings = setting
        
        client = SMBClient(host: settings.ipAddress)
    }
    
    
    // ファイルリストを取得
    func loadFiles(path: String = "") async throws -> [String] {
        do {
            try await loginOrThrow()
            try await connectOrThrow()
            
            //
            let files = try await client.listDirectory(path: path)
            
            try await client.disconnectShare()
            try await client.logoff()
            
            return files
                .map { $0.name }
                .filter { $0 != "." && $0 != ".." }
        }
        catch {
            // ログインや接続、ファイルリストのエラーが発生した場合
            try? await client.disconnectShare()
            try? await client.logoff()
            throw error
        }
    }
    
    
    // ファイルの存在確認
    func existFile(path: String) async throws -> Bool {
        do {
            try await loginOrThrow()
            try await connectOrThrow()
            
            let isExist = try await client.existFile(path: path)
            
            try await client.disconnectShare()
            try await client.logoff()
            
            return isExist
        }
        catch {
            // ログインや接続、ファイルリストのエラーが発生した場合
            try? await client.disconnectShare()
            try? await client.logoff()
            throw error
        }
    }
    
    
    // ダウンロード
    func downloadFile(filePath: String, localURL: URL) async throws {
        do{
            try await loginOrThrow()
            try await connectOrThrow()
            
            //
            let fileData = try await client.download(path: filePath)
            try fileData.write(to: localURL)
            
            try await client.disconnectShare()
            try await client.logoff()
        }
        catch {
            // ログインや接続、ファイルリストのエラーが発生した場合
            try? await client.disconnectShare()
            try? await client.logoff()
            throw error
        }
    }
    
    
    // アップロード
    func uploadFile(localURL: URL, remotePath: String) async throws {
        do{
            try await loginOrThrow()
            try await connectOrThrow()
            
            // 上書き
            if(try await client.existFile(path: remotePath)){
                try await client.deleteFile(path: remotePath)
            }
            try await client.upload(localPath: localURL, remotePath: remotePath)
            
            try await client.disconnectShare()
            try await client.logoff()
        }
        catch {
            // ログインや接続、ファイルリストのエラーが発生した場合
            try? await client.disconnectShare()
            try? await client.logoff()
            throw error
        }
    }
    
    // ファイル削除
    func deleteFile(remotePath: String) async throws {
        do{
            try await loginOrThrow()
            try await connectOrThrow()
            
            // 削除
            try await client.deleteFile(path: remotePath)
            
            try await client.disconnectShare()
            try await client.logoff()
        }
        catch {
            // ログインや接続、ファイルリストのエラーが発生した場合
            try? await client.disconnectShare()
            try? await client.logoff()
            throw error
        }
    }
    

    // MARK: - Private Method

    // ログイン処理
    private func loginOrThrow() async throws{
        do{
            try await client.login(username: settings.userName, password: settings.password)
        }catch{
            throw SMBError.loginFailed
        }
    }
    
    // 接続処理
    private func connectOrThrow() async throws{
        do{
            try await client.connectShare(settings.shareName)
        }catch{
            throw SMBError.connectionFailed
        }
    }

}
