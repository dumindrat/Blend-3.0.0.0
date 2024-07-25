(function () {
    var urlStraightLinePurchase = 'api/BlendReport';
    var urlBlenCreation = 'api/Blend';

    app.controller('blendReportController', function ($http, $scope, $rootScope, $route, $filter, $timeout, $window, $modal, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        var systemDate = $filter('date')(globalService.getDate(), 'yyyy-MM-dd');
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.recordCount = globalService.getDataLoadingMessage();
        $scope.ContractNumbers = [];
        $scope.searchstr = "";
        $scope.contractData = '';
        var _timeout;

        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getBlendList($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        getBlendList('all');

        function getBlendList(searchstring) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var blendNo = globalService.replaceSlash(searchstring);

            httpService.getdata(urlBlenCreation + '/GetBlends/' + blendNo, function (results) {
                $scope.BlendData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();

            });
        };

        //getContracts();

        function getContracts() {
            //$scope.recordCount = globalService.getDataLoadingMessage();
            //var contractno = globalService.replaceSlash(searchstr);
            httpService.getdata('api/Contract/GetAllContracts/all', function (results) {
                angular.forEach(results.data, function (value, key) {
                    $scope.addContracts(value['ContractName']);
                })
            });
        };

        $scope.addContracts = function (item) {
            var ret = $scope.ContractNumbers.indexOf(item);
            if (ret == -1) {
                $scope.ContractNumbers.push(item);
            }
            //$scope.item = {};
        }

        //getAvailabaleStandards('all');

        function getAvailabaleStandards(searchstr) {
            $scope.dataloading = true;

            httpService.getdata(conDeturl + '/' + 'GetAvailableStandards?id=' + $scope.selectedContract.ContractId + '&standard=' + searchstr, function (results) {
                $scope.teastandards = results.data;
                $scope.dataloading = false;

            });

        };

        $scope.gridOptions = {
            data: 'BlendData',
            showGroupPanel: false,
            rowHeight: globalService.GridRowHeight(),
            columnDefs: [{ field: 'BlendId', displayName: 'Id', visible: false, width: '5%' },
                { field: 'BlendNo', displayName: 'BlendNo', width: '25%' },
                { field: 'ContractId', displayName: 'ContractId', visible: false, width: '20%' },
                { field: 'ContractNo', displayName: 'Contract', visible: true, width: '15%' },
                { field: 'StandardId', displayName: 'StandardId', visible: false, width: '15%' },
                { field: 'StandardName', displayName: 'Standard', visible: true, width: '20%' },
                { field: 'ExportQty', displayName: 'ExportQty', visible: true, width: '10%' },
                { field: 'CompleteDate', displayName: 'Complete Date', visible: true, width: '12%' },
                { field: 'CloseStatus', displayName: 'Status', visible: true, width: '8%' },
                { field: 'IsClosed', displayName: 'IsClosed', visible: false, width: '10%' },
                {
                    displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.V" ng-click="PrintOption(row.entity)" name="edit"  value="Blend Sheet">'
                    //'<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.V" ng-click="printStoresSheet(row.entity.BlendId)" name="blrnd"  value="SBS">' +
                    //'<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.V" ng-click="formPd(row.entity.BlendId)" name="print"  value="FPD">'
                    , width: '10%'
                }
            ]
        };

        $scope.PrintOption = function (p, size) {

            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/Blend/PrintOptions.html',
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

        $scope.printBlendSheet = function (blendId) {
            globalService.openLoadingWindow("Blend Sheet");
            var obj = { 'BlendId': blendId, 'AfterClosed': 0, 'IsWithValues': 1, 'IsDownload': 0, 'FileType': 'xls' };
            $http.post('api/BlendReports', obj, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report')
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };

        //$scope.printStoresSheet = function (blendId) {
        //    globalService.openLoadingWindow("Blend Sheet");
        //    $http.get('api/BlendReports/GetStoresSheet/' + blendId, { responseType: 'arraybuffer' }).success(function (data) {
        //        var file = new Blob([data], { type: 'application/pdf' });
        //        var fileURL = URL.createObjectURL(file);
        //        window.open(fileURL);
        //        $scope.$emit('LOAD_DATA_EVENT', true);
        //    }).error(function (data) {
        //        alert('Error in processing report.Contact Administrator.')
        //        $scope.$emit('LOAD_DATA_EVENT', true);

        //    });
        //};

        //$scope.printPurchaseSheet = function (saleId) {
        //    //alert(blendId);
        //    $window.open("api/PurchaseReport/GetPurchaseSheet/" + saleId);
        //};

        //$scope.printCanisterSheet = function (saleId) {
        //    //alert(blendId);
        //    $window.open("api/PurchaseReport/GetCanisterSheet/" + saleId);
        //};

        $scope.CanisterSheet = function (blendId) {
            globalService.openLoadingWindow("Canister Sheet");
            $http.get('api/BlendReports/GetBlendCanister/' + blendId, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);
            }).error(function (data) {
                alert('Error in processing report.Contact Administrator.')
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };

    });

    app.controller('blendSheetPrintController', function ($http, $scope, $route, $modalInstance, $location, item, globalService) {
        $scope.itemname = item.BlendNo
        $scope.New = { 'BlendId': item.BlendId, 'AfterClosed': 0, 'IsWithValues': 0, 'IsDownload': 0, 'FileType': 'xls' };
        $scope.cancel = function () {

            $modalInstance.dismiss('Close');
        };

        $scope.FileTypes = [{ FileType: "xls" }, { FileType: "doc" }];

        $scope.printlines = function () {
            globalService.openLoadingWindow("Blend Sheet");
            var doctype = 'application/pdf';
            if ($scope.New.IsDownload === true && $scope.New.FileType == 'xls') {
                doctype = 'application/vnd.ms-excel'
            }
            if ($scope.New.IsDownload === true && $scope.New.FileType == 'doc') {
                doctype = 'application/msword'
            }
            $http.post('api/BlendReports', $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: doctype });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report')
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };
    });

})();