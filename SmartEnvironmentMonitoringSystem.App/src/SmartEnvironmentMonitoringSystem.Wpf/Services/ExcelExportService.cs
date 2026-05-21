using ClosedXML.Excel;
using SmartEnvironmentMonitoringSystem.Wpf.Entities;

namespace SmartEnvironmentMonitoringSystem.Wpf.Services;

public sealed class ExcelExportService : IExcelExportService
{
    private readonly IFreeSql freeSql;

    public ExcelExportService(IFreeSql freeSql)
    {
        this.freeSql = freeSql;
    }

    public async Task ExportAsync(DateTime startTime, DateTime endTime, string filePath)
    {
        var tempHumidityRecords = await freeSql.Select<TempHumidityRecordEntity>()
            .Where(x => x.CollectTime >= startTime && x.CollectTime <= endTime)
            .OrderBy(x => x.CollectTime)
            .ToListAsync();

        var airQualityRecords = await freeSql.Select<AirQualityRecordEntity>()
            .Where(x => x.CollectTime >= startTime && x.CollectTime <= endTime)
            .OrderBy(x => x.CollectTime)
            .ToListAsync();

        var alarmRecords = await freeSql.Select<AlarmRecordEntity>()
            .Where(x => x.AlarmTime >= startTime && x.AlarmTime <= endTime)
            .OrderBy(x => x.AlarmTime)
            .ToListAsync();

        var reports = await freeSql.Select<MonitorReportEntity>()
            .Where(x => x.StartTime >= startTime && x.EndTime <= endTime)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        WriteTempHumiditySheet(workbook, tempHumidityRecords);
        WriteAirQualitySheet(workbook, airQualityRecords);
        WriteAlarmSheet(workbook, alarmRecords);
        WriteReportSheet(workbook, reports);
        workbook.SaveAs(filePath);
    }

    private static void WriteTempHumiditySheet(XLWorkbook workbook, IReadOnlyList<TempHumidityRecordEntity> records)
    {
        var sheet = workbook.Worksheets.Add("温湿度数据");
        WriteHeaders(sheet, ["时间", "传感器编号", "温度", "湿度", "是否告警"]);

        for (var i = 0; i < records.Count; i++)
        {
            var row = i + 2;
            var record = records[i];
            sheet.Cell(row, 1).Value = record.CollectTime;
            sheet.Cell(row, 2).Value = record.SensorId;
            sheet.Cell(row, 3).Value = record.Temperature;
            sheet.Cell(row, 4).Value = record.Humidity;
            sheet.Cell(row, 5).Value = record.IsAlarm ? "是" : "否";
        }

        FormatSheet(sheet, 5);
    }

    private static void WriteAirQualitySheet(XLWorkbook workbook, IReadOnlyList<AirQualityRecordEntity> records)
    {
        var sheet = workbook.Worksheets.Add("空气质量数据");
        WriteHeaders(sheet, ["时间", "传感器编号", "烟雾浓度", "CO2 浓度", "状态等级", "是否告警"]);

        for (var i = 0; i < records.Count; i++)
        {
            var row = i + 2;
            var record = records[i];
            sheet.Cell(row, 1).Value = record.CollectTime;
            sheet.Cell(row, 2).Value = record.SensorId;
            sheet.Cell(row, 3).Value = record.SmokePpm;
            sheet.Cell(row, 4).Value = record.Co2Ppm;
            sheet.Cell(row, 5).Value = record.Level;
            sheet.Cell(row, 6).Value = record.IsAlarm ? "是" : "否";
        }

        FormatSheet(sheet, 6);
    }

    private static void WriteAlarmSheet(XLWorkbook workbook, IReadOnlyList<AlarmRecordEntity> records)
    {
        var sheet = workbook.Worksheets.Add("告警记录");
        WriteHeaders(sheet, ["时间", "设备编号", "告警类型", "实际值", "阈值", "等级", "是否处理"]);

        for (var i = 0; i < records.Count; i++)
        {
            var row = i + 2;
            var record = records[i];
            sheet.Cell(row, 1).Value = record.AlarmTime;
            sheet.Cell(row, 2).Value = record.SensorId;
            sheet.Cell(row, 3).Value = record.AlarmType;
            sheet.Cell(row, 4).Value = record.ActualValue;
            sheet.Cell(row, 5).Value = record.ThresholdDescription;
            sheet.Cell(row, 6).Value = record.AlarmLevel;
            sheet.Cell(row, 7).Value = record.IsHandled ? "是" : "否";
        }

        FormatSheet(sheet, 7);
    }

    private static void WriteReportSheet(XLWorkbook workbook, IReadOnlyList<MonitorReportEntity> records)
    {
        var sheet = workbook.Worksheets.Add("监测报告");
        WriteHeaders(
            sheet,
            [
                "报告编号",
                "开始时间",
                "结束时间",
                "最高温度",
                "最低温度",
                "平均温度",
                "最高湿度",
                "最低湿度",
                "平均湿度",
                "最高烟雾",
                "平均烟雾",
                "最高 CO2",
                "平均 CO2",
                "告警次数",
                "危险告警次数",
                "综合评价",
                "生成时间",
                "生成人"
            ]);

        for (var i = 0; i < records.Count; i++)
        {
            var row = i + 2;
            var record = records[i];
            sheet.Cell(row, 1).Value = record.ReportNo;
            sheet.Cell(row, 2).Value = record.StartTime;
            sheet.Cell(row, 3).Value = record.EndTime;
            sheet.Cell(row, 4).Value = record.MaxTemperature;
            sheet.Cell(row, 5).Value = record.MinTemperature;
            sheet.Cell(row, 6).Value = record.AvgTemperature;
            sheet.Cell(row, 7).Value = record.MaxHumidity;
            sheet.Cell(row, 8).Value = record.MinHumidity;
            sheet.Cell(row, 9).Value = record.AvgHumidity;
            sheet.Cell(row, 10).Value = record.MaxSmokePpm;
            sheet.Cell(row, 11).Value = record.AvgSmokePpm;
            sheet.Cell(row, 12).Value = record.MaxCo2Ppm;
            sheet.Cell(row, 13).Value = record.AvgCo2Ppm;
            sheet.Cell(row, 14).Value = record.AlarmCount;
            sheet.Cell(row, 15).Value = record.DangerAlarmCount;
            sheet.Cell(row, 16).Value = record.Evaluation;
            sheet.Cell(row, 17).Value = record.CreatedAt;
            sheet.Cell(row, 18).Value = record.CreatedBy;
        }

        FormatSheet(sheet, 18);
    }

    private static void WriteHeaders(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }
    }

    private static void FormatSheet(IXLWorksheet sheet, int columnCount)
    {
        var headerRange = sheet.Range(1, 1, 1, columnCount);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E7EEF8");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Columns(1, columnCount).AdjustToContents();
        sheet.SheetView.FreezeRows(1);
    }
}
