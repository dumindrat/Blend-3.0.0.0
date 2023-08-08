(function () {
    var urlLineAdjust = 'api/AdjustLine';

    app.controller('adjustmentOpsController', function ($http, $scope, $rootScope, $sce, $modal, $modalInstance, $route, $timeout, item, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.SelectedBoxNo = item;
        $scope.message = false;
        $rootScope.$on('ADJUSTMENT_CANCEL_EVENT', function (event, data) {
            getAdjustedLine(item.Id);
        });

        getAdjustedLine(item.Id);

        function getAdjustedLine(Id) {
            globalService.openLoadingWindow('Loading..');

            httpService.getdata(urlLineAdjust + '/GetAdjustedLine/' + Id, function (results) {
                $scope.AdjustedLineData = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);
            });
        };

        $scope.adjustedlinecolumns = [
                  { text: "AdjustmentId", predicate: "Id", sortable: true, dataType: "number", hide: true },
                  { text: "PurchaseId", predicate: "StockItemId", sortable: false, hide: true },
                  { text: "Packs", predicate: "Packs", sortable: true, hide: false },
                  { text: "Qty", predicate: "Qty", sortable: true, hide: false },
                  { text: "Type", predicate: "AdjustmentType", sortable: true, hide: false },
                  { text: "Reason", predicate: "Reason", sortable: true, hide: false },
                  { text: "Status", predicate: "Status", sortable: true, hide: false },
                  { text: "StatusId", predicate: "StatusId", sortable: true, hide: true },
                  { text: "User", predicate: "User", sortable: true, hide: false },
                  { text: "RowVersion", predicate: "RowVersion", sortable: true, hide: true },
                  { text: "Option", predicate: "Option", sortable: true, hide: true }


        ];

        $scope.checkDisabled = function (status) {
            return status == 0 ? false : true;

        }

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        var Validate = function (p) {
            if (p.IsVoid == 'Yes') {
                $scope.message = "Alredy request for cancellation";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            return true;
        }

        $scope.CancelConfirm = function (p) {
            $scope.message = "";
            if (Validate(p)) {
                $scope.openCancelWindow(p);
            }
        }

        $scope.PaymentSettle = function (id) {
            var retVal = confirm("Are you sure want to Settle the Payment for this Adjustment ?");
            if (retVal == true) {
                globalService.openLoadingWindow('Loading..');

                httpService.getdata(urlLineAdjust + '/GetAdjustPaymentSettle/' + id, function (results) {
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.message = "Successfully settled the Payment";
                    $scope.messageclass = "alert alert-success";
                });
            }
        }

        $scope.openCancelWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: 'App/Tea/Adjustment/Reject_Short/AdjustmentsCancel.html',
                controller: 'adjustmentConfirmController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    },
                    box: function () {
                        return $scope.SelectedBoxNo;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };
    });

    app.controller('adjustmentConfirmController', function ($http, $scope, $rootScope, $modalInstance, $route, $timeout, item, box, httpService, globalService) {
        var urlLineAdjust = 'api/AdjustLine';
        $scope.selectedObject = item;
        $scope.confirmReason = '';
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };



        $scope.CancelRequest = function () {
            var retVal = confirm("Are you sure want to Reverse this Adjustment ?");
            if (retVal == true) {
                globalService.openLoadingWindow('Saving..');

                var cancelObj = { 'Id': $scope.selectedObject.AdjustmentId, 'CancelReason': $scope.confirmReason, 'RowVersion': $scope.selectedObject.RowVersion };
                httpService.savedata(urlLineAdjust, cancelObj, function (response) {
                    if (response.status === 200) {
                        alert('Success');
                        $scope.$emit('ADJUSTMENT_CANCEL_EVENT', true);
                        $scope.btnRejectDisable = false;
                        $scope.$emit('LOAD_DATA_EVENT', true);
                        $scope.cancel();
                    } else {
                        $scope.message = response.message;
                        $scope.btnRejectDisable = false;
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    }

                });
            }
        }
    });
})();