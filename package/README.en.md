# DV Vulkan (DXVK) 1.0.2

Install DVVulkan-1.0.2.zip through Unity Mod Manager with the game closed. Enable the mod in game, exit, wait for deployment.log to confirm deployment, then launch again. The rendering API is initialized before UMM loads, so changes take effect on the next launch.

The settings UI, buttons, status messages, mod logs and deployment.log follow the game's I2 Localization language. Chinese selections use Simplified Chinese text; English and other languages use English. The UI follows language changes immediately. The deployment worker reads the last saved game language after the game exits. The settings page can display existing structured deployment results in the current language without rewriting historical disk logs.

To uninstall, click **Uninstall / Remove DXVK and disable** in the mod settings, then exit the game. Wait for deployment.log to show **Applied: uninstall**, then remove the mod files through UMM. Removing the mod through UMM alone does not remove the root DXVK DLLs. Use **Install / Restore DXVK** to explicitly restore DXVK after an uninstall request. Only this package's SHA-256-verified DLLs are removed; other DLLs are preserved.

If the game cannot start, close all game processes and run Mods/DVVulkan/Disable-DXVK.cmd. Apply-Pending.cmd retries an existing pending operation. Both scripts display UTF-8 deployment logs.

Use D3D11 (default, or -force-d3d11). Remove conflicting rendering flags such as -force-vulkan and -force-d3d12. Unity still reports Direct3D11 with DXVK. Confirm the Vulkan device in the native DXVK logs. These third-party logs remain in their original English; OS error details and API names are preserved verbatim.

Requires Windows x64, Unity Mod Manager 0.27.3+ and a Vulkan driver supported by DXVK 3.1.1. Build, file deployment, rollback, uninstall and localization behavior are tested; rendering quality, VR compatibility and performance require live game testing.

DXVK: https://github.com/doitsujin/dxvk/releases/tag/v3.1.1

The official DXVK DLLs are distributed with DXVK-LICENSE and DXVK-README.md.
