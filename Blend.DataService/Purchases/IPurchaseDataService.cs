using System;
using System.Collections.Generic;
using Blend.DataService.Documents;

namespace Blend.DataService.Purchases;

public interface IPurchaseDataService
{
    IReadOnlyList<PurchaseSaleRecord> GetSales(string search, int companyId);
    IReadOnlyList<PurchaseLineRecord> GetLinesBySale(int saleId);
    IReadOnlyList<PurchaseLineRecord> GetLinesByReservedNo(long id);
    IReadOnlyList<TeaCertificateRecord> GetTeaCertificates();
    bool CreateSale(PurchaseSaleRecord sale);
    bool UpdateSale(PurchaseSaleRecord sale);
    bool SetSaleClosed(PurchaseSaleRecord sale, bool closed);
    string AddLines(int companyId, PurchaseLineRecord first, PurchaseLineRecord? repeated, Func<DocumentRecord, string> formatDocumentNumber);
    bool UpdateLines(PurchaseLineRecord first, PurchaseLineRecord? repeated);
    bool DeleteLine(PurchaseLineRecord line);
}
