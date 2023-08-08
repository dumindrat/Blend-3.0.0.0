(function () {
    var urlPO = 'api/PurchaseOrder';
    app.controller('addPOItemsController', function ($scope, $rootScope, $modalInstance, $route, $http, $timeout, item, globalService, httpService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.selectedPo = item;
        $scope.selectedAddedItem = {};
        $scope.stockitems = [];
        $scope.addeditems = [];
        $scope.searchText = '';
        $scope.model = {
            items: []
        };
        $scope.itemcolumns = [
               { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
               { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
               { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
               { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
               { text: "UOM", predicate: "UomName", sortable: true, hide: false },
               { text: "Order Qty", predicate: "Qty", sortable: true, hide: false },
               { text: "Order Price", predicate: "Price", sortable: true, hide: false },
               { text: "Note", predicate: "Comment", sortable: true, hide: false },
        ];

        getItems('all');

        function getItems(searchstr) {
            $scope.searchText = searchstr;
            httpService.getdata('api/PurchaseOrder/GetAllPOItems?id=' + searchstr + '&poid=' + $scope.selectedPo.Id, function (results) {
                $scope.model.items = results.data;//assignQty(results.data);
                $scope.recordCount = results.data.length;
            });
        };

        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getItems($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        $scope.selected = {};
        $scope.IsAdded = function (item) {
            if (item.IsAdded) {
                return true;
            }
            return false;
        };

        // gets the template to ng-include for a table row / item
        $scope.getTemplate = function (item) {
            if (item.Id === $scope.selected.Id) return 'edit';
            else return 'display';
        };

        $scope.editContact = function (item) {
            $scope.selected = angular.copy(item);
        };

        $scope.saveContact = function (idx) {
            console.log("Saving contact");
            $scope.model.items[idx] = angular.copy($scope.selected);
            $scope.reset();
        };

        $scope.reset = function () {
            $scope.selected = {};
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.addToPO = function (item) {
            if (validate(item)) {
                var payload = {};
                payload.POId = $scope.selectedPo.Id;
                payload.ItemId = item.Id;
                payload.Qty = item.Qty
                payload.Price = item.Price
                payload.Comment = item.Comment

                globalService.openLoadingWindow('Adding..');

                httpService.savedata('api/POAddRemoveItem', payload, function (response) {
                    if (response.status === 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.model.items = [];
                    $scope.message = response.message;
                    getItems($scope.searchText);
                    $scope.$emit('ADD_ITEM_PO', true);
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.reset();
                    angular.element('#itemsearch').focus()

                })
            }
        };

        var validate = function (item) {
            if (item.Qty == 0 || item.Qty == undefined) {
                alert("Invalid Qty")
                return false;
            }
            if (item.Price == 0 || item.Price == undefined) {
                alert("Invalid Price")
                return false;
            }
            return true;
        }

    });
})();