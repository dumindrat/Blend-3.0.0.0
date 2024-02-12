(function () {
    var urlBlenCreation = 'api/Blend';
    var urlBlenAddRemove = 'api/BlendAddRemove';

    app.controller('blendEditController', function ($scope, $rootScope, $modalInstance, $modal, $route, $filter, $timeout, item, httpService, globalService) {
        var systemDate = $filter('date')(globalService.getDate(), 'yyyy-MM-dd');
        var stocktype_strline = 1;
        var stocktype_blends = 2;
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();

        $scope.showLoader = false;
        $scope.SelectedBlend = item;

        $scope.closestatusbadgeclass = 'btn btn-warning';
        if ($scope.SelectedBlend.IsClosed === true)
            $scope.closestatusbadgeclass = 'btn btn-warning';

        $scope.setvisiblecreatescreen = false;
        $scope.setvisibleaddedscreen = true;
        $scope.StockType = stocktype_strline;
        $scope.stocktypes = [{ stocktype: 1, desc: 'StraightLine' }, { stocktype: 2, desc: 'BlendBalance' }];
        $scope.SeachType = 1;
        $scope.seachtypes = [{ seachtype: 1, desc: 'BoxNo' }, { stocktype: 2, desc: 'LotNo' }, { stocktype: 2, desc: 'InvoiceNo' }];


        $scope.setnew = function () {
            $scope.BL = { 'BlendItemStockId': 0, 'BlendItemStockItemId': 0, 'BlendItemQty': 0, 'BlendItemStockQty': 0, 'BlendItemRate': 0, 'BlendItemValue': 0, 'BlendItemStockName': '' };
            $scope.showLoader = false;
            $scope.searchstr = '';
            $scope.boxNoData = '';
            $scope.PackD = 0;
            $scope.PackDR = 0;
        };

        //$scope.BoxNumbers = [];

        //$scope.addQuickSearchItems = function (item) {
        //    var ret = $scope.BoxNumbers.indexOf(item);
        //    if (ret == -1) {
        //        $scope.BoxNumbers.push(item);
        //    }
        //    //$scope.item = {};
        //}

        //GetQuickSearchItems();

        //function GetQuickSearchItems() {
        //    httpService.getdata(urlBlenCreation + '/GetQuickSearchItems/sl', function (results) {
        //        angular.forEach(results.data, function (value, key) {
        //            $scope.addQuickSearchItems(value['ItemName']);
        //        })
        //    });
        //};

        //$scope.addQuickSearchItem = function (data) {
        //    $scope.openAddStraightLinesWindow(data);
        //}


        SetNew();

        function SetNew() {
            $scope.setnew();
        };


        $rootScope.$on('ADDSL_BLEND_EVENT', function (event, data) {
            $scope.searchstr = '';
            //angular.element('#' + 'searchby').focus()
            //getStraghtLineStock('all');
            GetBlendDetailById($scope.SelectedBlend.BlendId);

        });

        $rootScope.$on('ADDSL_POPUP_WINDOW_EVENT', function (event, data) {
            //angular.element('#' + 'searchby').removeClass("loading");
        });

        $rootScope.$on('ADDBL_BLEND_EVENT', function (event, data) {
            $scope.searchstr = '';
            //angular.element('#' + 'searchby').focus()
            //getBlendStock('all');
            GetBlendDetailById($scope.SelectedBlend.BlendId);
        });

        $rootScope.$on('ADDBL_POPUP_WINDOW_EVENT', function (event, data) {
            angular.element('#' + 'searchby').removeClass("loading");
        });



        GetBlendDetailById($scope.SelectedBlend.BlendId);


        function GetBlendDetailById(blendid) {
            globalService.openLoadingWindow('Loading..');
            httpService.getdata(urlBlenAddRemove + '/' + blendid, function (results) {
                $scope.BlendDisplayItems = results.data.BlendItem;
                $scope.BlendDisplayQty = results.data.DisplayQty;
                $scope.BlendDisplayRate = results.data.DisplayRate;
                $scope.setvisiblecreatescreen = false;
                $scope.setvisibleaddedscreen = true;
                $scope.$emit('LOAD_DATA_EVENT', true);
                $('#searchby').focus();//Setfocus search box when gone through adding lines to blend,to continue adding lines
            });
        };

        $scope.openViewAddLines = function (size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/AddLines.html',
                controller: 'addLinesController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return $scope.SelectedBlend;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };


        //$scope.openCreateView = function () {
        //    $scope.setvisiblecreatescreen = true;
        //    $scope.setvisibleaddedscreen = false;
        //    $scope.setvisiblestockwindow = true;
        //    //angular.element('#' + 'searchby').focus()
        //    getStraghtLineStock("all");
        //};

        //$scope.closeCreteView = function () {
        //    //GetBlendDetailById($scope.SelectedBlend.BlendId);
        //    $scope.setvisiblecreatescreen = false;
        //    $scope.setvisibleaddedscreen = true;
        //    $scope.setvisiblestockwindow = false;
        //};

        $scope.New = { 'BoxNo': '', 'Pack': 0, 'Net': 0, 'Qty': 0, 'ReducedQty': 0, 'PackR': 0, 'NetR': 0, 'QtyR': 0, 'ReducedQtyR': 0, 'BlendQty': 0 };

        //$scope.searchlinestock = function (searchstring) {
        //    //if ($scope.searchstr.length === 8) {
        //    //    $scope.openAddStraightLinesWindow($scope.searchstr, 0);
        //    //}
        //    //else {

        //    //}

        //    if (_timeout) {
        //        $timeout.cancel(_timeout);
        //    }
        //    _timeout = $timeout(function () {
        //        if ($scope.StockType === stocktype_strline)
        //            getStraghtLineStock($scope.searchstr === '' ? "all" : $scope.searchstr);
        //        if ($scope.StockType === stocktype_blends)
        //            getBlendStock($scope.searchstr === '' ? "all" : $scope.searchstr);
        //        _timeout = null;
        //    }, loadingDelay);
        //};


        //$scope.switchStockSearchType = function () {
        //    angular.element('#' + 'txtSearchStock').focus();
        //};

        $scope.CheckCloseStatus = function () {
            if ($scope.SelectedBlend.IsClosed === false) {
                return false;
            }
            return true;
        }

        $scope.CheckEnabledTeaBoard = function () {
            return $scope.SelectedBlend.IsTeaBoardEnabled;
        }

        $scope.GetTrialLable = function () {
            if ($scope.SelectedBlend.IsTeaBoardEnabled ) {
                return "-Production";
            }
            return "-Trial Blend";
        }

        $scope.blendselectdisable = function (stockItemId) {
            if ($scope.SelectedBlend.BlendId === stockItemId) {
                return true;
            }
            return false;
        };

        //function getStraghtLineStock(searchstring) {
        //    var boxNo = globalService.replaceSlash(searchstring);//.replacegetStraightlinestockItem("/", "~");
        //    $scope.StraightLIneStockData = [];
        //    globalService.openLoadingWindow("Loading SL Stock..");

        //    httpService.getdata(urlBlenCreation + '/GetStraghtLineStock/' + boxNo, function (results) {
        //        $scope.StraightLIneStockData = results.data;
        //        $scope.$emit('LOAD_DATA_EVENT', true);
        //        $scope.viewstkgrid = "/straightlinestock.html";
        //    });

        //}

        //function getBlendStock(searchstring) {
        //    globalService.openLoadingWindow("Loading Blend Stock..");
        //    var blendNo = globalService.replaceSlash(searchstring);
        //    httpService.getdata(urlBlenCreation + '/GetBlendStock/' + blendNo, function (results) {
        //        $scope.BlendStockData = results.data;
        //        $scope.$emit('LOAD_DATA_EVENT', true);

        //    });
        //}

        //$scope.viewStockGrid = function () {
        //    if ($scope.StockType === stocktype_strline) {
        //        $scope.viewstkgrid = "/straightlinestock.html";
        //    }
        //    else {
        //        $scope.viewstkgrid = "/blendstock.html";
        //    }
        //};

        $scope.focusFunc = function (keyEvent, elementId) {
            if (keyEvent.which === 13)
                angular.element('#' + elementId).focus()
        }

        //$scope.stockgridcolumns = [
        //          { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
        //          { text: "StockItemId", predicate: "StockItemId", sortable: false, hide: true },
        //          { text: "BoxNo", predicate: "BoxNo", sortable: true, hide: false },
        //          { text: "SaleDate", predicate: "SaleDate", sortable: true, hide: false },
        //          { text: "LotNo", predicate: "LotNo", sortable: true, hide: false },
        //          { text: "Invoice", predicate: "Invoice", sortable: true, hide: false },
        //          { text: "Grade", predicate: "grade", sortable: true, hide: false },
        //          { text: "Standard", predicate: "Standard", sortable: true, hide: false },
        //          { text: "GardenMark", predicate: "GardenMark", sortable: true, hide: false },
        //          { text: "Packs", predicate: "packs", sortable: true, hide: false },
        //          { text: "NetWeight", predicate: "NetWeight", sortable: true, hide: false },
        //          { text: "Qty", predicate: "Qty", sortable: true, hide: false },
        //          { text: "(AD)Qty", predicate: "ADQty", sortable: true, hide: false },
        //          { text: "Rate", predicate: "rate", sortable: true, hide: false },
        //          { text: "Value", predicate: "value", sortable: true, hide: false },
        //          { text: "RowVersion", predicate: "RowVersion", sortable: true, hide: true },
        //          { text: "Option", predicate: "Option", sortable: true, hide: true }


        //];

        //$scope.stockGridOptions = {
        //    data: 'StraightLIneStockData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'Id', displayName: 'Id', visible: false, width: '1%' },
        //        { field: 'StockItemId', displayName: 'StockItemId', visible: false, width: '10%' },
        //        { field: 'BoxNo', displayName: 'BoxNo', visible: true, width: '7%' },
        //        { field: 'SaleCode', displayName: 'Code', visible: false, width: '5%' },
        //        { field: 'SaleDate', displayName: 'Sale Date', visible: true, width: '10%' },
        //        { field: 'Invoice', displayName: 'InvNo', visible: true, width: '5%' },
        //        { field: 'LotNo', displayName: 'LotNo', visible: true, width: '5%' },
        //        { field: 'Grade', displayName: 'Grade', visible: true, width: '7%' },
        //        { field: 'Standard', displayName: 'Standard', visible: true, width: '7%' },
        //        { field: 'GardenMark', displayName: 'GardenMark', visible: true, width: '20%' },
        //        { field: 'Packs', displayName: 'Packs', visible: true, width: '5%', cellFilter: 'number: 2' },
        //        { field: 'NetWeight', displayName: 'Net', visible: true, width: '5%', cellFilter: 'number: 2' },
        //        { field: 'Qty', displayName: 'Qty', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'Rate', displayName: 'Rate', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'Value', displayName: 'Value', visible: true, width: '10%', cellFilter: 'number: 2' },
        //        { field: 'RowVersion', displayName: 'RowVersion', visible: false, width: '5%' },

        //        {
        //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.C" ng-click="openAddStraightLinesWindow(row.entity.BoxNo)" name="edit"  value="Add">'

        //    , width: '15%'
        //        }
        //    ]
        //};

        $scope.blendstockgridcolumns = [
                 { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
                 { text: "StockItemId", predicate: "StockItemId", sortable: false, hide: true },
                 { text: "ItemName", predicate: "ItemName", sortable: true, hide: false },
                 { text: "Status", predicate: "Status", sortable: true, hide: false },
                 { text: "Rate", predicate: "Rate (Rs)", sortable: true, hide: false },
                 { text: "Qty", predicate: "Qty (Kg)", sortable: true, hide: false },
                 { text: "Value", predicate: "Value(Rs)", sortable: true, hide: false },
                 { text: "IsClosed", predicate: "Status", sortable: true, hide: true },
                 { text: "Option", predicate: "Option", sortable: true, hide: true }


        ];

        //$scope.blendStockGridOptions = {
        //    data: 'BlendStockData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'Id', displayName: 'Id', visible: false, width: '1%' },
        //        { field: 'StockItemId', displayName: 'Id', visible: false, width: '10%' },
        //        { field: 'ItemName', displayName: 'StockName', visible: true, width: '33%' },
        //        { field: 'Status', displayName: 'Status', visible: true, width: '10%' },
        //        { field: 'Rate', displayName: 'Rate(Rs)', visible: true, width: '15%', cellFilter: 'number: 2' },
        //        { field: 'Qty', displayName: 'Qty(Kg)', visible: true, width: '15%', cellFilter: 'number: 2' },
        //        { field: 'Value', displayName: 'Value(Rs)', visible: true, width: '20%', cellFilter: 'number: 2' },
        //        { field: 'IsClosed', displayName: 'Status', visible: false, width: '10%' },

        //        {
        //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="blendselectdisable(row.entity.StockItemId)" ng-click="openAddBlendBalanceWindow(row.entity)" name="edit"  value="Add">'
        //            , width: '7%'
        //        }
        //    ]
        //};

        $scope.blenddetailcolumns = [
                 { text: "BlendDetalId", predicate: "BlendDetalId", sortable: true, dataType: "number", hide: true },
                  { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
                  { text: "StockItemId", predicate: "StockItemId", sortable: false, hide: true },
                  { text: "BoxNo", predicate: "BoxNo", sortable: true, hide: false },
                  { text: "SaleDate", predicate: "SaleDate", sortable: true, hide: false },
                  { text: "LotNo", predicate: "LotNo", sortable: true, hide: false },
                  { text: "Invoice", predicate: "Invoice", sortable: true, hide: false },
                  { text: "Grade", predicate: "grade", sortable: true, hide: false },
                  { text: "Standard", predicate: "Standard", sortable: true, hide: false },
                  { text: "ST-SC", predicate: "SaleType", sortable: true, hide: false },
                  //{ text: "SaleCode", predicate: "SaleCode", sortable: true, hide: false },
                  { text: "GardenMark", predicate: "GardenMark", sortable: true, hide: false },
                  { text: "Packs", predicate: "packs", sortable: true, hide: false },
                  { text: "NetWeight", predicate: "NetWeight", sortable: true, hide: false },
                  { text: "Qty", predicate: "Qty", sortable: true, hide: false },
                  { text: "Rate", predicate: "rate", sortable: true, hide: false },
                  { text: "Value", predicate: "value", sortable: true, hide: false },
                  { text: "RowVersion", predicate: "RowVersion", sortable: true, hide: true },
                  { text: "ItemType", predicate: "ItemType", sortable: true, hide: true },
                  { text: "Option", predicate: "Option", sortable: true, hide: true }


        ];



        $scope.getBox = function () {
            for (var key in $scope.StraightLIneStockData) {
                var test = {};
                var obj = $scope.StraightLIneStockData[key];
                if (obj.StandardId === standardId) {
                    $scope.New.ExportQty = obj.ExportQty;
                    angular.element('#blendno').focus()
                    break;
                }
            }
        };

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.BlendItem = { 'BlendDetailId': 0, 'BlendId': 0, 'StockId': 0, 'StockItemId': 0, 'RowVersion': '' };

        $scope.RemoveItem = function (entity) {
            $scope.BlendItem.BlendDetailId = entity.BlendDetailId;
            $scope.BlendItem.BlendId = $scope.SelectedBlend.BlendId;
            $scope.BlendItem.StockId = entity.Id;
            $scope.BlendItem.StockItemId = entity.StockItemId;
            $scope.BlendItem.RowVersion = entity.RowVersion;
            $scope.BlendItem.BlendQty = entity.Qty;
            $scope.BlendItem.Packs = entity.Packs;
            $scope.BlendItem.ItemName = entity.BoxNo;


            var retVal = confirm("Are you sure want to remove item :" + entity.BoxNo + '?');
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');
                httpService.updatedata(urlBlenAddRemove, $scope.BlendItem, function (response) {
                    if (response.status === 200) {
                        alert("Success");
                        $scope.disablestatus = false;
                        GetBlendDetailById($scope.SelectedBlend.BlendId);
                        //getStraghtLineStock("all");
                    } else {
                        alert(response.message);
                        $scope.disablestatus = false;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);
                });
            }
        };

        ////Added lines
        //$scope.openAddStraightLinesWindow = function (p, size) {
        //    var modalInstance = $modal.open({
        //        templateUrl: '/App/Tea/BlendCreation/AddSL.html',
        //        controller: 'blendAddSLController',
        //        windowClass: 'app-modal-sm-window',
        //        resolve: {
        //            item: function () {
        //                return p;
        //            },
        //            blend: function () {
        //                return $scope.SelectedBlend;
        //            }
        //        }
        //    });
        //};

        ////Added Blends
        //$scope.openAddBlendBalanceWindow = function (p, size) {
        //    var modalInstance = $modal.open({
        //        templateUrl: '/App/Tea/BlendCreation/AddBL.html',
        //        controller: 'blendAddBLController',
        //        windowClass: 'app-modal-sm-window',
        //        resolve: {
        //            item: function () {
        //                return p;
        //            },
        //            blend: function () {
        //                return $scope.SelectedBlend;
        //            }
        //        }
        //    });
        //};

        $scope.PrintOption = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/PrintOptions.html',
                controller: 'blendSheetPrintController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
                p.contractname = selectedObject.contractname;
            });
        };

    });

})();