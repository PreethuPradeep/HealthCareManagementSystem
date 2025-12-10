using HealthCareManagementSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection.Metadata;

namespace HealthCare.Services
{
    public class PdfService
    {
        // Existing Bill PDF method...
        public byte[] GenerateBillPdf(PharmacyBill bill)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Pharmacy Bill #{bill.PharmacyBillId}").FontSize(20).Bold();
                        col.Item().Text($"Date: {bill.BillDate:d}");
                        col.Item().Text($"Total: ${bill.Total}");
                    });
                });
            }).GeneratePdf();
        }

        // NEW: Generate Prescription PDF
        public byte[] GeneratePrescriptionPdf(Prescription prescription, IEnumerable<PrescriptionItem> items)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Hospital Management System").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Prescription").FontSize(15);
                        });
                    });

                    // Content
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Prescription ID: {prescription.PrescriptionId}");

                        col.Item().PaddingTop(10).LineHorizontal(1);

                        col.Item().PaddingTop(10).Text("Medicines:").Bold();

                        foreach (var item in items)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"• {item.Medicine?.Name ?? "Medicine"}");
                                row.RelativeItem().Text($"{item.MorningDose}-{item.NoonDose}-{item.EveningDose} ({item.MealTime})");
                                row.RelativeItem().Text($"{item.DurationInDays} Days");
                            });
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated automatically by system.");
                    });
                });
            }).GeneratePdf();
        }
    }
}