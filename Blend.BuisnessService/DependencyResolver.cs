using System.ComponentModel.Composition;
using Blend.Resolver;
using Blend.BuisnessService.TeaBuyer;
using Blend.BuisnessService.User;
using Blend.BuisnessService.Menu;
using BusinessServices;
using Blend.BuisnessService.Broker;
using Blend.BuisnessService.Purchase;
using Blend.BuisnessService.Document;
using Blend.BuisnessService.Contract;
using Blend.BuisnessService.BlendCreation;
using Blend.BuisnessService.BlendClose;
using Blend.BuisnessService.TeaStandards;
using Blend.BuisnessService.AccessRights.Roles;
using Blend.BuisnessService.RejectLine;
using Blend.BuisnessService.Adjustent;
using Blend.BuisnessService.Approvals;
using Blend.BuisnessService.Reports.Blend;
using Blend.BuisnessService.Company;
using Blend.BuisnessService.Reports.Purchase;
using Blend.BuisnessService.Reports.Stock;
using Blend.DataModel.DataOperation;
using Blend.BuisnessService.Download;
using Blend.BuisnessService.Helper.Email;
using Blend.BuisnessService.TransactionOwners;
using Blend.BuisnessService.TeaCollection;
using Blend.BuisnessService.GardenMarks;
using Blend.BuisnessService.BlendRateConvert;
using Blend.BuisnessService.LineSummary;
using Blend.BuisnessService.Shipments;
using Blend.BuisnessService.Item;
using Blend.BuisnessService.PackGoods;
using Blend.BuisnessService.StoresInstruction;
using Blend.BuisnessService.Grades;
using Blend.BuisnessService.Packing.PO;
using Blend.BuisnessService.UploadSale;
using Blend.BuisnessService.Packing.UOM;
using Blend.BuisnessService.Packing.ItemReceive;
using Blend.BuisnessService.Packing.ItemIssue;
using Blend.BuisnessService.Packing.ItemReturn;

namespace Blend.BuisnessService
{
    [Export(typeof(IComponent))]
    public class DependencyResolver : IComponent
    {
        public void SetUp(IRegisterComponent registerComponent)
        {
            registerComponent.RegisterType<IBuyerService, BuyerManagement>();
            registerComponent.RegisterType<IUserRegistration, UserRegistrationService>();
            registerComponent.RegisterType<IUserService, UserService>();
            registerComponent.RegisterType<IMenuService, MenuService>();
            registerComponent.RegisterType<ITokenServices, TokenServices>();
            registerComponent.RegisterType<IBrokerService, BrokerService>();
            registerComponent.RegisterType<IPurchaseService, PurchaseService>();
            registerComponent.RegisterType<IDocumentService, DocumentService>();
            registerComponent.RegisterType<IContractService, ContractService>();
            registerComponent.RegisterType<IBlendCreationService, BlendCreationService>();
            registerComponent.RegisterType<IBlendCloseService, BlendCloseService>();
            registerComponent.RegisterType<ITeaStandardService, TeaStandardService>();
            registerComponent.RegisterType<IRoleService, RoleService>();
            registerComponent.RegisterType<IAdjustmentService, AdjustmentService>();
            registerComponent.RegisterType<IAdjustmentCancelService, AdjustmentCancelService>();
            registerComponent.RegisterType<IBlendReportService, BlendReportService>();
            registerComponent.RegisterType<ICompanyService, CompanyService>();
            registerComponent.RegisterType<IPurchaseReportService, PurchaseReportService>();
            registerComponent.RegisterType<IStockReportService, StockReportService>();
            registerComponent.RegisterType<IDownloadFileService, DownloadFilesService>();
            registerComponent.RegisterType<IEmailService, EmailService>();
            registerComponent.RegisterType<ITransactionOwnersService, TransactionOwnersService>();
            registerComponent.RegisterType<ITeaCollectionService, TeaCollectionService>();
            registerComponent.RegisterType<IGardenMarkService, GardenMarkService>();
            registerComponent.RegisterType<IBlendGainLossConvertService, BlendGainLossConvertService>();
            registerComponent.RegisterType<ILineSummaryService, LineSummaryService>();
            registerComponent.RegisterType<ITeaShipmentService, TeaShipmentService>();
            registerComponent.RegisterType<IItemService, ItemService>();
            registerComponent.RegisterType<IPackGoodService, PackGoodService>();
            registerComponent.RegisterType<ISupplierService, SupplierService>();
            registerComponent.RegisterType<IStoresInstructionService, StoresInstructionService>();
            registerComponent.RegisterType<IGradeService, GradeService>();
            registerComponent.RegisterType<IPurchaseOrderService, PurchaseOrderService>();
            registerComponent.RegisterType<ISaleUploadService, SaleUploadService>();
            registerComponent.RegisterType<IUomService, UomService>();
            registerComponent.RegisterType<IItemReceiveService, ItemReceiveService>();
            registerComponent.RegisterType<IItemIssueService, ItemIssueService>();
            registerComponent.RegisterType<IItemRetunService, ItemReturnService>();

            

        }
    }
}
