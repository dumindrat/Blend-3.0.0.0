(function () {
    var urlStraightLinePurchase = 'api/StraightLinePurchase';
    var purchaseSheetUrl = 'api/PurchaseReport';

    app.controller('purchaseReportController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout, $window, httpService, globalService, reportService) {
        var renderAction = $route.current.$$route.originalPath;
        var systemDate = $filter('date')(globalService.getDate(), 'yyyy-MM-dd');
        $scope.accessLevels = globalService.getaccess(renderAction);
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.recordCount = globalService.getDataLoadingMessage();

        $scope.searchstr = "";
        var _timeout;

        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getSales($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        getSales('all');

        function getSales(seachby) {
            $scope.recordCount = globalService.getDataLoadingMessage();

            httpService.getdata(urlStraightLinePurchase + '/GetAllSales/' + seachby, function (results) {
                $scope.SaleData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();

            });

        };

        $scope.BrokerPurchases = function (p, size) {

            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/Purchase/BrokerPurchaseOptions.html',
                controller: 'brokerPurchaseSheetPrintController',
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


        $scope.PrintOption = function (p, size) {

            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/PrintOptions.html',
                controller: 'purchaseSheetPrintController',
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

        $scope.MoreOptions = function () {
            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/Purchase/AdvanceOptions.html',
                controller: 'purchaseSheetAdvancePrintController',
                windowClass: 'app-modal-sm-window'

            });

        };

        $scope.SaleSummaryPrintOption = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/PrintOptions.html',
                controller: 'salesSummaryPrintController',
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

        //This is for view in Html window
        $scope.printSalesSummary = function (saleId) {
            //globalService.openLoadingWindow("Sales Summary");
            //$http.get('api/PurchaseReport/GetSalesSummary/' + saleId, { responseType: 'arraybuffer' }).success(function (data) {
            //    var file = new Blob([data], { type: 'application/pdf' });
            //    var fileURL = URL.createObjectURL(file);
            //    window.open(fileURL);
            //    $scope.$emit('LOAD_DATA_EVENT', true);
            //}).error(function (data) {
            //    alert('Error in processing report.Contact Administrator.')
            //    $scope.$emit('LOAD_DATA_EVENT', true);
            //});
            var rep = { 'api': purchaseSheetUrl + '/GetSalesSummary/' + saleId, 'title': 'Sales Summary' };
            reportService.getviewreport(rep);
        };


        $scope.printCanisterSheet = function (saleId) {
            globalService.openLoadingWindow("Canister Sheet");
            $http.get('api/PurchaseReport/GetCanisterSheet/' + saleId, { responseType: 'arraybuffer' }).success(function (data) {
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

    app.controller('purchaseSheetPrintController', function ($http, $scope, $route, $modal, $modalInstance, $location, item, globalService, reportService) {
        //$scope.New = { 'SaleId': item.SaleId, 'IsWithValues': 0, 'IsExcelFormat': 0 };
        $scope.New = { 'SaleId': item.SaleId, 'IsWithValues': 0, 'IsDownload': 0, 'FileType': 'xls' };
        $scope.FileTypes = [{ FileType: "xls" }, { FileType: "doc" }];

        $scope.cancel = function () {

            $modalInstance.dismiss('Close');
        };

        $scope.printlines = function () {

            var doctype = 'application/pdf';

            if ($scope.New.IsDownload === true && $scope.New.FileType == 'xls') {
                doctype = 'application/vnd.ms-excel'
            }
            if ($scope.New.IsDownload === true && $scope.New.FileType == 'doc') {
                doctype = 'application/msword'
            }
            if ($scope.New.IsDownload) {

                $scope.dataLoading = true;

                $http.post(purchaseSheetUrl, $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
                    var file = new Blob([data], { type: doctype });
                    var fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                    $scope.dataLoading = false;


                }).error(function (data) {
                    alert('Error in processing report')
                    $scope.dataLoading = false;
                });
            }
            else {
                var rep = { 'api': purchaseSheetUrl, 'data': $scope.New };
                reportService.postviewreport(rep);
            }

        };
    });

    app.controller('purchaseSheetAdvancePrintController', function ($http, $scope, $route, $modalInstance, $filter, $location, globalService) {
        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.New = { 'IsExcelFormat': 0, 'IsWithValues': 0, 'FromSaleDate': initSaleDate, 'ToSaleDate': initSaleDate, 'Grade': '', 'Standard': '', 'SaleType': 'MS', 'BrokerCode': '' };

        $scope.cancel = function () {

            $modalInstance.dismiss('Close');
        };

        $scope.dateStateChange = function (state) {
            alert(state);
        }


        $scope.printlines = function () {
            globalService.openLoadingWindow("Purchase Report");
            $scope.New.FromSaleDate = globalService.convertDateToIST($scope.New.FromSaleDate);
            $scope.New.ToSaleDate = globalService.convertDateToIST($scope.New.ToSaleDate);

            var doctype = $scope.New.IsExcelFormat === true ? 'application/vnd.ms-excel' : 'application/pdf';
            $http.post('api/PurchaseReportAdvance', $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: doctype });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report.Contact Administrator.')
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };
    });

    app.controller('salesSummaryPrintController', function ($http, $scope, $route, $modal, $modalInstance, $location, item, globalService, reportService) {
        $scope.New = { 'SaleId': item, 'IsWithValues': 1, 'IsExcelFormat': 1 };
        $scope.cancel = function () {

            $modalInstance.dismiss('Close');
        };
        $scope.FileTypes = [{ FileType: "xls" }];
        var salesSummUrl = 'api/SalesSummaryReport';
        $scope.printlines = function () {
            if ($scope.New.IsDownload) {
                $scope.dataLoading = true;
                reportService.postdownload(salesSummUrl, $scope.New, function (response) {
                    $scope.dataLoading = false;

                });
            }
            else {
                $scope.New.IsExcelFormat = false;
                var rep = { 'api': salesSummUrl, 'data': $scope.New, 'title': 'Saless Summary' };
                reportService.postviewreport(rep);
            }

        };
    });

    app.controller('brokerPurchaseSheetPrintController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $location, $filter, item, globalService, httpService) {

        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);
        var loadingDelay = globalService.getDataLoadingDelay();
        //$scope.accessLevels = globalService.getaccess(renderAction);
        $scope.New = { 'FromSale': initSaleDate, 'ToSale': initSaleDate, 'BrokerId': 0, 'BrokerName': '' };
        $scope.filterOptions = { host: { BrokerId: 0 } }

        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        getBrokers();

        function getBrokers() {
            httpService.getdata('api/Broker', function (results) {
                $scope.BrokerData = results.data;
                $scope.filterOptions = $scope.BrokerData[0];
            });

        };

        $scope.print = function () {
            $scope.dataLoading = true;
            $scope.New.BrokerName = $scope.filterOptions.host.BrokerName;
            $scope.New.BrokerId = $scope.filterOptions.host.BrokerId;
            $scope.New.FromSaleDate = globalService.convertDateToIST($scope.New.FromSaleDate);
            $scope.New.ToSaleDate = globalService.convertDateToIST($scope.New.ToSaleDate);
            var doctype = $scope.New.IsExcelFormat === true ? 'application/vnd.ms-excel' : 'application/pdf';

            $http.post('api/BrokerPurchase', $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.dataLoading = false;


            }).error(function (data) {
                alert('Error in processing report')
                $scope.dataLoading = false;
            });

        }

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
    });
})();