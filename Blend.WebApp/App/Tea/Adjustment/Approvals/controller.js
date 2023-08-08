(function () {
    var urlLineAdjustCancel = 'api/CancellationApprove';

    app.controller('cancellationApproveController', function ($http, $scope, $sce, $rootScope, $modal, $route, $timeout, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('ADJUSTMENT_APPROVE_EVENT', function (event, data) {
            getStraightLines('all');
        });



        getStraightLines('all');

        function getStraightLines(boxno) {
            $scope.recordCount = globalService.getDataLoadingMessage();

            httpService.getdata(urlLineAdjustCancel + '/GetAdjustedLine/' + boxno, function (results) {
                $scope.StraightLineData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });
        };



        //$scope.gridOptions = {
        //    data: 'StraightLineData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'Id', displayName: 'Id', visible: false, width: '5%' },
        //        { field: 'AdjustmentId', displayName: 'AdjustmentId', visible: false, width: '7%' },
        //        { field: 'StockId', displayName: 'StockId', visible: false, width: '7%' },
        //        { field: 'BoxNo', displayName: 'BoxNo', visible: true, width: '7%' },
        //        { field: 'SaleDate', displayName: 'SaleDate', visible: true, width: '10%' },
        //        { field: 'BrokerCode', displayName: 'Bk', visible: true, width: '5%' },
        //        { field: 'Invoice', displayName: 'Inv', visible: false, width: '5%' },
        //        { field: 'LotNo', displayName: 'Lot', visible: true, width: '5%' },
        //        { field: 'Grade', displayName: 'Grade', visible: true, width: '10%' },
        //        { field: 'GardenMark', displayName: 'GardenMark', visible: true, width: '20%' },
        //        { field: 'Packs', displayName: 'Packs', visible: true, width: '5%', cellFilter: 'number: 2' },
        //        { field: 'Qty', displayName: 'Qty', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'CancelReason', displayName: 'Cancel Reason', visible: true, width: '25%' },
        //        { field: 'AdjustReason', displayName: 'Adjust Reason', visible: false, width: '15%' },
        //        { field: 'RowVersion', displayName: 'RowVersion', visible: false, width: '7%' },

        //        {
        //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.C" ng-click="Approve(row.entity)" name="approve"  value="Approve">'
        //            , width: '10%'
        //        }
        //    ]
        //};


        $scope.searchlines = function searchlines() {
            if (_timeout) { // if there is already a timeout in process cancel it
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getStraightLines($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        $scope.Approve = function (obj) {
            globalService.openLoadingWindow('Saving..');
            httpService.savedata(urlLineAdjustCancel, obj, function (response) {
                if (response.status === 200) {
                    alert('Success');
                    $scope.$emit('ADJUSTMENT_APPROVE_EVENT', true);
                } else {
                    $scope.message = response.message;
                }
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        }


    });


})();