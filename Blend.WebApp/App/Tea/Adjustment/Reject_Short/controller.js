(function () {
    var urlLineAdjust = 'api/AdjustLine';

    app.controller('adjustmentController', function ($http, $scope, $sce, $rootScope, $modal, $route, $timeout, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.content = null;
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('ADJUSTMENT_EVENT', function (event, data) {
            getStraightLines('all');
        });



        getStraightLines('all');

        function getStraightLines(boxno) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var boxNoData = globalService.replaceSlash(boxno);

            httpService.getdata(urlLineAdjust + '/GetStraightLines/' + boxNoData, function (results) {
                $scope.StraightLineData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();
            });
        };



        //$scope.gridOptions = {
        //    data: 'StraightLineData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'Id', displayName: 'Id', visible: false, width: '5%' },
        //        { field: 'StockItemId', displayName: 'StockItemId', visible: false, width: '10%'},
        //        { field: 'BoxNo', displayName: 'BoxNo', visible: true, width: '10%' },
        //        { field: 'SaleCode', displayName: 'Code', visible: false, width: '4%' },
        //        { field: 'SaleDate', displayName: 'Date', visible: true, width: '10%' },
        //        { field: 'Invoice', displayName: 'Inv', visible: true, width: '5%' },
        //        { field: 'LotNo', displayName: 'Lot', visible: true, width: '5%' },
        //        { field: 'SaleType', displayName: 'Type', visible: true, width: '5%' },
        //        { field: 'Grade', displayName: 'Grade', visible: true, width: '7%' },
        //        { field: 'Standard', displayName: 'Standard', visible: false, width: '7%' },
        //        { field: 'GardenMark', displayName: 'GardenMark', visible: true, width: '17%' },
        //        { field: 'NetWeight', displayName: 'Net', visible: true, width: '5%', cellFilter: 'number: 2' },
        //        { field: 'Packs', displayName: 'Packs', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'Qty', displayName: 'Stock', visible: true, width: '10%', cellFilter: 'number: 2' },
        //        { field: 'ReducedQty', displayName: 'RQty', visible: false, width: '5%', cellFilter: 'number: 2' },
        //        { field: 'Rate', displayName: 'Rate', visible: false, width: '7%' },
        //        { field: 'Value', displayName: 'Value', visible: false, width: '7%' },
        //        { field: 'RowVersion', displayName: 'RowVersion', visible: false, width: '7%' },

        //        {
        //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.M" ng-click="openRejectWindow(row.entity)" name="edit"  value="Reject">' +
        //                '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.M" ng-click="openShortWindow(row.entity)" name="edit"  value="Short">' +
        //                '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.M" ng-click="openAdjustmentViewWindow(row.entity)" name="blrnd"  value="Cancel">'

        //            , width: '40%'
        //        }
        //    ]
        //};

        $scope.print = function () {
            httpService.getdata(urlLineAdjust + '/GetReport/' + 1, function (results) {
                var data = results.data;
                var file = new Blob([results.data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                $scope.content = $sce.trustAsResourceUrl(fileURL);
                //window.open(results, '');
            });
        }
        $scope.searchlines = function searchlines() {
            if (_timeout) { // if there is already a timeout in process cancel it
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getStraightLines($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        //Added lines
        $scope.openShortWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Adjustment/Reject_Short/ShortLine.html',
                controller: 'shortController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

        //Reject lines
        $scope.openRejectWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Adjustment/Reject_Short/RejectLine.html',
                controller: 'rejectController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

        $scope.openAdjustmentViewWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Adjustment/Reject_Short/Adjustments.html',
                controller: 'adjustmentOpsController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

    });


})();