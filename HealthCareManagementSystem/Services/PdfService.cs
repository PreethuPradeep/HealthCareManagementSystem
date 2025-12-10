using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
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
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Hospital Management System")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                        col.Item().Text($"Prescription ID: {prescription.PrescriptionId}")
                            .FontSize(12);

                        col.Item().Text($"Date: {DateTime.UtcNow:d}")
                            .FontSize(12);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        col.Item().Text("Medicines").Bold().FontSize(14);

                        foreach (var item in items)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"• {item.Medicine?.Name ?? "Unknown"}");
                                row.RelativeItem().Text($"{item.MorningDose}-{item.NoonDose}-{item.EveningDose} ({item.MealTime})");
                                row.RelativeItem().Text($"{item.DurationInDays} Days");
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text("Generated automatically by system.");
                });
            })
            .GeneratePdf();
        }

    }
}