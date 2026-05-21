# 智能环境监测系统

这是一个基于 .NET 8 WPF 的智能环境监测系统示例项目，包含实时监测、数据库存储、登录权限、TCP 温湿度接入、串口空气质量接入、告警、历史数据、监测报告、Excel 导出和管理员后台管理。

## 项目结构

- `SmartEnvironmentMonitoringSystem.sln`：解决方案文件。
- `src/SmartEnvironmentMonitoringSystem.Wpf`：WPF 主程序。
- `tools/TcpTempHumiditySimulator`：TCP 温湿度模拟器。
- `tools/SerialAirQualitySimulator`：串口空气质量模拟器。
- `publish/win-x64`：发布输出目录，执行发布命令后生成。

## 默认账号

- 管理员：`admin / admin123`
- 普通用户：可在登录页注册，默认角色为 `User`。

## 权限说明

Admin 可以使用：

- 实时监测
- 历史数据查询和删除
- 监测报告
- Excel 导出
- 用户管理
- 阈值设置
- 通信设置
- 告警处理

User 可以使用：

- 实时监测
- 历史数据查询
- 监测报告
- Excel 导出

## 运行开发版

在 `SmartEnvironmentMonitoringSystem.App` 目录执行：

```powershell
dotnet build SmartEnvironmentMonitoringSystem.sln
dotnet run --project src\SmartEnvironmentMonitoringSystem.Wpf\SmartEnvironmentMonitoringSystem.Wpf.csproj
```

程序启动后会在运行目录下创建 SQLite 数据库：

```text
data/environment_monitor.db
```

## TCP 温湿度数据

主程序登录进入 Dashboard 后默认监听：

```text
0.0.0.0:9000
```

TCP 数据格式为一行一个 JSON：

新协议 telemetry：

```json
{"type":"telemetry","deviceId":"TH-001","temperature":26.5,"humidity":58.2,"timestamp":"2026-05-21 10:30:00"}
```

新协议 heartbeat：

```json
{"type":"heartbeat","deviceId":"TH-001","status":"online","timestamp":"2026-05-21 10:30:00"}
```

旧协议仍兼容：

```json
{"id":"TH-001","temperature":26.5,"humidity":58.2,"timestamp":"2026-05-21 10:30:00"}
```

可运行模拟器：

```powershell
dotnet run --project tools\TcpTempHumiditySimulator\TcpTempHumiditySimulator.csproj
```

多设备模拟示例：

```powershell
dotnet run --project tools\TcpTempHumiditySimulator\TcpTempHumiditySimulator.csproj -- 127.0.0.1 9000 TH-001 60 1000 5
dotnet run --project tools\TcpTempHumiditySimulator\TcpTempHumiditySimulator.csproj -- 127.0.0.1 9000 TH-002 60 1000 5
```

旧协议模拟示例：

```powershell
dotnet run --project tools\TcpTempHumiditySimulator\TcpTempHumiditySimulator.csproj -- 127.0.0.1 9000 TH-LEGACY 20 1000 5 legacy
```

## 串口空气质量数据

主程序默认尝试打开：

```text
COM3 / 9600 / 8N1 / ASCII
```

串口帧格式：

```text
AQ,AQ-001,135,620,NORMAL,2026-05-21 10:30:00
```

如果本机没有真实硬件，需要先安装成对虚拟串口工具，再让模拟器写入与主程序读取配对的串口。

TCP 和串口参数可以由 Admin 在 `通信设置` 页面修改，保存后重新登录或重启程序，Dashboard 会按新配置启动通信。

## 后台管理

Admin 登录后可以使用：

- `用户管理`：查看用户、启用/禁用用户、修改角色、重置密码。
- `阈值设置`：修改温度、湿度、烟雾、CO2 告警阈值。
- `通信设置`：修改 TCP 监听 IP、端口、串口号、波特率。
- `历史数据`：按日期范围和数据类型删除历史数据。

## Excel 导出

登录后进入 `Excel 导出` 页面，选择日期范围和 `.xlsx` 输出路径。导出的工作簿包含：

- `温湿度数据`
- `空气质量数据`
- `告警记录`
- `监测报告`

## 发布独立运行程序

在 `SmartEnvironmentMonitoringSystem.App` 目录执行：

```powershell
dotnet publish src\SmartEnvironmentMonitoringSystem.Wpf\SmartEnvironmentMonitoringSystem.Wpf.csproj -c Release -r win-x64 --self-contained true -o publish\win-x64
```

发布完成后运行：

```powershell
publish\win-x64\SmartEnvironmentMonitoringSystem.Wpf.exe
```

## 异常处理范围

当前阶段已处理以下异常场景：

- 程序启动初始化失败时提示错误并退出。
- UI 线程未处理异常会显示提示，避免直接崩溃。
- TCP 数据格式错误会显示状态，不影响后续连接。
- TCP 温湿度入库失败会显示状态，不中断监听。
- 串口打开失败、串口读取错误、串口关闭异常会显示状态。
- 告警检查、历史查询、报告生成和报告查询失败时会在页面显示错误。

## 当前限制

- 没有真实硬件时，只能通过 TCP 模拟器和虚拟串口工具进行联调。
- 通信参数保存后需要重新登录或重启程序，Dashboard 才会按新配置启动通信。
