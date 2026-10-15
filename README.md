# DV Vulkan (DXVK)

[English](#english) | [简体中文](#简体中文)

## English

DV Vulkan is a **Unity Mod Manager mod for Derail Valley on Windows x64**. It manages the deployment of DXVK, which translates the game's Direct3D 11 rendering calls into Vulkan.

The current version is **1.0.2**, bundled with the official **DXVK 3.1.1** x64 `d3d11.dll` and `dxgi.dll` libraries. Performance depends on your hardware, drivers and workload; an improvement is not guaranteed.

### Features

- Enable or disable DXVK through Unity Mod Manager, with changes applied after the game exits.
- Uninstall and restore controls in the mod settings.
- Checks whether the current game process loaded both bundled DXVK DLLs.
- Fixed SHA-256 checks for the bundled libraries; unrelated DLLs are never intentionally overwritten or removed.
- Chinese and English interface, status messages and mod logs, selected from the game's language.
- Recovery scripts for failed deployments or situations where the game cannot start.

### Requirements

- Derail Valley for Windows x64, using Direct3D 11. The integration targets Unity 2019.
- [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) **0.27.3 or newer**, installed for Derail Valley.
- A GPU and Vulkan driver supported by [DXVK 3.1.1](https://github.com/doitsujin/dxvk/releases/tag/v3.1.1). See the upstream [driver requirements](https://github.com/doitsujin/dxvk/wiki/Driver-support).
- Write access to the game directory and `Mods/DVVulkan`.

No separate DXVK installation is required. Resolve existing `d3d11.dll` or `dxgi.dll` files from other rendering tools before enabling this mod; it will report a conflict instead of replacing them.

### Installation

1. Close the game.
2. Install the packaged `DVVulkan-1.0.2.zip` through Unity Mod Manager. GitHub's **Source code** ZIP is not an installable mod package.
3. Launch the game and enable **DV Vulkan (DXVK)** in UMM.
4. Exit the game. Wait for `Mods/DVVulkan/deployment.log` to report **Applied: enable** (Chinese: **已应用：启用**).
5. Launch the game again. DXVK takes effect on this launch.

**Why a second launch?** Unity initializes the rendering device before UMM loads the mod. The helper deploys the DLLs after the game exits, so they can be loaded at the next startup. Subsequent launches use DXVK while its DLLs remain installed.

Keep the default Direct3D 11 renderer, or use `-force-d3d11`. Remove conflicting rendering flags such as `-force-vulkan` or `-force-d3d12`.

### Verify rendering and language

Open the mod settings to check whether both DXVK DLLs were loaded by the current process. Unity still reports **Direct3D11** when calls are translated by DXVK. Check the current launch's `DerailValley_d3d11.log` and `DerailValley_dxgi.log` in the game's working directory for Vulkan device and driver information.

The interface follows the game's language immediately. Chinese language selections use Simplified Chinese text; English and unsupported languages use English. The helper uses the last saved game language when processing changes after exit. The in-game deployment log view can render structured results in the current language; already written disk logs are not rewritten. Native DXVK logs remain in English, and system error details retain their original text.

### Disable, uninstall and recover

**Disable temporarily:** Turn off the mod in the in-game UMM window, exit, and wait for **Applied: disable** / **已应用：停用**. The next launch uses native Direct3D 11.

**Uninstall:**

1. In the mod settings, click **Uninstall / Remove DXVK and disable**.
2. Exit the game and wait for **Applied: uninstall** / **已应用：卸载** in `Mods/DVVulkan/deployment.log`.
3. Remove the mod files through UMM.

**Removing or disabling the mod only in the external UMM application does not remove DXVK DLLs already deployed to the game directory.** Do not remove the mod files before the pending cleanup completes.

The uninstall button leaves the mod files and logs available for UMM removal. A persistent uninstall marker prevents stale enable requests from redeploying DXVK. To restore it, enable the mod in UMM if necessary to access its settings, click **Install / Restore DXVK**, exit, and launch again after deployment completes.

If the game cannot start, close all Derail Valley processes and run:

| Script in `Mods/DVVulkan` | Purpose |
| --- | --- |
| `Disable-DXVK.cmd` | Remove this package's deployed DXVK DLLs. |
| `Apply-Pending.cmd` | Retry the pending enable, disable or uninstall operation. |

Inspect `deployment.log` if an operation fails. Close processes still using the DLLs before retrying. Keep the mod folder and its bundled DLLs intact until removal succeeds.

### Build from source

Build on Windows with PowerShell 7 recommended and the .NET Framework C# compiler available at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`. A separate .NET SDK or NuGet restore is not used by the build script.

The build references your local game's `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `I2.Localization.dll`, and UMM's `UnityModManager.dll`. These dependencies must exist in the game's managed directory; they are not redistributed in the mod ZIP.

```powershell
git clone https://github.com/2580m/dv-dxvk.git
Set-Location dv-dxvk
.\build.ps1 -GameDirectory 'C:\Program Files (x86)\Steam\steamapps\common\Derail Valley'
```

Replace the example game path with your installation directory. The script uses the version in `package/Info.json` and writes:

```text
dist/DVVulkan/              Ready-to-package mod folder
dist/DVVulkan-1.0.2.zip     UMM installation archive
```

The expected vendor files are `vendor/dxvk-3.1.1/x64/d3d11.dll`, `vendor/dxvk-3.1.1/x64/dxgi.dll`, `vendor/DXVK-LICENSE`, and `vendor/DXVK-README.md`. If absent from your checkout, obtain them from the official DXVK release above. The source and build scripts do not download them automatically. Expected SHA-256 hashes are defined in `src/Deployment.cs`.

### Tests and implementation

Close all Derail Valley instances, build, then run:

```powershell
.\tests\Verify.ps1
```

Tests use isolated mock game directories under `tests/runs/`. They cover deployment, repeated operations, rollback, foreign or locked DLLs, process-exit waiting, uninstall persistence, reinstall, language selection, localized logs and ZIP structure. The mock `DerailValley.exe` files are test placeholders, not copies of the game. These checks do not establish rendering quality, VR support, performance or compatibility with every other mod; those need live testing.

| File or directory | Role |
| --- | --- |
| `src/Main.cs` | UMM callbacks, settings interface and game-language integration. |
| `src/Deployment.cs` | DLL hash validation, deployment, removal and rollback. |
| `src/Worker.cs` | Standalone helper that waits for game exit and applies pending changes. |
| `src/Text.cs` | Shared translations, saved language and deployment log formatting. |
| `package/` | UMM metadata, recovery scripts and end-user documentation. |
| `vendor/` | Upstream DXVK files and license. |
| `tests/` | Deployment and localization verification. |

The separate `DVVulkan.Worker.exe` exists because loaded graphics DLLs may remain locked until the game exits. It waits for the original game process, checks for other running game instances, and applies the latest pending request. The mod and helper do not download components or collect user data. Their source is included here for review; this is not a guarantee that antivirus software will accept the binaries.

### Credits and license

Thanks to **Philip Rebohle and the DXVK contributors**, and to the **Unity Mod Manager contributors**.

The mod source is licensed under [MIT](LICENSE). Bundled DXVK components retain their separate [zlib/libpng license](vendor/DXVK-LICENSE); the mod's MIT license does not replace it. Upstream documentation is included in [vendor/DXVK-README.md](vendor/DXVK-README.md).

## 简体中文

DV Vulkan 是面向 **Windows x64 版 Derail Valley** 的 Unity Mod Manager 模组，通过 DXVK 将游戏的 Direct3D 11 渲染调用转译为 Vulkan。当前版本为 **1.0.2**，包含官方 **DXVK 3.1.1** 的两个 x64 DLL。性能表现取决于硬件、驱动和游戏负载，不保证提升帧率。

### 功能与要求

- 在 UMM 中管理 DXVK，退出游戏后自动部署或移除 DLL。
- 设置页提供卸载和恢复按钮，并检查本次进程是否加载两个 DXVK DLL。
- 使用固定 SHA-256 校验；遇到其他来源的同名 DLL 时停止操作。
- 界面和模组日志跟随游戏语言，中文选项使用简体中文，其余语言回退英文。
- 提供游戏无法启动或部署失败时的恢复脚本。

需要 Windows x64、使用 Direct3D 11 的 Derail Valley、已为游戏安装的 **UMM 0.27.3 或更新版本**，以及符合 [DXVK 驱动要求](https://github.com/doitsujin/dxvk/wiki/Driver-support)的显卡和 Vulkan 驱动。集成目标为 Unity 2019。游戏目录和模组目录需要可写，无需单独安装 DXVK。

### 安装和启用

1. 关闭游戏，用 UMM 安装 `DVVulkan-1.0.2.zip`。GitHub 自动生成的源码 ZIP 不能直接作为模组安装。
2. 启动游戏，在 UMM 中启用 **DV Vulkan (DXVK)**。
3. 退出游戏，等待 `Mods/DVVulkan/deployment.log` 显示 **已应用：启用**。
4. 再次启动游戏，DXVK 此时生效。以后保持部署状态即可，无需重复配置。

游戏在加载模组前已经初始化渲染器，因此首次启用需要再启动一次。使用默认 D3D11，或设置 `-force-d3d11`；移除 `-force-vulkan`、`-force-d3d12` 等冲突参数。

DXVK 下 Unity 仍显示 **Direct3D11**。设置页可检查 DLL 加载状态，实际 Vulkan 设备和驱动信息请查看游戏工作目录中本次启动生成的 `DerailValley_d3d11.log` 和 `DerailValley_dxgi.log`。

### 停用、卸载与恢复

临时停用：在游戏内关闭 UMM 启用开关，退出后等待日志显示 **已应用：停用**，下次启动恢复原生 Direct3D 11。

完整卸载：

1. 在模组设置页点击 **卸载 / 移除 DXVK 并停用**。
2. 退出游戏，等待日志显示 **已应用：卸载**。
3. 最后用 UMM 删除模组文件。

**只在外部 UMM 中禁用或卸载模组，不会删除已部署到游戏根目录的 DXVK DLL。请勿在后台清理完成前删除模组目录。**

卸载按钮保留模组文件和日志，供确认结果后由 UMM 删除。持久卸载标记可防止旧启用请求重新部署 DXVK。若要恢复，必要时先在 UMM 中启用模组以打开设置页，再点击 **安装 / 恢复 DXVK**，退出并等待部署完成后重新启动。

如果游戏无法启动，关闭所有游戏进程，运行 `Mods/DVVulkan/Disable-DXVK.cmd` 移除本包的转译 DLL。`Apply-Pending.cmd` 用于重试待执行任务。失败原因请查看 `deployment.log`；移除成功前保留模组目录和其中的 DXVK 文件。

界面会随游戏语言即时更新，后台程序在游戏退出后使用最后保存的游戏语言。设置页中的结构化部署结果可按当前语言重新显示，已写入的磁盘日志不会改写。DXVK 自身的底层日志保持英文，系统错误详情保留原文。

### 构建、测试与许可证

在 Windows 上构建，推荐使用 PowerShell 7。需要 .NET Framework 自带的 C# 编译器，以及本机游戏和 UMM 的引用程序集；无需单独安装 .NET SDK 或执行 NuGet 还原。详细依赖和项目结构见上方英文部分。

```powershell
git clone https://github.com/2580m/dv-dxvk.git
Set-Location dv-dxvk
.\build.ps1 -GameDirectory '你的游戏安装目录'
# 关闭所有游戏进程后运行测试
.\tests\Verify.ps1
```

输出为 `dist/DVVulkan/` 和按 `package/Info.json` 版本号命名的安装 ZIP。测试覆盖部署、卸载、回滚、DLL 冲突、进程等待、语言切换和压缩包结构；实际画质、VR、性能及其他模组兼容性仍需实机验证。

感谢 **DXVK** 和 **Unity Mod Manager** 的开发者及贡献者。模组源码使用 [MIT 许可证](LICENSE)，DXVK 组件保留其独立的 [zlib/libpng 许可证](vendor/DXVK-LICENSE)。
