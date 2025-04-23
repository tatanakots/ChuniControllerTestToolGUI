# 中二节奏控制器测试工具

本项目是一个用于测试“中二节奏”游戏专用控制器的软件工具，同时提供图形用户界面（GUI）以便于直观地检测和调试控制器各项功能。

该工具主要用于验证和调试控制器的各项功能，包括：

- **地键触摸板测试**：检测并反馈地键触摸板的触控情况。
- **地键灯光控制**：监控和控制地键的灯光状态。
- **天键触发状态测试**：检测天键的触发情况。
- **天键灯光控制**：控制天键灯光，仅供娱乐。

## 项目背景

控制器连接部分基于 `chuniio_affine.dll` 实现，通过该动态链接库与控制器硬件进行通信。同时，本项目部分代码参考了 [Affine_IO 项目的 Chuniio 测试代码](https://github.com/QHPaeek/Affine_IO/blob/master/chuniio/test.c) 。

## 功能概览

- **地键触摸板检测**：实时监控触摸板的触控数据，方便用户进行校准和测试。
- **地键灯光控制**：测试灯光响应和显示效果，确保灯光控制正常。
- **天键触发状态检测**：捕获天键触发状态，用于功能验证和故障排查。
- **天键灯光控制**：控制天键灯光，仅供娱乐。

## 环境要求

- Windows 操作系统（由于 `chuniio_affine.dll` 依赖）
- 支持的编译环境（如 Visual Studio 或其他兼容工具）
- 基本的 C/C++ 编译环境
- 图形用户界面（GUI）支持环境

## 编译与运行

1. **下载代码**  
   将本项目代码克隆或下载到本地：

   ```bash
   git clone https://github.com/tatanakots/ChuniControllerTestToolGUI.git
   ```

2. **配置依赖**  
   确保 `chuniio_affine.dll` 已放置在 `/ChuniControllerTestToolGUI` 或系统路径中。

3. **编译项目**  
   使用你喜欢的编译工具（例如 Visual Studio）打开解决方案，并进行编译。

4. **运行测试工具**  
   编译成功后，运行生成的 GUI 可执行文件，即可开始测试各项控制器功能。

## 使用方法

- 前往Releases或Actions下载最新的预编译版本后运行即可。

## 已知问题

 - ~~COM口为10或以上可能会出现连接问题。~~(已解决)
 - ~~天键灯光命令可能会吃，因为发太快了设备可能反应不过来。~~(应该已解决)
> ~~![原项目作者给出的说明.png](/docs/img/img1.png)~~

## 参考

- [Affine_IO Chuniio 测试代码](https://github.com/QHPaeek/Affine_IO/blob/master/chuniio/test.c)

## 贡献

欢迎大家提交 issues 或 pull requests 来改进和扩展本项目。如果你有更好的实现方案或发现 bug，请随时反馈。

## 许可证

本项目采用 [GPL3.0](LICENSE) 开源许可证，详情请参阅 LICENSE 文件。