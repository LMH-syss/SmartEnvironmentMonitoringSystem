# 智能环境监测系统

基于 .NET 8 WPF 的智能环境监测系统，支持 TCP/串口传感器接入、实时监测、告警判别、历史数据管理、Excel 导出和后台用户管理。

## 技术栈

| 技术 | 说明 |
|------|------|
| .NET 8 WPF | 桌面客户端框架 |
| CommunityToolkit.Mvvm 8.4 | MVVM 架构工具包 |
| FreeSql 3.5 + SQLite | ORM 与本地数据库 |
| LiveChartsCore (SkiaSharp) | 实时图表 |
| ClosedXML 0.105 | Excel 导出 |
| System.IO.Ports | 串口通信 |
| Microsoft.Extensions.DependencyInjection | 依赖注入 |

## 项目结构

```
wpf/
├── .gitignore
├── README.md
└── SmartEnvironmentMonitoringSystem.App/
    ├── SmartEnvironmentMonitoringSystem.sln
    ├── README.md                      # 详细使用说明
    ├── src/
    │   └── SmartEnvironmentMonitoringSystem.Wpf/
    │       ├── Communication/         # 通信协议解析（TCP JSON / 串口 CSV）
    │       ├── Data/                  # 数据库初始化与种子数据
    │       ├── Entities/              # 数据库实体
    │       ├── Infrastructure/        # 密码哈希工具
    │       ├── Models/                # 数据传输对象
    │       ├── Resources/             # XAML 样式资源
    │       ├── Services/              # 业务逻辑层
    │       ├── ViewModels/            # MVVM ViewModel 层
    │       └── Views/                 # XAML 视图层
    └── tools/
        ├── TcpTempHumiditySimulator/  # TCP 温湿度模拟器
        └── SerialAirQualitySimulator/ # 串口空气质量模拟器
```

## 功能模块

### 传感器数据接入
- **TCP 温湿度**：JSON 协议，支持 telemetry/heartbeat/legacy 三种格式，支持多设备并发连接，15 秒心跳超时检测
- **串口空气质量**：CSV 帧协议（`AQ,传感器ID,烟雾ppm,CO2ppm,等级,时间戳`），基于换行符的帧组装

### 实时监测（Dashboard）
- 温度实时折线图（LiveCharts）
- 温湿度 / 烟雾 / CO2 数值卡片
- 传感器、告警、通信三栏状态面板
- 在线设备列表与通信日志

### 告警系统
- WARN / DANGER 两级告警，烟雾 >= 700ppm 或 CO2 >= 1500ppm 自动升级
- 阈值从数据库动态加载，Admin 可在阈值设置页实时修改
- Dashboard 内直接处理告警

### 数据管理
- 历史数据查询（温湿度 / 空气质量 / 告警，按日期与传感器筛选）
- 管理员可按类型批量删除历史数据
- 监测报告生成（自动统计 max/min/avg/告警次数/综合评价）
- Excel 四工作簿导出（温湿度 / 空气质量 / 告警 / 报告）

### 用户与权限
- 默认管理员 `admin / admin123`
- 登录页可注册普通用户（默认角色 User）
- Admin：全部功能 | User：监测、查询、报告、导出
- 用户状态（启用/禁用）、角色切换、密码重置

## 快速开始

```powershell
cd SmartEnvironmentMonitoringSystem.App
dotnet build SmartEnvironmentMonitoringSystem.sln
dotnet run --project src\SmartEnvironmentMonitoringSystem.Wpf
```

程序启动后自动创建 SQLite 数据库（`data/environment_monitor.db`）并初始化种子数据。

详细使用说明（TCP 协议格式、模拟器启动、串口配置、发布部署等）请查看 [App 级 README](SmartEnvironmentMonitoringSystem.App/README.md)。

## 架构设计

项目采用严格的 MVVM 分层架构：

```
View (XAML) ──DataContext──> ViewModel (ObservableObject) ──调用──> Service (接口注入)
                                                                        │
                                                                        ├──> FreeSql (SQLite)
                                                                        ├──> TcpListener / SerialPort
                                                                        └──> ClosedXML (Excel)
```

- **View ↔ ViewModel**：通过 DataTemplate 自动匹配，使用 CommunityToolkit.Mvvm 的 `ObservableObject` 和 `RelayCommand`
- **ViewModel → Service**：全部通过 DI 注入接口，解耦具体实现
- **Service → Data**：FreeSql 作为 ORM，CodeFirst 模式自动同步表结构
- **跨线程调度**：通信事件在后台线程触发，通过 `Application.Current.Dispatcher.BeginInvoke` 回到 UI 线程

