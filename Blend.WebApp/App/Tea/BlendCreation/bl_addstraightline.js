(function () {
    var urlBlenCreation = 'api/Blend';
    var urlBlenAddRemove = 'api/BlendAddRemove';

    app.controller('blendAddSLController', function ($scope, $rootScope, $modalInstance, $route, $filter, item, blend, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        var bybags = 1;
        var byqty = 2;
        var stocktype_strline = 1;
        var stocktype_blends = 2;
        var messagedata = { message: '', status: true };
        $scope.PackDR = 0;
        $scope.PackD = 0;
        $scope.showLoader = false;
        $scope.message = '';
        $scope.isOpenAddSLModal = false;
        $scope.SelectedBlend = blend;

        $scope.BlendItems = [];

        $scope.disableQty = true;

        $scope.AddType = bybags;

        $scope.addtypes = [{ addtype: 1, desc: 'Bags' }, { addtype: 2, desc: 'Qty' }];

        $scope.New = { 'BoxNo': '', 'Pack': 0, 'Net': 0, 'Qty': 0, 'ReducedQty': 0, 'PackR': 0, 'NetR': 0, 'QtyR': 0, 'ReducedQtyR': 0, 'BlendQty': 0 };

        $scope.open = function () {
            $scope.isOpenAddSLModal = true;
        };

        $scope.close = function () {
            $scope.isOpenAddSLModal = false;
        };

        getStraightlinestockItem(item);

        function getStraightlinestockItem(boxNoData) {
            $scope.showLoader = true;
            var boxNo = globalService.replaceSlash(boxNoData);
            //globalService.openLoadingWindow('Loading..');

            httpService.getdata(urlBlenCreation + '/GetStraghtLineStockByBoxNo/' + boxNo, function (results) {
                if (results.data.BoxNo != null) {

                    $scope.New.BoxNo = results.data.BoxNo;
                    $scope.New.Pack = results.data.Pack;
                    $scope.New.Net = results.data.Net;
                    $scope.New.Qty = results.data.Qty;
                    $scope.New.Id = results.data.Id;
                    $scope.New.StockItemId = results.data.StockItemId;
                    $scope.New.ReducedQty = results.data.ReducedQty;
                    //Repeating line
                    $scope.New.PackR = results.data.PackR;
                    $scope.New.NetR = results.data.NetR;
                    $scope.New.QtyR = results.data.QtyR;
                    $scope.New.IdR = results.data.IdR;
                    $scope.New.ReducedQtyR = results.data.ReducedQtyR;
                    $scope.New.StockItemIdR = results.data.StockItemIdR;
                    $scope.PackDR = results.data.PackR;
                    $scope.PackD = results.data.Pack;
                }
                else {
                    $scope.New.BoxNo = 'N/A';
                    $scope.New.Pack = 0;
                    $scope.New.Net = 0;
                    $scope.New.Qty = 0;
                    $scope.New.Id = 0;
                    $scope.New.StockItemId = 0;
                    $scope.New.ReducedQty = 0;
                    $scope.New.PackR = 0;
                    $scope.New.NetR = 0;
                    $scope.New.QtyR = 0;
                    $scope.New.IdR = 0;
                    $scope.New.StockItemIdR = 0;
                    $scope.New.ReducedQtyR = 0;
                    $scope.PackDR = 0;
                    $scope.message = 'No data found';
                    $scope.messageclass = "alert alert-danger";

                };
                $scope.showLoader = false;
                $scope.$emit('ADDSL_POPUP_WINDOW_EVENT', true);
                $scope.$emit('LOAD_DATA_EVENT', true);
            });
            httpService.getdata(urlBlenCreation + '/GetStraghtLineStock?id=' + $scope.New.Id+ '&searchby=BoxNo', function (results) {
                $('#packs1').focus();

            });
        }

        $scope.qtydisablestatus = function () {
            if ($scope.AddType === byqty) {
                return false;
            }
            return true;
        }

        $scope.onTextClick = function ($event) {
            $event.target.select();
        };

        $scope.switchEnterBox = function () {
            if ($scope.AddType === bybags) {
                angular.element(document.getElementById('blndqty'))[0].disabled = true;
                angular.element(document.getElementById('blndqtyr'))[0].disabled = true;

                angular.element(document.getElementById('packs1'))[0].disabled = false;
                angular.element(document.getElementById('packs2'))[0].disabled = false;
                //angular.element('#' + 'packs1').focus();
            }
            if ($scope.AddType === byqty) {
                angular.element(document.getElementById('packs1'))[0].disabled = true;
                angular.element(document.getElementById('packs2'))[0].disabled = true;

                angular.element(document.getElementById('blndqty'))[0].disabled = false;
                angular.element(document.getElementById('blndqtyr'))[0].disabled = false;
                //angular.element('#' + 'blndqty').focus()
            }
        };

        $scope.value = 0;

        $scope.$watch('New.Pack', function (value) {
            var pack = value;
            if (value * $scope.New.Net >= $scope.New.Qty) {
                $scope.New.BlendQty = $scope.New.Qty;// - $scope.New.ReducedQty;
            }
            else {
                $scope.New.BlendQty = value * $scope.New.Net;
            }
            $scope.New.TotalQty = $scope.New.BlendQty + $scope.New.BlendQtyR
            $scope.disableValidate();
        });

        $scope.$watch('New.PackR', function (value) {
            var pack = value;
            if (value * $scope.New.NetR >= $scope.New.QtyR) {
                $scope.New.BlendQtyR = $scope.New.QtyR;// - $scope.New.ReducedQtyR;
            }
            else {
                $scope.New.BlendQtyR = value * $scope.New.NetR;
            }
            $scope.New.TotalQty = $scope.New.BlendQty + $scope.New.BlendQtyR
            $scope.disableValidate();
        });

        $scope.btnAddDisable = false;

        $scope.disableValidate = function () {
            $scope.btnAddDisable = false;
            if ($scope.New.BlendQty > $scope.New.Qty) {
                $scope.btnAddDisable = true;
            }
            if ($scope.New.BlendQtyR > $scope.New.QtyR) {
                $scope.btnAddDisable = true;
            }
            if ($scope.New.Pack > $scope.PackD) {
                $scope.btnAddDisable = true;
            }
            if ($scope.New.PackR > $scope.PackDR) {
                $scope.btnAddDisable = true;
            }
        };

        //$scope.getStraightlinestockItem = function () {
        //    if ($scope.boxNoData.length === 8) {
        //        var boxNo = globalService.replaceSlash($scope.boxNoData);//.replace("/", "~");

        //        getStraightlinestockItem(boxNo);
        //    }
        //};

        $scope.getStraightlinestockItemSelectByBoxNo = function (boxNo) {
            var boxNo = globalService.replaceSlash(boxNo);//.replacegetStraightlinestockItem("/", "~");
            $scope.setvisibleaddwindow = true;
            $scope.setvisiblestockwindow = false;
            getStraightlinestockItem(boxNo);

        };

        var Validate = function () {
            if (!angular.isNumber(parseFloat($scope.New.Pack)) | angular.isUndefined($scope.New.Pack) | $scope.PackD < parseFloat($scope.New.Pack)) {
                $scope.message = "Error in [Packets]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if (!angular.isNumber($scope.New.PackR) | angular.isUndefined($scope.New.PackR)) {
                if ($scope.PackDR < parseFloat($scope.New.PackR)) {
                    $scope.message = "Error in [Packets] Repeting line";
                    $scope.messageclass = "alert alert-danger";
                    return false
                }
            }
            if ($scope.New.Qty === parseFloat($scope.New.BlendQty) && parseFloat($scope.New.Pack) < $scope.PackD) {
                $scope.message = "Error in [Packets].Use all packets";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if ($scope.New.QtyR === parseFloat($scope.New.BlendQtyR) && parseFloat($scope.New.PackR) < $scope.PackDR) {
                $scope.message = "Error in [Packets].Use all packets";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if (parseFloat($scope.New.BlendQtyR) === 0 && parseFloat($scope.New.BlendQty) === 0) {
                $scope.message = "Error in [Blend Qty]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if (parseFloat($scope.New.BlendQtyR) > $scope.New.QtyR | parseFloat($scope.New.BlendQty) > $scope.New.Qty) {
                $scope.message = "Error in [Blend Qty]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            if (parseFloat($scope.New.BlendQty) === 0 | parseFloat($scope.New.BlendQtyR) === 0) {
                var zerocount = 0;
                if (parseFloat($scope.New.BlendQty) === 0) {
                    zerocount = 1;
                }
                if (parseFloat($scope.New.BlendQtyR) === 0) {
                    zerocount = zerocount + 1;
                }
                if (zerocount > 1) {
                    $scope.message = "No Blend Qty found to Add";
                    $scope.messageclass = "alert alert-danger";
                    return false;
                }
            }
            return true;
            //else if()
        };
        var SetPacksWhenAllocateByQty = function () {
            var pks = $scope.New.BlendQty === 0 ? 0 : $scope.New.BlendQty / $scope.New.Net;
            $scope.New.Pack = pks > $scope.PackD ? $scope.PackD : Math.ceil(pks);
            var pkrs = $scope.New.BlendQtyR === 0 ? 0 : $scope.New.BlendQtyR / $scope.New.NetR;
            $scope.New.PackR = pkrs > $scope.PackDR ? $scope.PackDR : Math.ceil(pkrs);
            //Overide packs when 0
            if ($scope.PackD === 0)
                $scope.New.Pack = 0;
            //$scope.New.Pack = $scope.PackD === 0 ? 0 : pks;
            if ($scope.PackDR === 0)
                $scope.New.PackR = 0;
            //$scope.New.PackR = $scope.PackDR === 0 ? 0 : pkrs;
        };
        $scope.AddSLToBlend = function () {
            if (Validate()) {
                //This is for setting up the packet when add as Qty direct
                if ($scope.AddType === byqty) {
                    SetPacksWhenAllocateByQty();
                }
                $scope.BlendItems.length = 0;//initialize
                $scope.BlendItems.push({
                    BlendId: $scope.SelectedBlend.BlendId, StockId: $scope.New.Id,
                    StockItemId: $scope.New.StockItemId, ItemName: $scope.New.BoxNo,
                    Packs: $scope.New.Pack, Qty: $scope.New.Qty, BlendQty: $scope.New.BlendQty
                });
                //Repeating line
                $scope.BlendItems.push({
                    BlendId: $scope.SelectedBlend.BlendId, StockId: $scope.New.IdR,
                    StockItemId: $scope.New.StockItemIdR, ItemName: $scope.New.BoxNo,
                    Packs: $scope.New.PackR, Qty: $scope.New.QtyR, BlendQty: $scope.New.BlendQtyR
                });
                $scope.btnAddDisable = true;
                $scope.showLoader = true;
                //globalService.openLoadingWindow('Loading..');

                httpService.savedata(urlBlenAddRemove, $scope.BlendItems, function (response) {
                    if (response.status === 200) {
                        $scope.cancel();
                        $scope.showLoader = false;
                        $scope.$emit('ADDSL_BLEND_EVENT', true);
                        messagedata.message = 'Successfully Added Straight-Line: ' + $scope.New.BoxNo + ' (Bags: ' + $scope.New.Pack + ' Qty: ' + $scope.New.Qty + ')';
                        messagedata.status = true;
                        $scope.$emit('ADDLINE_MSG_EVENT', messagedata);
                        $scope.btnAddDisable = false;
                        

                    } else {
                        alert(response.message);
                        $scope.btnAddDisable = false;
                        $scope.showLoader = false;

                    }
                    //$scope.$emit('LOAD_DATA_EVENT', true);

                });
            };
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
            $('#searchby').focus();
        };

    });
})();