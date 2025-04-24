
>摘要
本文档详细介绍了我基于 HybridCLR 与 AR Foundation 的 Unity AR 开发插件，旨在为开发者提供高效的跨平台热更新方案。文章从背景与动机出发，覆盖一键安装工具、环境配置、热更新数据制作与示例程序运行等核心模块，并展示代码结构与使用指南。文末说明项目已归档不再维护，欢迎 Fork 与二次开发。


**引言**

在增强现实（AR）技术快速发展的大背景下，Unity 已成为移动端 AR 应用的首选引擎。然而，随着项目规模扩大与功能复杂度提升，传统的“全量构建+发布”流程已难以满足快速迭代与热修复需求：每次更改都需要长时间的 AOT 编译与打包，影响开发效率。为此，我基于 HybridCLR 和 AR Foundation，打造了一款支持热更新的 Unity AR 开发插件（以下简称“本插件”），让 AR 场景内容与逻辑的更新更轻量、更灵活。

---

## 一、插件背景与动机

1. **提升迭代效率**  
   Unity 在 IL2CPP 模式下的 AOT 编译与打包环节耗时惊人，尤其是大型项目常需数分钟至十几分钟。通过 HybridCLR，我们可将游戏核心逻辑与示例功能打包为可热更新的 Assembly，只在需要时更新对应 DLL，极大缩短迭代周期。

2. **跨平台一致性**  
   HybridCLR 扩展了 IL2CPP，使其从纯 AOT Runtime 转变为 AOT + Interpreter 混合模式，原生支持动态加载 Assembly。它在 iOS、Android 及其他平台（如主机）上都能以 AOT+Interpreter 模式高效执行，为跨平台 AR 应用热更新提供统一方案。

3. **内容驱动开发**  
   AR 应用核心在于场景与资源内容的丰富与优化。本插件将 AR 场景、模型及逻辑统一封装为可热更新资源，允许动态下发与版本管理，缩减客户端包体积，提升用户体验。

---

## 二、功能概览

- **一键安装工具**：集成 EnvInstaller，一键导入 AR Foundation、ARCore Extension、HybridCLR 相关 Package；  
- **开发环境配置**：通过插件设置与 Unity Project/Player Settings 快速完成 ARCore 与 HybridCLR 所需配置；  
- **热更新数据制作**：提供 “BuildBundle-Android” 导出流程，将 AR 场景打包为热更新数据（ZIP + version 文件）；  
- **示例程序运行**：包含 AR 图片识别场景示例与动态场景示例，展示如何加载热更新数据并进入入口场景；  
- **动态加载逻辑**：核心组件 DataDownLoader、DllLoader 与 HotFixDataMgr，分别负责数据下载、DLL 加载与入口场景切换；  
- **灵活扩展接口**：Expose DetectCallback 等回调机制，可自定义图片识别、场景交互与资源加载逻辑；  
- **归档开源**：当前版本为稳定归档版，不再维护，欢迎社区 Fork 与二次开发。

---

## 三、技术架构与核心模块

### 3.1 一键安装环境

- **EnvInstaller 插件**：在导入后新增 “Installer” 菜单，提供 “Install ARCore” 与 “Install HybridCLR” 两个选项，自动添加所需 Package 与依赖。  
- **ARCore（XR Foundation）**：EnvInstaller 会导入 AR Foundation 与 ARCore Extensions，简化手动导入流程。  
- **HybridCLR**：EnvInstaller 包含 HybridCLR package，执行两步 “Install” 操作后，自动替换 libil2cpp 并启用混合运行时。

### 3.2 开发环境配置

1. **导入 AR SDK**  
   通过 `Assets -> Import Package -> Custom Package…` 导入 `AR SDK_v1.0.1.x.unitypackage`，激活菜单 “Holo-XR > Settings”，勾选“热更新”和“ARCore”。

2. **Project/Player Settings 调整**  
   - **Graphics API**：取消 Auto Graphics API，移除 Vulkan；  
   - **Minimum API Level**：设为 Android 7.0 (API Level 24) 或以上；  
   - **Scripting Backend**：选择 IL2CPP，API Level 切换为 .NET Framework（Unity 2021+）或 .NET 4.x（Unity ≤2020）。

3. **XR Plug-in Management**  
   在 Project Settings > XR Plug-in Management 中启用 ARCore，并在 ARCore 菜单设置深度可选。

4. **HybridCLR Settings**  
   - 打开菜单 `HybridCLR > Installer…`，确保 Installed 状态为 True；  
   - 在 `HybridCLR > Settings` 中，将示例程序集（Holo.Demo、DynamicScene）添加到 `hotUpdateAssemblies` 或 `hotUpdateAssemblyDefinitions`；  
   - 配置 “Patch AOT Assemblies”，添加需要补充元数据的 DLL 列表（UnityEngine.AndroidJNIModule.dll、UnityEngine.CoreModule.dll、mscorlib.dll）。

### 3.3 热更新数据制作

1. **场景配置**  
   - 在 `ARCore Session` 对象上添加 `ARCoreImageDetect` 组件，配置待识别的图像集合及对应 Prefab；  
   - 自定义继承 `DetectCallback`，重写 `OnAdded`、`OnUpdate`、`OnRemoved` 回调，实现识别事件逻辑。

2. **导出热更数据**  
   在菜单 `Holo-XR -> BuildBundle-Android` 中，选择入口场景，点击“导出”，生成 ZIP 包与 `version` 文件，作为热更数据。

### 3.4 示例程序运行

1. **程序集划分**  
   在 `HybridCLR Settings` 中完成示例程序集划分与元数据补充。  
2. **场景与打包**  
   打开示例场景 `/Assets/Scenes/AOT/Scene_AOT_2.unity`，在 Build Settings 中添加场景并运行 Build or Build And Run。  
3. **关键组件**  
   - **DataDownLoader**：负责版本校验与热更数据下载，提供 `StartDownload()` 方法、`OnProgressUpdate`、`OnError` 回调；  
   - **DllLoader**：读取热更 Assembly 与 AB包，提供 `StartReadData()` 与 `getEntrance()` 方法；  
   - **HotFixDataMgr**：管理下载与加载流程，提供 `StartGame()` 与 `UpdateData()`，并在 `Awake()` 中注册错误与进度事件。

---

## 四、代码结构与仓库布局

```
/Assets
  /Editor
    EnvInstaller_vX.X
  /HoloXR
    /Scripts
      DataDownLoader.cs
      DllLoader.cs
      HotFixDataMgr.cs
      DetectCallback.cs
    /Settings
      ARCoreSettings.asset
      HybridCLRSettings.asset
  /Scenes
    AOT/Scene_AOT_2.unity
    HotUpdate/Scene_Example.unity
  /Resources
    /HotUpdateData
/HybridCLRData
  AssembliesPostIl2CppStrip/{platform}
/BuildScripts
  build_android.bat
LICENSE.md
README.md
```

- **Assets/HoloXR**：核心脚本与资源；  
- **Assets/Editor/EnvInstaller**：EnvInstaller 插件源代码；  
- **BuildScripts**：一键打包脚本，可调整 SDK 版本与输出路径；  
- **HybridCLRData**：AOT 裁剪后 DLL 与元数据文件；  
- **README.md**：项目介绍、快速开始与使用指南；  
- **LICENSE.md**：MIT 开源协议。

---

## 五、使用指南

1. Clone 仓库：  
   ```bash
   git clone https://github.com/YourRepo/Unity-AR-Plugin-Archived.git
   ```
2. 打开 Unity 项目，执行 `Assets -> Import Package -> Custom Package…` 导入 EnvInstaller 与 AR SDK；  
3. 在 `Holo-XR > Settings` 中启用热更新与 ARCore；  
4. 调整 Project/Player Settings 后，点击 `Installer -> Install ARCore`、`Installer -> Install HybridCLR`；  
5. 导入 AR SDK Package，并在 `Holo-XR -> BuildBundle-Android` 导出示例热更数据；  
6. 在示例场景中运行，或在自定义场景下调用 `HotFixDataMgr` 的方法加载热更内容。

---

## 六、贡献与二次开发

本仓库已“归档”且不再维护，欢迎社区 Fork 后：  
- 修复 Issue；  
- 优化打包与生成流程；  
- 支持更多 AR 平台（ARKit、Magic Leap 等）；  
- 集成其他热更新方案（如 ILRuntime）。

请参考 `CONTRIBUTING.md` 提交 PR，并在 Issue 中描述具体需求或改进建议。

---

## 七、归档说明
仓库地址:[EQ-MR-For-Unity](https://github.com/eqgis/EQ-MR-For-Unity/releases)
本项目于 2024 年 5 月完成最后一次维护，为稳定归档版本，不再接受更新或修复。如需新功能或定制化需求，请自行 Fork 并开发。

---

v1.0.2 Latest
Runtime：
-接入ARCore，新增功能如下：
1、支持通过图像识别实现新载入场景进行重定位

Editor：
1、新增Installer部分（用于预先安装依赖项）

其它：
1、更新与Android 原生交互部分所依赖的aar
2、完善程序集版本定义

---

May 9, 2024
v1.0.1
Runtime：
-接入ARCore，新增功能如下：
1、接入ARFoudation中所有功能
1、支持在热更程序集中使用AR增强图像的功能
2、支持使用ARCore采集点云数据
3、支持“通过图片识别的方式动态加载场景”的功能
-更新AndroidPlugin下aar

Editor：
1、支持一键导入ARFoundation
1、支持导入"*.pts"格式的点云数据
2、完善热更数据打包工具

Demo：
1、新增“AR动态场景”的示例
2、新增“点云采集”的示例

BUG：
1、解决热更数据重复加载的报错
2、解决使用Editor导出场景时偶尔报错的问题

---



v0.1.2
Sep 14, 2023
通用功能：
1、支持语音唤醒、语音识别、语音合成
2、提供语音助手（“VoiceAssistant”）
3、支持调用Eqgis的后台语音服务（参考“VoiceAssistantService”）

Unity编辑器：
1、支持在层级菜单中添加语音助手（“VoiceAssistant”）

示例：
1、语音服务调用示例（Holo/Demo/Speech/语音服务.unity）
2、思必驰语音助手使用示例（Holo/Demo/Speech/AiSpeech.unity）

备注：当前版本需要使用speech-plugin-1.0.6.aar


---


v0.1.1
Aug 24, 2023
MR平台：
-接入XVisio 设备，新增功能如下：
1、支持CSlamMapLoader加载在线homap数据

通用功能：
1、支持自动校验本地数据版本与服务器数据版本
2、支持根据服务器地址自动进行数据热更
3、数据热更时添加进度监听事件

Unity编辑器：
1、支持“*.homap”数据通过网络路径导入
2、支持对生成的热更数据进行加密

示例：
1、新增XVisio设备的热更示例

---


v0.1.0
Aug 15, 2023
MR平台：
-接入XVisio 设备，新增功能如下：
1、引入AprilTag识别
2、支持场景扫描，并将扫描结果保存为“*.homap”
3、支持根据“ *.homap”恢复场景
4、支持MR场景与MR场景之间的跳转

Unity编辑器：
1、新增Xvisio平台默认配置导入
2、支持“*.homap”数据从本地文件路径导入
3、支持安卓平台的场景资源以及C#脚本打包

资源内容：
1、引入用于实现遮挡的材质文件


---

## 八、未来规划

> **提示**：本仓库已停止维护，后续不会有更新。建议有意向的开发者参考或二次开发，贡献社区力量。  

---

**结语**

本插件旨在为 Unity AR 开发者提供一站式热更新解决方案，结合 HybridCLR 与 AR Foundation，实现跨平台、快速迭代的 AR 应用交付。虽然项目已归档，但其设计思路与核心模块对类似需求或许有借鉴意义。期待社区同学发扬开源精神，在此基础上持续创新，推动XR技术的发展！


