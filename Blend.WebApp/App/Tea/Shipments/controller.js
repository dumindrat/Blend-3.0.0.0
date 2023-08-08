(function () {
    var urlTeaShipment = 'api/TeaShipment';
    var urlPackGoods = 'api/PackGood';

    app.controller('teaShipmentController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout,
        httpService, globalService) {

        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.content = null;
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('SHIPMENT_EVENT', function (event, data) {
            getClosedBlends('all');
        });

        $rootScope.$on('PACKGOODS_EVENT', function (event, data) {
            getClosedBlends('all');
        });
        



        $scope.CheckPackGoodConvertion = function (blend) {
            if (blend.IsPackGoodsCreate)
                return true;
            return false;
        }

        getClosedBlends('all');

        function getClosedBlends(searchstring) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var blendNo = globalService.replaceSlash(searchstring);

            httpService.getdata(urlTeaShipment + '/GetClosedBlends/' + blendNo, function (results) {
                $scope.BlendData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();
            });
        };

        $scope.searchblend = function searchblend() {
            if (_timeout) { // if there is already a timeout in process cancel it
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getClosedBlends($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        $scope.openShipmentWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Shipments/Shipments.html',
                controller: 'teaShipmentCreateController',
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

        $scope.openPackGoodWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Shipments/PackGoods.html',
                controller: 'packGoodOptionsController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('teaShipmentCreateController', function ($scope, $rootScope, $modalInstance, $route, item, httpService, globalService) {
        $scope.SelectedBlend = item;
        $scope.ExportTypes = [{ exptype: "BULK" }, { exptype: "PACK" }, { exptype: "BAGS" }];
        $scope.New = { 'BlendId': item.BlendId, 'Packs': 0, 'Net': 0, 'RowVersion': item.RowVersion, 'Qty': 0 };
        $scope.message = '';
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        //Authentication fails. emit from data.service
        $scope.dataLoading = false;

        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.calqty = function () {
            $scope.New.Qty = $scope.New.Net * $scope.New.Packs;
        }

        getPurchaseAndStocks(item.BlendNo);

        function getPurchaseAndStocks(boxno) {
            $scope.linedataLoading = true;

            //httpService.getdata(urlPurchaseAndStock + '/GetLineDetails/' + search, function (results) {
            //    $scope.StraightLineData = results.data;
            //    $scope.linedataLoading = false;

            //});
        };

        function Validate() {
            if ($scope.New.Qty > $scope.SelectedBlend.AvailableStock) {
                alert('Available Stock is not enough for the shipment');
                return false;
            }
            return true;
        }

        $scope.save = function () {
            $scope.dataLoading = true;
            if (Validate()) {
                httpService.savedata(urlTeaShipment, $scope.New, function (response) {
                    if (response.status === 200) {
                        alert(response.message);
                        $scope.$emit('SHIPMENT_EVENT', true);
                        $scope.cancel();
                    }
                    $scope.dataLoading = false;

                })
            }
            $scope.dataLoading = false;
        }



    });

    app.controller('packGoodOptionsController', function ($scope, $rootScope, $modal, $modalInstance, item, httpService, globalService) {
        $scope.Blend = {};
        $scope.showLoader = false;
        $scope.SelectedBlend = item;
        
        $scope.New = { 'RowVersion': item.RowVersion, 'BlendId': item.BlendId, 'BlendNo': item.BlendNo, 'BB': item.AvailableBalance, 'GoodTea': item.AvailableBalance, 'FlavouredTea': 0, 'PackGoodTea': 0 };
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        var Validate = function () {
            
            if ($scope.SelectedBlend.AvailableBalance != $scope.New.GoodTea + $scope.New.FlavouredTea + $scope.New.PackGoodTea) {
                alert('Invalid convertion');
                return false;
            }
            return true;
        }

        $scope.save = function () {
            var retVal = confirm("Are you sure want to Convert Pack Goods?");
            if (retVal == true) {
                $scope.dataLoading = true;
                if (Validate()) {
                    httpService.savedata(urlPackGoods, $scope.New, function (response) {
                        if (response.status === 200) {
                            alert(response.message);
                            $scope.$emit('PACKGOODS_EVENT', true);
                            $scope.cancel();
                        }
                        $scope.dataLoading = false;

                    })
                }
                $scope.dataLoading = false;
            }
        }

    });

})();