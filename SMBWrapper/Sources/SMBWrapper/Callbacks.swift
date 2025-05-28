import Foundation

// MARK: - C関数として公開される型
// Swiftから外部へ値を渡す用途で使用する。

public typealias Callback = @convention(c) @Sendable (Int32) -> Void
public typealias Callback_withInt = @convention(c) @Sendable (Int32, Int32) -> Void
public typealias Callback_withBool = @convention(c) @Sendable (Int32, Bool) -> Void
public typealias Callback_withString = @convention(c) @Sendable (Int32, UnsafePointer<CChar>) -> Void
public typealias Callback_withIntString = @convention(c) @Sendable (Int32, Int32, UnsafePointer<CChar>) -> Void
