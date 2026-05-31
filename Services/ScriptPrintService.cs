using BayBrain.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BayBrain.Services
{
    /// <summary>
    /// Handles printing and HTML export of generated customer scripts.
    /// </summary>
    public class ScriptPrintService
    {
        // ── Print via WPF PrintDialog ────────────────────────────────────

        public void PrintScript(ServiceItem service, UrgencyScore urgency, string script, string customerName)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() != true) return;

            var doc = BuildFlowDocument(service, urgency, script, customerName);
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            paginator.PageSize = new System.Windows.Size(
                dlg.PrintableAreaWidth,
                dlg.PrintableAreaHeight);

            dlg.PrintDocument(paginator,
                $"BayBrain Script — {service.Name}");
        }

        // ── Export to HTML ───────────────────────────────────────────────

        public string ExportToHtml(ServiceItem service, UrgencyScore urgency,
                                   string script, string customerName)
        {
            var date = DateTime.Now.ToString("MMMM d, yyyy h:mm tt");
            var urgencyColor = urgency.ColorHex;
            var lines = script.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var paragraphs = string.Join("\n",
                lines.Select(l => $"        <p>{System.Web.HttpUtility.HtmlEncode(l.Trim())}</p>"));

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
<title>BayBrain Script — {HtmlEncode(service.Name)}</title>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f4f6; color: #1a1a1a; padding: 40px 20px; }}
  .card {{ background: #fff; border-radius: 16px; max-width: 760px; margin: 0 auto;
           padding: 40px 48px; box-shadow: 0 4px 24px rgba(0,0,0,0.10); }}
  .header {{ border-bottom: 2px solid #e5e5ea; padding-bottom: 24px; margin-bottom: 28px; }}
  .logo {{ font-size: 13px; font-weight: 700; letter-spacing: 0.08em;
           color: #636366; text-transform: uppercase; margin-bottom: 12px; }}
  h1 {{ font-size: 26px; font-weight: 700; color: #1a1a1a; margin-bottom: 6px; }}
  .meta {{ font-size: 13px; color: #8e8e93; }}
  .badge {{ display: inline-block; padding: 4px 12px; border-radius: 20px;
            font-size: 12px; font-weight: 700; color: #fff;
            background: {urgencyColor}; margin-left: 12px; vertical-align: middle; }}
  .score {{ font-size: 13px; color: #636366; margin-top: 10px; }}
  .section {{ margin-bottom: 28px; }}
  .section-title {{ font-size: 10px; font-weight: 700; letter-spacing: 0.1em;
                    text-transform: uppercase; color: #636366; margin-bottom: 12px; }}
  .script-box {{ background: #f9f9fb; border-left: 4px solid {urgencyColor};
                 border-radius: 0 12px 12px 0; padding: 24px 28px; }}
  .script-box p {{ font-size: 15px; line-height: 1.75; color: #1a1a1a;
                   font-style: italic; margin-bottom: 12px; }}
  .script-box p:last-child {{ margin-bottom: 0; }}
  .details {{ display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-top: 28px; }}
  .detail-box {{ background: #f4f4f6; border-radius: 12px; padding: 16px 20px; }}
  .detail-box .label {{ font-size: 10px; font-weight: 700; text-transform: uppercase;
                        letter-spacing: 0.08em; color: #636366; margin-bottom: 8px; }}
  .detail-box .value {{ font-size: 13px; color: #1a1a1a; line-height: 1.6; }}
  .consequence .value {{ color: #ff6b00; }}
  .footer {{ margin-top: 36px; padding-top: 20px; border-top: 1px solid #e5e5ea;
             font-size: 11px; color: #8e8e93; display: flex; justify-content: space-between; }}
  @media print {{
    body {{ background: #fff; padding: 0; }}
    .card {{ box-shadow: none; border-radius: 0; max-width: 100%; padding: 32px; }}
  }}
</style>
</head>
<body>
<div class=""card"">
  <div class=""header"">
    <div class=""logo"">🧠 BayBrain · Service Advisor Intelligence</div>
    <h1>{HtmlEncode(service.Name)}
      <span class=""badge"">{HtmlEncode(urgency.Label)}</span>
    </h1>
    <div class=""meta"">
      {(string.IsNullOrWhiteSpace(customerName) ? "Customer Script" : HtmlEncode(customerName))}
      &nbsp;·&nbsp; {HtmlEncode(service.Category)}
      &nbsp;·&nbsp; {HtmlEncode(service.PriceRange)}
      &nbsp;·&nbsp; {HtmlEncode(service.EstimatedMinutes.ToString())} min
    </div>
    <div class=""score"">Urgency Score: {urgency.Score}/10 &nbsp;·&nbsp; {HtmlEncode(urgency.Reasoning)}</div>
  </div>

  <div class=""section"">
    <div class=""section-title"">Customer Script</div>
    <div class=""script-box"">
{paragraphs}
    </div>
  </div>

  <div class=""details"">
    <div class=""detail-box"">
      <div class=""label"">⏱ Service Interval</div>
      <div class=""value"">{HtmlEncode(service.Interval)}</div>
    </div>
    <div class=""detail-box consequence"">
      <div class=""label"">⚠ If Skipped</div>
      <div class=""value"">{HtmlEncode(service.SkipConsequence)}</div>
    </div>
    <div class=""detail-box"">
      <div class=""label"">💡 Why It Matters</div>
      <div class=""value"">{HtmlEncode(service.WhyItMatters)}</div>
    </div>
    <div class=""detail-box"">
      <div class=""label"">📋 Service Detail</div>
      <div class=""value"">{HtmlEncode(service.ShortDescription)}</div>
    </div>
  </div>

  <div class=""footer"">
    <span>Generated by BayBrain v1.0 &nbsp;·&nbsp; {HtmlEncode(date)}</span>
    <span>For advisor use only</span>
  </div>
</div>
</body>
</html>";
        }

        public bool SaveHtmlToFile(string html, string suggestedName)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Script as HTML",
                Filter = "HTML File (*.html)|*.html|All Files (*.*)|*.*",
                FileName = suggestedName,
                DefaultExt = ".html"
            };

            if (dlg.ShowDialog() != true) return false;

            File.WriteAllText(dlg.FileName, html, System.Text.Encoding.UTF8);

            // Open in default browser
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dlg.FileName,
                UseShellExecute = true
            }); } catch { /* non-critical */ }

            return true;
        }

        // ── FlowDocument for WPF print ───────────────────────────────────

        private static FlowDocument BuildFlowDocument(ServiceItem service, UrgencyScore urgency,
                                                       string script, string customerName)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PagePadding = new Thickness(60, 48, 60, 48),
                ColumnWidth = double.PositiveInfinity
            };

            // Header
            var header = new Paragraph
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 12),
                Margin = new Thickness(0, 0, 0, 16)
            };
            header.Inlines.Add(new Run("🧠 BayBrain · Service Advisor Intelligence")
                { Foreground = Brushes.Gray, FontSize = 9 });
            doc.Blocks.Add(header);

            // Title
            var title = new Paragraph(new Run(service.Name))
            {
                FontSize = 22, FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            doc.Blocks.Add(title);

            // Meta line
            var metaColor = (Color)ColorConverter.ConvertFromString(urgency.ColorHex);
            var meta = new Paragraph { Margin = new Thickness(0, 0, 0, 16) };
            meta.Inlines.Add(new Run($"{urgency.Label}  ·  {service.Category}  ·  {service.PriceRange}  ·  {service.EstimatedMinutes} min")
                { Foreground = new SolidColorBrush(metaColor), FontWeight = FontWeights.SemiBold });
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                meta.Inlines.Add(new LineBreak());
                meta.Inlines.Add(new Run(customerName) { Foreground = Brushes.Gray, FontSize = 11 });
            }
            doc.Blocks.Add(meta);

            // Section: Script
            doc.Blocks.Add(SectionLabel("CUSTOMER SCRIPT"));

            var scriptBox = new Section
            {
                Background = Brushes.WhiteSmoke,
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 16),
                BorderBrush = new SolidColorBrush(metaColor),
                BorderThickness = new Thickness(3, 0, 0, 0)
            };

            foreach (var line in script.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                scriptBox.Blocks.Add(new Paragraph(
                    new Run(line.Trim()) { FontStyle = FontStyles.Italic })
                { Margin = new Thickness(0, 0, 0, 8) });
            }
            doc.Blocks.Add(scriptBox);

            // Details grid (as table)
            doc.Blocks.Add(SectionLabel("SERVICE DETAILS"));
            var table = new Table { CellSpacing = 8 };
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            void AddRow(string label1, string val1, string label2, string val2)
            {
                var row = new TableRow();
                row.Cells.Add(DetailCell(label1, val1));
                row.Cells.Add(DetailCell(label2, val2));
                rowGroup.Rows.Add(row);
            }

            AddRow("⏱ Interval", service.Interval, "⚠ If Skipped", service.SkipConsequence);
            AddRow("💡 Why It Matters", service.WhyItMatters, "📋 Short Description", service.ShortDescription);
            doc.Blocks.Add(table);

            // Footer
            var footer = new Paragraph
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 10, 0, 0),
                Margin = new Thickness(0, 20, 0, 0)
            };
            footer.Inlines.Add(new Run($"BayBrain v1.0  ·  Generated {DateTime.Now:MMMM d, yyyy h:mm tt}  ·  For advisor use only")
                { Foreground = Brushes.Gray, FontSize = 9 });
            doc.Blocks.Add(footer);

            return doc;
        }

        private static Paragraph SectionLabel(string text) => new(new Run(text))
        {
            FontSize = 9, FontWeight = FontWeights.Bold,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 6)
        };

        private static TableCell DetailCell(string label, string value)
        {
            var cell = new TableCell
            {
                Background = Brushes.WhiteSmoke,
                Padding = new Thickness(10)
            };
            var block = new Section();
            block.Blocks.Add(new Paragraph(new Run(label))
                { FontSize = 8, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray,
                  Margin = new Thickness(0, 0, 0, 4) });
            block.Blocks.Add(new Paragraph(new Run(value))
                { FontSize = 11, Margin = new Thickness(0) });
            cell.Blocks.Add(block);
            return cell;
        }

        private static string HtmlEncode(string? s) =>
            System.Web.HttpUtility.HtmlEncode(s ?? string.Empty);
    }
}
