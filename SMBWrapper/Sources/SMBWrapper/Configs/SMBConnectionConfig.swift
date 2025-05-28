import SMBClient
import Foundation


// MARK: - SMBConnectionConfig

struct SMBConnectionConfig: CustomStringConvertible, Decodable {
    var ipAddress: String
    var userName: String
    var password: String
    var shareName: String
    
    var description: String {
        return """
        IP Address: \(self.ipAddress)
        User Name: \(self.userName)
        Password: \(self.password)
        Share Name: \(self.shareName)
        """
    }
}

// 受け取ったJSONデータをデコード
func decodeSMBConnectionConfig(from json: String) -> SMBConnectionConfig? {
    let data = json.data(using: .utf8)!
    let decoder = JSONDecoder()
    do {
        let config = try decoder.decode(SMBConnectionConfig.self, from: data)
        return config
    } catch {
        print("Failed to decode JSON: \(error)")
        return nil
    }
}
