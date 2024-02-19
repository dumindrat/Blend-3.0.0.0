delete from [dbo].[purchasereturn_detail]
delete from [dbo].[purchasereturn_header]
delete from [dbo].[purchasereceived_detail]
delete from [dbo].[purchasereceived_header]

delete from purchase_detail
delete from purchase_header
delete from blend_detail
delete from dbo.blend_additiondeletion
delete from teastock
delete from dbo.blendcategorize_cancellation
delete from blendcategorize
delete from dbo.contractdetail
delete from dbo.blendcategorize_log
delete from dbo.contract
delete from dbo.standard
truncate table [x_saledetails]
truncate table x_saledates
--select * from x_saledates
delete from dbo.contractdetail
--select * from [x_saledetails]
go
DBCC CHECKIDENT('purchase_header', RESEED, 0)
go
DBCC CHECKIDENT('purchase_detail', RESEED, 0)
go
DBCC CHECKIDENT('contract', RESEED, 0)
go
DBCC CHECKIDENT('standard', RESEED, 0)
go
DBCC CHECKIDENT('blendcategorize', RESEED, 0)
go
DBCC CHECKIDENT('teastock', RESEED, 0)
go
DBCC CHECKIDENT('blend_detail', RESEED, 0)
go
DBCC CHECKIDENT('blend_header', RESEED, 0)
go
INSERT INTO [dbo].[x_saledetails]
           ([perAutoKey],[nvrPurchKeyAuto],[nchPurchBoxNum],[datPurchSalDate],[chrPurchSalType],[strPurchBkCode]
           ,[chrPurchSalLocation],[intPurchLotNum],[intPurchInvNum],[strPurchGardenMrk],[strStandard],[chrPurchArea]
           ,[strPurchGrade],[intPurchNoPack],[chrPurchPackTyp],[nvrPurchNett],[nvrPurchNettWtK],[nvrPurchRate]
           ,[nvrPurchValue],[booPurchDel],[intSaleCode],[intRedQty],[AccountName],[TinNo],[panel],[StockReceived]
           ,[CollectionDate],[StockReceivedDate],[StoreLocation],[CollectionNoteNo],[Saled],[BrokerId])
select [perAutoKey],[nvrPurchKeyAuto],[nchPurchBoxNum],[datPurchSalDate],[chrPurchSalType],[strPurchBkCode]
           ,[chrPurchSalLocation],[intPurchLotNum],[intPurchInvNum],[strPurchGardenMrk],[strStandard],[chrPurchArea]
           ,[strPurchGrade],[intPurchNoPack],[chrPurchPackTyp],[nvrPurchNett],[nvrPurchNettWtK],[nvrPurchRate]
           ,[nvrPurchValue],[booPurchDel],[intSaleCode],[intRedQty],[AccountName],[TinNo],[panel],[StockReceived]
           ,[CollectionDate],[StockReceivedDate],[StoreLocation],[CollectionNoteNo],0,0 
           from [continentalnew].[dbo].tblPurchaseGenDetail  order by perAutoKey

go
insert into purchase_header (CompanyId, SaleDate,IsClosed,CreateUser,ClosedUser,ModifiedDate)
select distinct 1,datPurchsalDate,bitSaleClose,'migrate','migrate',getdate() from  continentalnew.dbo.tblPurchaseGenHeader
go
--This is for sale dates where dates in detail and not in header
insert into dbo.x_saledates(F1)
select distinct datPurchsalDate from [x_saledetails] where datPurchsalDate 
not in (select distinct datPurchsalDate from continentalnew.dbo.tblPurchaseGenHeader ) order by datPurchsalDate
go
insert into purchase_header(SaleDate,IsClosed,CreateUser,ClosedUser,ModifiedDate)
select F1,1,'Dumindra','Dumindra',getdate() from x_saledates
go

------------------------------------------------------------------
delete from broker
DBCC CHECKIDENT('broker', RESEED, 0)
insert into broker(CompanyId, brokercode,brokername,address,modifieddate,createuser,createdate,ModifiedUser)
select 1,strbkcode,strbkname,strbkaddress,getdate(),'migrate',getdate(),'migrate' from ContinentalNew.dbo.tblbrockerdef

update [dbo].[x_saledetails] set Saled= h.Id from [dbo].[x_saledetails] x 
inner join purchase_header h on x.[datPurchSalDate] = h.SaleDate
go
update [dbo].[x_saledetails] set BrokerId= h.BrokerId from [dbo].[x_saledetails] x 
inner join broker h on x.[strPurchBkCode] = h.BrokerCode
go

INSERT INTO [DB_tmxcontenental].[dbo].[purchase_detail]
([BoxNo],[SaleId],[SaleCode],[BrokerId],[Invoice],[LotNo],[SaleType],[Grade],[Standard],[GardenMark]
,[TeaCertificates] ,[Packs],[NetWeight],[Weight]           ,[ReducedQty]           ,[Rate]           ,[Value]
,[TinNo]           ,[IsVoid]           ,[CreateUser]           ,[CreateDate]           ,[ModifiedUser]
,[ModifiedDate]           ,[IsRepeated]           ,[VirtualWeight]           ,[VirtualValue]           ,[mapKey])

SELECT [nchPurchBoxNum]     ,[saled]      ,[intSaleCode]     ,[BrokerId]      ,[intPurchInvNum]      ,[intPurchLotNum]
,[chrPurchSalType]      ,[strPurchGrade]      ,[strStandard]      ,[strPurchGardenMrk]      ,null      ,[intPurchNoPack]
,[nvrPurchNett]      ,[nvrPurchNettWtK]      ,[intRedQty]      ,[nvrPurchRate]      ,[nvrPurchValue]      ,null
,0      ,'dumindra'     ,getdate()     ,'dumindra'     ,getdate()     ,0      ,[nvrPurchNettWtK]      ,[nvrPurchValue]
,[perautokey] 
FROM [DB_tmxcontenental].[dbo].[x_saledetails] where brokerid <>0
order by  [perautokey]

go
update purchase_detail set isVoid = 1 
where BoxNo in (select nchPurchBoxNum from dbo.x_saledetails where boopurchdel = 1)

go



INSERT INTO [DB_tmxcontenental].[dbo].[contract]
(CompanyId, [BuyerId]           ,[ContractName]           ,[CompletionDate]           ,[CreateUser]           ,[ModifiedUser]
,[CreateDate]           ,[ModifiedDate]           ,[IsClosed])
select 1,1,
[strOrdContractNum]      ,isnull([ComplitedDate],getdate())      ,'dumindra'      ,'dumindra'      ,getdate()
,getdate()      ,[bitOrdComplitConNum]  FROM [continentalnew].[dbo].[tblOrdGenHeader]
go

INSERT INTO [DB_tmxcontenental].[dbo].[standard]
           (CompanyId, [StandardName]
           ,[CreateUser]
           ,[CreateDate]
           ,[ModifiedUser]
           ,[ModifiedDate])
select  1,strOrdStandard,'migrate',getdate(),'migrate',getdate()
 from [continentalnew].dbo.tblOrdGenDetail group by strOrdStandard
go

insert into [DB_tmxcontenental].dbo.contractdetail (contractId,StandardId,OrderQty,CreateDate,LastUpdateDate,CreateUser)
select c.ContractId,s.StandardId,fltOrdExpQty,getdate(),getdate(),'migrate' from continentalnew.dbo.tblOrdGenDetail d inner join 
[DB_tmxcontenental].dbo.contract c on d.strOrdContractNum = c.ContractName inner join 
[DB_tmxcontenental].dbo.standard s on d.strOrdStandard = s.StandardName

go
delete from blend_detail
delete from dbo.blendcategorize_cancellation
delete from dbo.blendcategorize
delete from dbo.storesinstructions
delete from blend_header
delete  from teastock
DBCC CHECKIDENT('blend_detail', RESEED, 0)
DBCC CHECKIDENT('blendcategorize_cancellation', RESEED, 0)
DBCC CHECKIDENT('blendcategorize', RESEED, 0)
DBCC CHECKIDENT('blend_header', RESEED, 0)
DBCC CHECKIDENT('teastock', RESEED, 0)

go


insert into teastock ( StockItemId,ItemName,StockName,ItemType,Packs,Qty,ReceivedVia)
select p.Id,strStkBoxNo,strStkBoxNo,'L',intStkNoOfCs,nvrStkNW,'sys' from continentalnew.dbo.tblStockMain s inner join purchase_detail p on
s.perAutoKey = p.mapkey and s.chrStkStockType = 'L' order by s.perAutoKey 
go

update teastock set Packs = 0,Qty=0 where StockItemId in
 (Select id from purchase_detail where IsVoid = 1 ) and ItemType = 'L'
go

INSERT INTO [DB_tmxcontenental].[dbo].[blend_header]
(CompanyId, [BlendNo]           ,[Contract]           ,[Standard]           ,[ExportQty]           ,[IsClosed]
,[DateOfCompletion]           ,[ClosedDate]           ,[CreateUser]           ,[CreateDate]
,[ModifiedUser]           ,[ModifiedDate])
SELECT 
1,[strBlNumber]		,c.ContractId		,s.StandardId      ,[fltBlOrdQty]      ,[booBlCreated]
,isnull([CreatedDate],getdate())      ,[CreatedDate]      ,'dumindra'      ,isnull([CreatedDate],getdate())      ,'dumindra'
,[CreatedDate]

FROM continentalnew.[dbo].[tblBlendCreationGenHeader] h 
inner join [DB_tmxcontenental].dbo.contract c on   h.strOrdContractNum = c.ContractName
inner join [DB_tmxcontenental].dbo.standard s on h.[strBLCStandard] = s.StandardName
order by h.createdDate,strblnumber 
GO

INSERT INTO [DB_tmxcontenental].[dbo].[teastock]
           ( [StockItemId],[ItemName],[StockName],[ItemType],[Packs]     ,[Qty]           ,[ReceivedVia])

SELECT b.Id      ,[strStkBoxNo]      ,[strStkBoxNo]      ,[strBlendCategory]      ,[intStkNoOfCS]      ,[nvrStkNW]      ,'sys'
  FROM continentalnew.[dbo].[tblStockMain] s 
  inner join [DB_tmxcontenental].dbo.blend_header b on s.strBlendId = b.BlendNo
   where strBlendId <>'L' 

go


INSERT INTO [DB_tmxcontenental].[dbo].[blend_detail]
           ([BlendId] ,[StockId] ,[ItemName]  ,[Packs]    ,[Qty]       ,[CreateUser]         ,[CreateDate])
     select h.id,s.id,bd.nchblcboxnum,intblcusedbags,fltblcrequiredqty,'migrate',updatedate from continentalnew.dbo.tblBlendCreationGenDetail bd inner join
[DB_tmxcontenental].dbo.purchase_detail d on d.MapKey = bd.perautokey and bd.perautokey>0 inner join
[DB_tmxcontenental].dbo.teastock s on d.ID = s.stockItemId and s.ItemType = 'L' inner join
[DB_tmxcontenental].dbo.blend_header h on bd.strBlcnumber = h.BlendNo
 union all
select h.id,s.id,bd.nchblcboxnum,intblcusedbags,fltblcrequiredqty,'migrate',updatedate from continentalnew.dbo.tblBlendCreationGenDetail bd inner join
[DB_tmxcontenental].dbo.teastock s on bd.nchblcboxnum = s.ItemName and s.ItemType <> 'L' inner join
[DB_tmxcontenental].dbo.blend_header h on bd.strBlcnumber = h.BlendNo 

go


INSERT INTO [DB_tmxcontenental].[dbo].[blendcategorize]
           ( [BlendId]         ,[LineQty]          ,[CreateQty]          ,[Rate]        ,[ClosedRate]         ,[BB]
           ,[BG]           ,[BL]           ,[STDBB]           ,[STDBG]           ,[SIFTED]           ,[SFTBB]
           ,[SFTBG]           ,[ClosedDate]           ,[IsConverted]           ,[IsCancelRequest]           ,[CreateUser]
           ,[CreateDate]           ,[ModifiedUser]           ,[ModifiedDate])
     SELECT 
      h.[Id]      ,[fltLineQty]      ,[fltCreatedQty]      ,[fltRate]      ,[afterconRate]      ,[fltBlBB]      ,[fltBG]
      ,0      ,[fltSTDBB]      ,[fltSTDBG]      ,[fltSFTtot]      ,[fltSFTBB]      ,[fltSFTBG]      ,[datCreatedDate]
      ,[booBConvert]      ,0      ,'migrate'      ,getdate()      ,'migrate'      ,getdate()
  FROM [continentalnew].[dbo].[tblBlendCategorizing] c inner join 
[DB_tmxcontenental].dbo.blend_header h on c.[strBlNumber] = h.BlendNo

go
delete from shipments
DBCC CHECKIDENT('shipments', RESEED, 0)
go
insert into shipments ( ShipmentItemId,Qty,Bags,CreateDate,CreateUser,TransType,ExportType)
select h.Id,floShipQty,floExportBage,datShipShipmentDate ,'migrate','SHIP','BULK' from  continentalnew.dbo.tblShipments s 
inner join blend_header h on h.BlendNo = s.strBlendNum order by AutoKey
go

update dbo.blendcategorize set BL = LineQTy-CreateQty where CreateQty<LineQTy and CreateQty>0
go

update  dbo.blendcategorize  set IsConverted =1,ClosedRate = ab.afterConRate from dbo.blendcategorize ct inner join 
(select id,c.afterConRate from blend_header h inner join [continentalnew].[dbo].tblBlendCategorizing c on h.BlendNo = c.strBlNumber  
and (c.boolConvert = 1 or c.booBConvert = 1) and c.afterConRate>0 ) ab on ct.blendid = ab.id

go

delete from [lineadjustment]
go
DBCC CHECKIDENT('lineadjustment', RESEED, 0)
go
INSERT INTO [DB_tmxcontenental].[dbo].[lineadjustment]
           ( [PurchaseId]           ,[Stockid]           ,[Packs]           ,[Qty]           ,[AdjustmentType]
           ,[Reason]           ,[CreateUser]           ,[CreateDate]           ,[Status]           ,[VoidReason]
           ,[VoidApproved]           ,[ModifiedUser]           ,[ModifiedDate])


select p.id,s.id,0,r.Qty,1,r.chrRemark,'migrate',r.redate,0,null,0,null,null from [continentalnew].[dbo].tblRejectedLineRemark r 
inner join dbo.purchase_detail p on r.perautokey = p.mapkey
inner join (select id,stockItemId from teastock where itemtype = 'L') s on p.id=s.stockItemId
union all
select p.id,s.id,0,r.ReducedQty,2,r.Remark,'migrate',r.reduceddate,case PaymentCondition when 0 then 0 when 1 then 1 when 2 then 1 end  PaymentCondition,null,0,null,null from [continentalnew].[dbo].tblReducedLine r 
inner join dbo.purchase_detail p on r.autokey = p.mapkey
inner join (select id,stockItemId from teastock where itemtype = 'L') s on p.id=s.stockItemId

go

delete from  [collectionnote] 
go
insert into [collectionnote] (id,CreateDate,CreateUser,Prefix)
select CollectionNoteNo,getdate(),'migrate','CN' from [x_saledetails]  group by CollectionNoteNo order by CollectionNoteNo
go
update  p set p.CollectionDate = s.CollectionDate,
p.StockReceived = s.StockReceived,
p.StoreLocation= s.StoreLocation,
p.CollectionNoteNo = s.CollectionNoteNo,
p.StockReceivedDate = s.StockReceivedDate
from [dbo].[purchase_detail] p inner join [x_saledetails] s 
on p.Id = s.perAutoKey

go
update blendcategorize set ClosedRate = Rate where closedrate is null
go

