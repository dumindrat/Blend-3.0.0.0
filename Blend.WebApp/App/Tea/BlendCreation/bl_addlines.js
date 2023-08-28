(function () {
    var urlBlenCreation = 'api/Blend';

    app.controller('addLinesController', function ($scope, $rootScope, $modal, $modalInstance, $http, $timeout, item, globalService,
        httpService) {
        $scope.BlendInfo = [];
        $scope.SelectedBlend = item;
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var stocktype_strline = 1;
        var stocktype_blends = 2;
        $scope.StockType = stocktype_strline;
        $scope.stocktypes = [{ stocktype: 1, desc: 'StraightLine' }, { stocktype: 2, desc: 'BlendBalance' }];
        $scope.SeachType = 1;
        $scope.seachtypes = [{ seachtype: 1, desc: 'BoxNo' }, { stocktype: 2, desc: 'LotNo' }, { stocktype: 2, desc: 'InvoiceNo' }];
        $scope.focusQuickSearch = true;
        $scope.focusGeneralSearch = false;
        $scope.disableQuickSearch = false;
        $scope.disableStockType = false;
        $scope.boxNoData = '';
        $scope.BoxNumbers = [];
        $scope.showAddlines = false;
        $scope.focusAddqty = false;
        $scope.loadingdata = false;
        $scope.message = '';
        $scope.messageclass = '';

        $rootScope.$on('ADDLINE_MSG_EVENT', function (event, data) {
            if (data.status) {
                $scope.message =  data.message;
                $scope.messageclass = "alert alert-success";
            }
            });
        $scope.close = function () {
            $scope.closeMsg = 'I was closed at: ' + new Date();
            //$scope.focusQuickSearch = false;
        };

        //$rootScope.$on('CLOSE_POPUP', function (event, data) {
        //    $scope.cancel();
        //});

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.addQuickSearchItem = function (data) {
            $scope.openAddStraightLinesWindow(data);
            //$scope.showAddlines = true;
        }

        $scope.viewStockGrid = function () {
            if ($scope.StockType === stocktype_strline) {
                $scope.viewstkgrid = "/straightlinestock.html";
                $scope.disableQuickSearch = false;
                $scope.focusGeneralSearch = true;
                $scope.disableStockType = false;

            }
            else {
                $scope.viewstkgrid = "/blendstock.html";
                $scope.focusGeneralSearch = true
                $scope.disableQuickSearch = true;
                $scope.disableStockType = true;

            }
        };


        $scope.addQuickSearchItems = function (item) {
            var ret = $scope.BoxNumbers.indexOf(item);
            if (ret == -1) {
                $scope.BoxNumbers.push(item);
            }
            //$scope.item = {};
        }

        $scope.stockgridcolumns = [
                { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
                { text: "StockItemId", predicate: "StockItemId", sortable: false, hide: true },
                { text: "BoxNo", predicate: "BoxNo", sortable: true, hide: false },
                { text: "SaleDate", predicate: "SaleDate", sortable: true, hide: false },
                { text: "LotNo", predicate: "LotNo", sortable: true, hide: false },
                { text: "Inv", predicate: "Invoice", sortable: true, hide: false },
                { text: "Grade", predicate: "grade", sortable: true, hide: false },
                { text: "Standard", predicate: "Standard", sortable: true, hide: false },
                { text: "GardenMark", predicate: "GardenMark", sortable: true, hide: false },
                { text: "Pks", predicate: "packs", sortable: true, hide: false },
                { text: "Net", predicate: "NetWeight", sortable: true, hide: false },
                { text: "Qty", predicate: "Qty", sortable: true, hide: false },
                { text: "(AD)Qty", predicate: "ADQty", sortable: true, hide: false },
                { text: "Rate", predicate: "rate", sortable: true, hide: false },
                { text: "Value", predicate: "value", sortable: true, hide: false },
                { text: "RowVersion", predicate: "RowVersion", sortable: true, hide: true },
                { text: "Option", predicate: "Option", sortable: true, hide: true }


        ];

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
        getStraghtLineStock('all');

        function getStraghtLineStock(searchstring) {
            var boxNo = globalService.replaceSlash(searchstring);//.replacegetStraightlinestockItem("/", "~");
            $scope.StraightLIneStockData = [];
            $scope.loadingdata = true;
            httpService.getdata(urlBlenCreation + '/GetStraghtLineStock/' + boxNo, function (results) {
                $scope.StraightLIneStockData = results.data;
                $scope.viewstkgrid = "/straightlinestock.html";
            $scope.loadingdata = false;
             $('#searchby').focus();
            });

        }

        function getBlendStock(searchstring) {
            $scope.loadingdata = true;
            var blendNo = globalService.replaceSlash(searchstring);
            httpService.getdata(urlBlenCreation + '/GetBlendStock/' + blendNo, function (results) {
                $scope.BlendStockData = results.data;
                $scope.loadingdata = false;
                $('#searchby').focus();

            });
        }

        $scope.searchlinestock = function (searchstring) {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                if ($scope.StockType === stocktype_strline)
                    getStraghtLineStock($scope.searchstr === '' ? "all" : $scope.searchstr);
                if ($scope.StockType === stocktype_blends)
                    getBlendStock($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
                
            }, loadingDelay);
        };


        GetQuickSearchItems();

        function GetQuickSearchItems() {
            httpService.getdata('api/Blend/GetQuickSearchItems/sl', function (results) {
                angular.forEach(results.data, function (value, key) {
                    $scope.addQuickSearchItems(value['ItemName']);
                })
            });
        };

        $scope.openAddlineWindow = function () {
            $scope.showAddlines = true;
            $scope.focusAddqty = true;

        }

        //Added lines
        $scope.openAddStraightLinesWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/AddSL.html',
                controller: 'blendAddSLController',
                windowClass: 'app-modal-sm-window',
                keyboard: false,
                resolve: {
                    item: function () {
                        return p;
                    },
                    blend: function () {
                        return $scope.SelectedBlend;
                    }
                }
            });
        };

        //Added Blends
        $scope.openAddBlendBalanceWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/AddBL.html',
                controller: 'blendAddBLController',
                windowClass: 'app-modal-sm-window',
                keyboard: false,
                resolve: {
                    item: function () {
                        return p;
                    },
                    blend: function () {
                        return $scope.SelectedBlend;
                    }
                }
            });

        };

        //Added Blends
        $scope.openSummaryWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/AddBL.html',
                controller: 'blendAddBLController',
                windowClass: 'app-modal-sm-window',
                keyboard: false,
                resolve: {
                    item: function () {
                        return p;
                    },
                    blend: function () {
                        return $scope.SelectedBlend;
                    }
                }
            });

        };
        
        $scope.blendselectdisable = function (stockItemId) {
            if ($scope.SelectedBlend.BlendId === stockItemId) {
                return true;
            }
            return false;
        };

    });
})();