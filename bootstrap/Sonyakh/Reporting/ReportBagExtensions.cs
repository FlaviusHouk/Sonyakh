using System.Collections.Generic;
using System.Linq;

namespace Sonyakh.Reporting;

public static class ReportBagExtensions
{
    public static bool IsFailed(this IEnumerable<ReportItem> reports) =>
        reports.Any(r => r.Level == ReportLevel.Error);

    public static void ReportError(this ICollection<ReportItem> reports, 
                                   string message,
                                   string inputName,
                                   int rowPos = -1,
                                   int charPos = -1)
    {
        reports.Add(new (ReportLevel.Error,
                         inputName,
                         rowPos,
                         charPos,
                         message));
    }

    public static void ReportWarning(this ICollection<ReportItem> reports, 
                                     string message,
                                     string inputName,
                                     int rowPos = -1,
                                     int charPos = -1)
    {
        reports.Add(new (ReportLevel.Warning,
                         inputName,
                         rowPos,
                         charPos,
                         message));
    }

    public static void ReportInfo(this ICollection<ReportItem> reports, 
                                  string message,
                                  string inputName,
                                  int rowPos = -1,
                                  int charPos = -1)
    {
        reports.Add(new (ReportLevel.Info,
                         inputName,
                         rowPos,
                         charPos,
                         message));
    }

    public static void ReportDiagnostic(this ICollection<ReportItem> reports, 
                                        string message,
                                        string inputName,
                                        int rowPos = -1,
                                        int charPos = -1)
    {
        reports.Add(new (ReportLevel.Diagnostic,
                         inputName,
                         rowPos,
                         charPos,
                         message));
    }
}