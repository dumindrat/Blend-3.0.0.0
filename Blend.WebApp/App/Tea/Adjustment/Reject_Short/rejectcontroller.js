(function () {
    var urlLineReject = 'api/RejectLine';

    app.controller('rejectController', function ($http, $scope, $rootScope, $modalInstance, $route, $timeout, item, httpService, globalService) {

        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.SelectedBoxNo = item;
        $scope.PacksD = item.Packs;
        $scope.QtyD = item.Qty;
        $scope.btnRejectDisable = true;

        $scope.New = {
            'Id': item.Id, 'StockItemId': item.StockItemId,
            'Net': item.NetWeight, 'Packs': item.Packs,
            'Qty': item.Qty, 'ReducedQty': item.ReducedQty,
            'Reason': '',
            'RowVersion': item.RowVersion
        };


        $scope.$watch('New.Packs', function (value) {
            var pack = value;
            if (value * $scope.New.Net >= $scope.QtyD) {
                $scope.New.Qty = $scope.QtyD;// - $scope.New.ReducedQty;
            }
            else {
                $scope.New.Qty = (value * $scope.New.Net);// - $scope.New.ReducedQty;
            }
            $scope.disableValidate();
        });

        $scope.disableValidate = function () {
            $scope.btnRejectDisable = false;
            if ($scope.New.Packs === 0) {
                $scope.btnRejectDisable = true;
            }
            if ($scope.New.Qty > $scope.QtyD) {
                $scope.btnRejectDisable = false;
            }

            if ($scope.New.Packs > $scope.PacksD) {
                $scope.btnRejectDisable = true;
            }

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
            if (!angular.isNumber(parseFloat($scope.New.Packs)) | angular.isUndefined($scope.New.Packs) | $scope.New.Packs === 0) {
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

        $scope.Reject = function () {
            var retVal = confirm("Are you sure want to Reject?");
            if (retVal == true) {
                if (Validate()) {
                    globalService.openLoadingWindow('Saving..');

                    $scope.btnRejectDisable = false;
                    var rejectObj = {
                        'PurchaseId': $scope.New.StockItemId, 'StockId': $scope.New.Id,
                        'AdjustBags': $scope.New.Packs, 'AdjustQty': $scope.New.Qty, 'Reason': $scope.New.Reason, 'RowVersion': $scope.SelectedBoxNo.RowVersion
                    };
                    httpService.savedata(urlLineReject, rejectObj, function (response) {
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