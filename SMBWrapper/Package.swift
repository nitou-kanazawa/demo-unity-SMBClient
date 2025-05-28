// swift-tools-version: 6.0
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "SMBWrapper",
    platforms: [
      .macOS(.v10_15),
      .iOS(.v13),
      .visionOS(.v1)
    ],
    products: [
        .library(
            name: "SMBWrapper",
            type: .dynamic,
            targets: ["SMBWrapper"]),
    ],
    dependencies: [
        .package(url: "https://github.com/kishikawakatsumi/SMBClient.git", .upToNextMajor(from: "0.3.1")),
    ],
    targets: [
        .target(
            name: "SMBWrapper",
            dependencies: ["SMBClient"]
        ),
    ]
)
