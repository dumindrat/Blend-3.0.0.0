(function () {
    var urlPO = 'api/PurchaseOrder';
    var urlDocument = 'api/Document';

    app.controller('updateItemController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.messageclass = "";
        $scope.message = "";
        $scope.selectedItem = item;
        $scope.recordCount = 0;
        $scope.itemcolumns = [
       { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
       { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
       { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
       { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
       { text: "UOM", predicate: "UomName", sortable: true, hide: false },
       { text: "Order Qty", predicate: "OrderQty", sortable: true, hide: false },
       { text: "Order Price", predicate: "OrderPrice", sortable: true, hide: false },
       { text: "GRN Qty", predicate: "ReceivedQty", sortable: true, hide: false },
       { text: "GRN Price", predicate: "ReceivedPrice", sortable: true, hide: false },
       { text: "Special Notes", predicate: "Comment", sortable: true, hide: false },
       { text: "Stock Receied Qty", predicate: "StkQty", sortable: true, hide: false },
        ];

        $rootScope.$on('IRN_EVENT', function (event, data) {
            $scope.messageclass = "alert alert-success";
            $scope.message = "Successfully updated Item : " + data;
            getItems($scope.selectedItem.Id);
        });


        getItems($scope.selectedItem.Id);

        function getItems(id) {
            httpService.getdata('api/ItemReceive/GetReceiveNotesItems/' + id, function (results) {
                $scope.received = results.data;
                $scope.recordCount = results.data.length;
            });
        };

        $scope.openChangeReceiveQtyView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReceive/UpdateItem.html' + '?ver=123',
                controller: 'changeReceivedQtyController',
                windowClass: 'app-modal-xxsm-window',
                resolve: {
                    item: function () {
                        return p;
                    },
                    id: function () {
                        return $scope.selectedItem.Id;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.getTemplate = function (item) {
            return 'display';
        };


    });

    app.controller('changeReceivedQtyController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $filter, $timeout, item, id, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var systemDate = globalService.getDate();
        var initDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.changeReqQty = false;
        $scope.changeReqPrice = false;
        $scope.disableQty = true;
        $scope.disablePrice = true;
        setTransaction();
        function setTransaction() {
            $scope.New = item;
            if (item.ReceivedQty == 0) {
                $scope.New.ReceivedQty = item.OrderQty;
                $scope.New.ReceivedPrice = item.OrderPrice;
                $scope.New.ReceivedDate = initDate;
            }
        }
        $scope.setRecQty = function (status) {
            if (status) {
                // $scope.changeReqQty = false;
                $scope.disableQty = false;
                angular.element('#' + 'irnqty').focus()

            }
            else {
                $scope.disableQty = true;
            }
        }
        $scope.setRecPrice = function (status) {
            if (status) {
                $scope.disablePrice = false;
                angular.element('#' + 'irnprice').focus()
            }
            else {
                $scope.disablePrice = true;
            }
        }
        $scope.New.ReceivedNoteId = id;
        $scope.recordCount = 0;

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.save = function () {
            if ($scope.validate($scope.New)) {
                httpService.updatedata('api/ItemReceive', $scope.New, function (response) {
                    $scope.message = '';
                    if (response.status == 200) {

                        $scope.$emit('IRN_EVENT', $scope.New.ItemCode);
                        $scope.cancel();
                    }
                    else {
                        $scope.messageclass = "alert alert-error";
                        $scope.message = response.message;
                    }
                })
            }
        };

        $scope.validate = function (item) {
            if (item.ReceivedQty == undefined || item.ReceivedQty > item.OrderQty) {
                alert('Invalid item Received Qty');
                return false;
            }
            if (item.ReceivedPrice == undefined || item.ReceivedPrice == 0) {
                alert('Invalid item Received Price');
                return false;
            }
            if (item.ReceivedDate == undefined) {
                alert('Invalid item Received Price');
                return false;
            }
            return true;
        }
    });
})();