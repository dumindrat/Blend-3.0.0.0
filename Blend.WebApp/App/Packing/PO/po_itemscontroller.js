(function () {
    var urlPO = 'api/PurchaseOrder';
    var urlDocument = 'api/Document';

    app.controller('itemsAddController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.selectedItem = item;
        $scope.model = {
        };
        $scope.recordCount = 0;
        $scope.itemcolumns = [
       { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
       { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
       { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
       { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
       { text: "UOM", predicate: "UomName", sortable: true, hide: false },
       { text: "Order Qty", predicate: "Qty", sortable: true, hide: false },
       { text: "Order Price", predicate: "Price", sortable: true, hide: false },
       { text: "Value", predicate: "Value", sortable: true, hide: false },
       { text: "Note", predicate: "Comment", sortable: true, hide: false },
        ];
        //Added lines

        $rootScope.$on('ADD_ITEM_PO', function (event, data) {
            getItems($scope.selectedItem.Id);
        });

        $scope.openAddItemWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/PO/AddItem.html',
                controller: 'addPOItemsController',
                windowClass: 'app-modal-md-window',
                keyboard: true,
                resolve: {
                    item: function () {
                        return $scope.selectedItem;
                    }
                }
            });
        };



        getItems($scope.selectedItem.Id);

        function getItems(id) {
            httpService.getdata('api/PurchaseOrder/GetPOItems/' + id, function (results) {
                $scope.model = results.data;
                $scope.recordCount = results.data.length;
            });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.deleteitem = function deleteItems(item) {
            var retVal = confirm("Are you sure want to remove item : " + item.ItemCode + '?');
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');
                item.PoId = $scope.selectedItem.Id;
                httpService.updatedata('api/POAddRemoveItem', item, function (response) {
                    if (response.status === 200) {
                        alert("Success");
                        getItems($scope.selectedItem.Id);
                    } else {
                        alert(response.message);
                        $scope.disablestatus = false;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);
                });
            }
        };

    });
})();