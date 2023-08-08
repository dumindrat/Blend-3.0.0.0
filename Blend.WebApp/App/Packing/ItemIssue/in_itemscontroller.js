(function () {
    var urlPO = 'api/ItemIssue';

    app.controller('updateIssueItemController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.messageclass = "";
        $scope.message = "";
        $scope.selectedItem = item;
        $scope.issueditem = [];
        $scope.recordCount = 0;
        $scope.itemcolumns = [
       { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
       { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
       { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
       { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
       { text: "UOM", predicate: "UomName", sortable: true, hide: false },
       { text: "Qty", predicate: "OrderQty", sortable: true, hide: false },
       { text: "Price", predicate: "OrderPrice", sortable: true, hide: false },
       { text: "CreateUser", predicate: "CreateUser", sortable: true, hide: false },
       { text: "CreateDate", predicate: "CreateDate", sortable: true, hide: false },
        ];

        $rootScope.$on('ISN_EVENT', function (event, data) {
            $scope.messageclass = "alert alert-success";
            $scope.message = "Successfully Issued Item : " + data;
            getItems($scope.selectedItem.Id);
        });


        getItems($scope.selectedItem.Id);

        function getItems(id) {
            httpService.getdata('api/ItemIssue/GetIssueNoteItemsById/' + id, function (results) {
                $scope.issueditem = results.data;
                $scope.recordCount = results.data.length;
            });
        };

        $scope.openIssueItemWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/ItemIssue/Items.html' + '?ver=123',
                controller: 'issueItemsController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return $scope.selectedItem;
                    }
                }
            });
        };

        $scope.removeitem = function deleteItems(item) {
            var retVal = confirm("Are you sure want to remove item : " + item.ItemCode + '?');
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');
                item.PoId = $scope.selectedItem.Id;
                httpService.updatedata('api/IssueNoteAddItem', item, function (response) {
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

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.getTemplate = function (item) {
            return 'display';
        };


    });

})();