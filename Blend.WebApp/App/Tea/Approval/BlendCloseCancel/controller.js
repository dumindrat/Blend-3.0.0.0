(function () {
    var urlBlenCloseCancel = 'api/BlendCloseReversalAppove';

    app.controller('blendCloseReversalAppove', function ($http, $scope, $sce, $rootScope, $modal, $route, $timeout, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('ADJUSTMENT_APPROVE_EVENT', function (event, data) {
            getBlendList('all');
        });



        getBlendList('all');

        function getBlendList(blendNo) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            httpService.getdata(urlBlenCloseCancel + '/GetClosedBlendsCancelRequests/' + blendNo, function (results) {
                $scope.BlendData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });
        };



        //$scope.gridOptions = {
        //    data: 'BlendData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'Id', displayName: 'Id', visible: false, width: '5%' },
        //        { field: 'CategorizedId', displayName: 'CategorizedId', visible: false, width: '7%' },
        //        { field: 'StockId', displayName: 'StockId', visible: false, width: '7%' },
        //        { field: 'StockItemId', displayName: 'StockItemId', visible: false, width: '7%' },
        //        { field: 'BlendNo', displayName: 'BlendNo', visible: true, width: '10%' },
        //        { field: 'LineQty', displayName: 'Blend Qty', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'ExportQty', displayName: 'Exp.Qty', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'CreateQty', displayName: 'Physical Qty', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'BB', displayName: 'BB', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'BG', displayName: 'BG', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'BL', displayName: 'BL', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'SIFTED', displayName: 'Sifted', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'Rate', displayName: 'Rate', visible: true, width: '7%', cellFilter: 'number: 2' },
        //        { field: 'RequestDate', displayName: 'RequestDate', visible: true, width: '10%' },
        //        { field: 'RequestBy', displayName: 'RequestBy', visible: true, width: '10%' },
        //        { field: 'Reason', displayName: 'Reason', visible: true, width: '15%' },
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
                getBlendList($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        $scope.Approve = function (obj) {
            var retVal = confirm("Are you sure want to Reverse this Blend Close transaction ?");
            if (retVal == true) {
                globalService.openLoadingWindow('Saving..');
                var cancelObj = { 'BlendId': obj.StockItemId, 'RowVersion': obj.RowVersion }
                httpService.savedata(urlBlenCloseCancel, cancelObj, function (response) {
                    if (response.status === 200) {
                        alert('Success');
                        $scope.$emit('ADJUSTMENT_APPROVE_EVENT', true);
                    } else {
                        $scope.message = response.message;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });

            }
        }


    });


})();