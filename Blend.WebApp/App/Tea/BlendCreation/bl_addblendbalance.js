(function () {
    var urlBlenCreation = 'api/Blend';
    var urlBlenAddRemove = 'api/BlendAddRemove';

    app.controller('blendAddBLController', function ($scope, $rootScope, $modalInstance, $route, $filter, item, blend, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.BL = { 'BlendItemStockId': 0, 'BlendItemStockItemId': 0, 'BlendItemQty': 0, 'BlendItemStockQty': 0, 'BlendItemRate': 0, 'BlendItemValue': 0, 'BlendItemStockName': '' };
        $scope.message = "";
        $scope.SelectedBlend = blend;
        $scope.ItemName = item.ItemName;
        $scope.showLoader = false;
        var messagedata = { message: '', status: true };

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
 
        getBlendStockItemByStockId(item);

         function getBlendStockItemByStockId() {
            $scope.message = "";
            $scope.BL.BlendItemStockId = item.Id;
            $scope.BL.BlendItemStockItemId = item.StockItemId;
            $scope.BL.BlendItemQty = item.Qty;
            $scope.BL.BlendItemStockQty = item.Qty;
            $scope.BL.BlendItemRate = item.Rate;
            $scope.BL.BlendItemValue = item.Value;
            $scope.BL.BlendItemStockName = item.StockName;
            httpService.getdata(urlBlenCreation + '/GetStraghtLineStock/' + item.StockItemId, function (results) {
               
                $('#blenditemqty').focus();
            });
            
        };

        var ValidateBlendQty = function () {
            if (!angular.isNumber(parseFloat($scope.BL.BlendItemQty)) | angular.isUndefined($scope.BL.BlendItemQty)
                | parseFloat($scope.BL.BlendItemQty) > $scope.BL.BlendItemStockQty) {
                $scope.message = "Error in [Allocated Blend Qty]";
                $scope.messageclass = "alert alert-danger";
                return false
            }
            return true;
        }


        $scope.BlendStockItem = { 'StockId': 0, 'StockItemId': 0, 'BlendId': 0, 'ItemName': '', 'Packs': 0, 'BlendQty': 0 };
        $scope.BlendItems = [];

        $scope.AddBLToBlend = function () {
            $scope.showLoader = true;

            if (ValidateBlendQty()) {
                $scope.BlendStockItem.BlendId = $scope.SelectedBlend.BlendId;
                $scope.BlendStockItem.StockId = $scope.BL.BlendItemStockId;
                $scope.BlendStockItem.StockItemId = $scope.BL.BlendItemStockItemId;
                $scope.BlendStockItem.ItemName = $scope.BL.BlendItemStockName;
                $scope.BlendStockItem.Packs = 0;
                $scope.BlendStockItem.BlendQty = $scope.BL.BlendItemQty;
                $scope.BlendItems.push($scope.BlendStockItem);

                httpService.savedata(urlBlenAddRemove, $scope.BlendItems, function (response) {
                    if (response.status === 200) {
                        $scope.$emit('ADDBL_BLEND_EVENT', true);
                        $scope.cancel();
                        $scope.showLoader = false;
                        messagedata.message = 'Successfully Added Blend: ' + $scope.BL.BlendItemStockName + ' (Qty: ' + $scope.BL.BlendItemQty + ')';
                        messagedata.status = true;
                        $scope.$emit('ADDLINE_MSG_EVENT', messagedata);
                    } else {
                        $scope.message = response.message;
                        $scope.showLoader = false;
                    }
                });
            };
        };

        $scope.cancel = function () {
            $scope.$emit('ADDBL_POPUP_WINDOW_EVENT', true);
            $modalInstance.dismiss('Close');
            $('#searchby').focus();
        };

    });
})();