export function downloadPdf(saleId, bytes) {
    const url = URL.createObjectURL(new Blob([bytes], { type: "application/pdf" }));
    const link = document.createElement("a");
    link.href = url;
    link.download = `sales-summary-${saleId}.pdf`;
    link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}
