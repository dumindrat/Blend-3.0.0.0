(function () {
    var urlPurchaseAndStock = 'api/LineSummary';
    var urlLineAdjust = 'api/AdjustLine';
    var urlDocument = 'api/Document';

    app.controller('straightLineSummaryController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout,
        httpService, globalService) {

        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.content = null;
        $scope.recordCount = globalService.getDataLoadingMessage();
        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.New = {
            'FromSaleDate': initSaleDate,
            'ToSaleDate': initSaleDate, 'GardenMark': '', 'Standard': '', 'BrokerCode': '', 'Invoice': '', 'LotNo': ''
        };

        $rootScope.$on('ADJUSTMENT_EVENT', function (event, data) {
            getStraightLines('all');
        });

        $rootScope.$on('LOAD_BY_FILTER', function (event, results) {
            $scope.StraightLineData = results.data;
            $scope.recordCount = globalService.getDataCountMessage(results.data.length);
            $('#datasearch').focus();
        });

        $scope.searchlines = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getStraightLines($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        getStraightLines('all');

        function getStraightLines(boxno) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var seachline = globalService.replaceSlash(boxno);

            httpService.getdata(urlLineAdjust + '/GetStraightLines/' + seachline, function (results) {
                $scope.StraightLineData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();
            });
        };

        $scope.openSummaryViewWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/LineSummary/SummaryView.html',
                controller: 'viewSummaryWindowController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

        $scope.loadLines = function () {
            globalService.openLoadingWindow("Loading..");
            $scope.New.FromSaleDate = globalService.convertDateToIST($scope.New.FromSaleDate);
            $scope.New.ToSaleDate = globalService.convertDateToIST($scope.New.ToSaleDate);

            httpService.getdata('api/LineSummary/GetFilterData?' + 'fromSaleDate=' + $scope.New.FromSaleDate
                    + '&toSaleDate=' + $scope.New.ToSaleDate + '&invoice=' + $scope.New.Invoice
                    + '&lotNo=' + $scope.New.LotNo + '&gardenMark=' + $scope.New.GardenMark
                    + '&brokerCode=' + $scope.New.BrokerCode + '&standard=' + $scope.New.Standard, function (results) {

                        $rootScope.$emit('LOAD_BY_FILTER', results);
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    }).error(function (data) {
                        alert('Error in processing report.Contact Administrator.')
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    });
        };
    });

    app.controller('viewSummaryWindowController', function ($scope, $rootScope, $modalInstance, item, httpService, globalService) {
        $scope.BlendInfo = [];
        $scope.SelectedBox = item;
        $scope.LineSummary = {};
        $scope.Allocations = [];
        $scope.linedataLoading = false;
        $scope.summarydataLoading = false;
        $scope.allocationdataLoading = false;
        $scope.linedataLoading = false;
        $scope.ver = '1w3re4';

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        getPurchaseAndStocks(item.BoxNo);

        function getPurchaseAndStocks(boxno) {
            var search = globalService.replaceSlash(boxno);
            $scope.linedataLoading = true;

            httpService.getdata(urlPurchaseAndStock + '/GetLineDetails/' + search, function (results) {
                $scope.StraightLineData = results.data;
                $scope.linedataLoading = false;

            });
        };

        getBlendAllocationByBoxNo(item.BoxNo);

        function getBlendAllocationByBoxNo(boxno) {
            var search = globalService.replaceSlash(boxno);
            $scope.allocationdataLoading = true;

            httpService.getdata(urlPurchaseAndStock + '/GetBlendAllocationsByBoxNo/' + search, function (results) {
                $scope.BlendAllocationData = results.data;
                $scope.allocationdataLoading = false;

            });
        };

        getAdjustedLine(item.BoxNo);

        function getAdjustedLine(Id) {
            var search = globalService.replaceSlash(Id);
            $scope.allocationdataLoading = true;
            httpService.getdata(urlPurchaseAndStock + '/GetAdjustedLine/' + search, function (results) {
                $scope.AdjustedLineData = results.data;
            });
        };

        getLineTransHistory($scope.SelectedBox.StockItemId);

        function getLineTransHistory(Id) {
            $scope.allocationdataLoading = true;
            httpService.getdata(urlPurchaseAndStock + '/GetTransactionHistory/' + Id, function (results) {
                $scope.LineHitoryData = results.data;
            });
        };



    });

    
})();