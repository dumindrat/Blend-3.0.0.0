(function () {
    var urlLineShort = 'api/ShortLine';

    app.controller('shortController', function ($http, $scope, $rootScope, $modalInstance, $route, $timeout, item, httpService, globalService) {

        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.SelectedBoxNo = item;
        $scope.PacksD = item.Packs;
        $scope.QtyD = item.Qty;
        $scope.btnShortDisable = true;

        $scope.New = {
            'Id': item.Id, 'StockItemId': item.StockItemId,
            'Net': item.NetWeight, 'Packs': item.Packs,
            'Qty': item.Qty, 'ReducedQty': item.ReducedQty,
            'Reason': '',
            'RowVersion': item.RowVersion
        };

        var SetPacksWhenAllocateByQty = function () {
            var pks = $scope.New.Qty === 0 ? 0 : $scope.New.Qty / $scope.New.Net;
            $scope.New.Packs = pks > $scope.PacksD ? $scope.PacksD : Math.ceil(pks);
            //Overide packs when 0
            //$scope.New.Packs = $scope.PacksD === 0 ? 0 : pks;
        };



        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        //angular.element('#' + 'packs').focus();

        var Validate = function () {
            if (!angular.isNumber(parseFloat($scope.New.Qty)) | angular.isUndefined($scope.New.Qty) | $scope.New.Qty === 0) {
                $scope.message = "Error in [Packets]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if (angular.isUndefined($scope.New.Reason) | ($scope.New.Reason.length) === 0) {
                $scope.message = "Error in [Reason]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            return true;
        }

        $scope.Short = function () {
            var retVal = confirm("Are you sure want to Short?");
            if (retVal == true) {
                if (Validate()) {
                    globalService.openLoadingWindow('Saving..');

                    SetPacksWhenAllocateByQty();
                    $scope.btnRejectDisable = false;
                    var rejectObj = {
                        'PurchaseId': $scope.New.StockItemId, 'StockId': $scope.New.Id,
                        'AdjustBags': $scope.New.Packs, 'AdjustQty': $scope.New.Qty, 'Reason': $scope.New.Reason, 'RowVersion': $scope.SelectedBoxNo.RowVersion
                    };
                    httpService.savedata(urlLineShort, rejectObj, function (response) {
                        if (response.status === 200) {
                            alert('Success');
                            $scope.$emit('ADJUSTMENT_EVENT', true);
                            $scope.btnRejectDisable = false;
                            $scope.cancel();
                        } else {
                            $scope.message = response.message;
                            $scope.btnRejectDisable = false;
                        }
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    });
                }
            }
        };
    });
})();