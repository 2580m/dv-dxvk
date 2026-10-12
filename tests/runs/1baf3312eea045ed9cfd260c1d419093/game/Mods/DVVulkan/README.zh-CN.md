# DV Vulkan (DXVK) 1.0.2

Windows x64 的 Derail Valley / Unity Mod Manager mod，包含 DXVK 官方 3.1.1 的 x64 d3d11.dll 与 dxgi.dll，将游戏的 Direct3D 11 调用转译成 Vulkan。需要支持该版本 DXVK 的 Vulkan 显卡驱动，建议先更新驱动。

## 安装与启用

1. 退出游戏，在 Unity Mod Manager 中选择 Derail Valley，把 DVVulkan-1.0.2.zip 拖入 Mods 页安装。
2. 启动游戏，在 UMM 中启用 DV Vulkan。首次加载不会改变本次渲染器。
3. 退出游戏，等待 Mods/DVVulkan/deployment.log 出现 `Applied: enable`，再启动游戏。通常只需几秒。
4. 如有渲染 API 启动参数，请保留 `-force-d3d11`，移除 `-force-vulkan`、`-force-d3d12` 和其他强制图形 API 参数。游戏默认 D3D11 时无需额外参数。

Unity 在 DXVK 下仍会报告 Direct3D11。mod 设置页会检查本次进程是否加载本包的两个转译 DLL。实际 Vulkan 设备和驱动信息位于游戏工作目录下的 DerailValley_d3d11.log / DerailValley_dxgi.log；应查看本次启动生成的日志。

## 停用、卸载与启动失败恢复

推荐卸载流程：在游戏内打开本 mod 的设置页，点击 **卸载 / 移除 DXVK 并停用**（英文为 **Uninstall / Remove DXVK and disable**），然后退出游戏。后台程序会移除游戏根目录内由本包部署的 `d3d11.dll` 和 `dxgi.dll`。等待 `Mods/DVVulkan/deployment.log` 出现 `已应用：卸载`（英文为 `Applied: uninstall`），再用 UMM 删除模组文件。下一次启动恢复原生 Direct3D 11。不要在后台移除完成前用 UMM 删除模组，否则待执行任务和后台程序可能被一起删除。

按钮会停用模组并保存持久卸载标记，即使 UMM 仍保留启用设置或存在旧的启用请求，也不会再次部署 DXVK。需要重新使用时，点击设置页的 **安装 / 恢复 DXVK**（英文为 **Install / Restore DXVK**），再退出并重新启动游戏。卸载按钮保留 mod 目录和日志，以便确认操作结果；最后由 UMM 删除模组文件。

普通停用仍可使用游戏内 UMM 启用开关：退出后，等待 `已应用：停用`（英文为 `Applied: disable`）。只在外部 UMM 中禁用或删除 mod 不会移除游戏根目录的转译 DLL。

## 自动语言切换

界面、按钮、状态、UMM 中的模组日志以及 deployment.log 使用游戏的 I2 Localization 当前语言。简体或繁体中文选项显示中文，其余语言回退为英文；中文文案为简体中文。游戏内切换语言后界面即时更新，后台部署程序在退出游戏后使用最后保存的游戏语言。设置页的部署日志视图也可重新显示为当前语言，已写入的磁盘日志不改写历史文本。

原生 DXVK 的 DerailValley_d3d11.log / DerailValley_dxgi.log 由 DXVK 生成，保持其原始英文。系统返回的错误细节、文件路径和 API 名称保持原文。

如果游戏无法启动：关闭所有 Derail Valley 进程，双击 `Mods/DVVulkan/Disable-DXVK.cmd`。若 DLL 仍在使用或出现错误，查看 deployment.log，关闭游戏再重试。若后台部署未完成，可用 `Apply-Pending.cmd` 重试。后台程序不会自动启动游戏。

只管理本版本的两个 DLL，使用固定 SHA-256 校验。不覆盖、备份或删除其他来源的同名 DLL；遇到 ReShade 或其他转译层的同名文件时会报冲突。请先自行处理冲突再启用。

## 支持范围与验证限制

目标环境为 Windows x64、Unity 2019、UMM 0.27.3+、D3D11。已验证构建与文件部署/恢复行为。游戏内 Vulkan 渲染、帧率、VR、画质和其他 mod 兼容性需要实机启动测试；不保证提升性能。首次使用可能出现着色器编译卡顿。需要诊断 HUD 时，在启动游戏的环境中设置 `DXVK_HUD=devinfo,fps,api`。

DXVK 项目：https://github.com/doitsujin/dxvk

官方发布：https://github.com/doitsujin/dxvk/releases/tag/v3.1.1

本包保留 DXVK-LICENSE 和 DXVK-README.md，DXVK 遵循其 zlib 许可证。Windows 支持和驱动要求以官方文档为准。
