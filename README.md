# MMO 学习工程

Unity MMORPG 课程学习与复盘项目，用于在不同设备上保存、下载和继续学习。仓库保存当前工程源码和资源，不代表已完成全部课程功能。

## 环境

- Unity：**2022.3.62f3c1**，以 `ProjectSettings/ProjectVersion.txt` 为准。
- 主要依赖：JKFrame、Netcode for GameObjects、Addressables、HybridCLR、Cinemachine、URP、MongoDB 驱动等。
- Unity 包配置见 `Packages/manifest.json`；`Packages/com.code-philosophy.hybridclr` 为随工程保存的嵌入式包。

## 在另一台设备下载

先安装 Git 和 **Git LFS**，然后执行：

```bash
git lfs install
git clone https://github.com/Zcoder-yyf/MMO.git
cd MMO
git lfs pull
```

大资源使用 Git LFS 保存。请使用上述克隆方式取得完整资源；网页上的 Download ZIP 不保证包含实际 LFS 文件内容。

在 Unity Hub 中添加克隆得到的项目目录，使用对应 Unity 版本打开，等待包恢复、资源导入和脚本编译完成。仓库根目录包含 `Assets`、`Packages` 和 `ProjectSettings`，不需要手动复制单个资源文件夹。请保留资源的 `.meta` 文件，以维持场景和预制体引用。

## 启动与配置

- 客户端入口：`Assets/Scene/ClientStartScene.unity`。
- 服务端入口：`Assets/Scene/ServerStartScene.unity`。
- `LoginScene` 和 `GameScene` 由启动和登录流程进入，通常从对应入口场景启动。
- 客户端和服务端分别运行。另一台设备上的服务地址、端口以及 Addressables 远程资源地址，需要按实际网络环境检查和配置。
- 当前 Addressables 远程地址配置在 `Assets/AddressableAssetsData/AddressableAssetSettings.asset`；局域网地址不能直接作为另一套环境的可用地址。

服务端使用 MongoDB：

| 项目 | 当前代码配置 |
| --- | --- |
| 连接字符串 | `mongodb://localhost:27017` |
| 数据库 | `MMONetCode` |
| 玩家集合 | `PlayerData` |

MongoDB 服务需要另外安装并启动。`localhost` 指运行服务端的设备；数据库中的用户记录不随 Git 仓库传输，需要单独备份和恢复，或在新环境中重新注册。配置逻辑见 `Assets/Scripts/Server/Data/DatabaseManager.cs`。

## 构建菜单

工程现有菜单定义在 `Assets/Editor/Scripts/BuildMenuItems.cs`：

| Unity 菜单 | 用途 |
| --- | --- |
| `Project → Build → NewClient` | 构建完整 Windows 客户端 |
| `Project → Build → UpdateClient` | 基于上次完整构建记录生成客户端内容更新包 |
| `Project → Build → Server` | 构建 Windows Dedicated Server |
| `Project → Build → All` | 依次构建客户端和服务端 |
| `Project → Build → GenerateDllBytesFile` | 将已有 AOT 和热更新程序集产物复制为 `.dll.bytes` 资源 |

当前构建脚本指定 Windows 64 位目标。完整客户端构建前应处于 Player 构建目标并启用工程使用的 Addressables 资源模式。HybridCLR 的本地工具链和生成缓存需要在新设备上准备；生成流程以项目对应 HybridCLR 菜单和配置为准。

`Builds`、`ServerData`、`HybridCLRData` 和 Addressables 构建状态记录不入库。新设备首次构建应先生成完整包及资源，建立自己的内容更新记录。客户端程序、服务端程序、资源包和热更新程序集产物应按实际构建目标重新生成；客户端更新包不会更新服务端可执行程序。切换到 Mac 等其他目标时，还需调整相应构建目标与脚本配置。

## 仓库内容

保留 `Assets` 及其 `.meta`、`Packages`、`ProjectSettings`、`Docs` 和原有 `ignore.conf`。Git 使用根目录 `.gitignore` 排除 Unity 缓存、本机 IDE 状态、Plastic SCM 元数据、构建输出、崩溃文件和 Mac 资源叉副文件。应用代码、场景、预制体及第三方库仍作为工程内容保留。

第三方依赖和资源遵循各自原有许可；本仓库没有为这些内容重新指定统一许可。
