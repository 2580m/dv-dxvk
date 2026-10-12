# DV Vulkan (DXVK) 1.0.0

Windows x64 的 Derail Valley / Unity Mod Manager mod，包含 DXVK 官方 3.1.1 的 x64 d3d11.dll 与 dxgi.dll，将游戏的 Direct3D 11 调用转译成 Vulkan。需要支持该版本 DXVK 的 Vulkan 显卡驱动，建议先更新驱动。

## 安装与启用

1. 退出游戏，在 Unity Mod Manager 中选择 Derail Valley，把 DVVulkan-1.0.0.zip 拖入 Mods 页安装。
2. 启动游戏，在 UMM 中启用 DV Vulkan。首次加载不会改变本次渲染器。
3. 退出游戏，等待 Mods/DVVulkan/deployment.log 出现 `Applied: enable`，再启动游戏。通常只需几秒。
4. 如有渲染 API 启动参数，请保留 `-force-d3d11`，移除 `-force-vulkan`、`-force-d3d12` 和其他强制图形 API 参数。游戏默认 D3D11 时无需额外参数。

Unity 在 DXVK 下仍会报告 Direct3D11。mod 设置页会检查本次进程是否加载本包的两个转译 DLL。实际 Vulkan 设备和驱动信息位于游戏工作目录下的 DerailValley_d3d11.log / DerailValley_dxgi.log；应查看本次启动生成的日志。

## 停用、卸载与启动失败恢复

在游戏中关闭 UMM 启用开关，退出游戏，等待 deployment.log 出现 `Applied: disable`，下一次启动恢复原生 D3D11。**卸载前必须先完成停用**；只在外部 UMM 中禁用或删除 mod 不会移除游戏根目录的转译 DLL，因为禁用后的 mod 不再执行。

如果游戏无法启动：关闭所有 Derail Valley 进程，双击 `Mods/DVVulkan/Disable-DXVK.cmd`。若 DLL 仍在使用或出现错误，查看 deployment.log，关闭游戏再重试。若后台部署未完成，可用 `Apply-Pending.cmd` 重试。后台程序不会自动启动游戏。

只管理本版本的两个 DLL，使用固定 SHA-256 校验。不覆盖、备份或删除其他来源的同名 DLL；遇到 ReShade 或其他转译层的同名文件时会报冲突。请先自行处理冲突再启用。

## 支持范围与验证限制

目标环境为 Windows x64、Unity 2019、UMM 0.27.3+、D3D11。已验证构建与文件部署/恢复行为。游戏内 Vulkan 渲染、帧率、VR、画质和其他 mod 兼容性需要实机启动测试；不保证提升性能。首次使用可能出现着色器编译卡顿。需要诊断 HUD 时，在启动游戏的环境中设置 `DXVK_HUD=devinfo,fps,api`。

DXVK 项目：https://github.com/doitsujin/dxvk

官方发布：https://github.com/doitsujin/dxvk/releases/tag/v3.1.1

本包保留 DXVK-LICENSE 和 DXVK-README.md，DXVK 遵循其 zlib 许可证。Windows 支持和驱动要求以官方文档为准。
