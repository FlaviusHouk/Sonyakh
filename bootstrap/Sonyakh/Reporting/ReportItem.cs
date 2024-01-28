namespace Sonyakh.Reporting;

public sealed record ReportItem(ReportLevel Level, 
                                string InputName,
                                int RowNumber,
                                int CharNumber,
                                string Message);