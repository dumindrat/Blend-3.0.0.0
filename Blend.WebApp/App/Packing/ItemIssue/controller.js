(function () {
    var urlDocument = 'api/Document';

    app.controller('itemIssueController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.rndata = {};
        $scope.searchstr = '';
        init();
        function init() {
            $scope.recordCount = 'Loading..';
            $scope.rndata = [];
        }

        $rootScope.$on('ISN_EVENT', function (event, data) {
            getItemIssueNotes('all');
        });

        getItemIssueNotes('all');

        function getItemIssueNotes(searchstr) {
            init();
            var blendNo = globalService.replaceSlash(searchstr);
            httpService.getdata('api/ItemIssue/GetAllIssueNotes/' + searchstr, function (results) {
                $scope.data = results.data;
                $scope.recordCount = results.data.length;
            });
        };



        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getItemReceiveNotes($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);
        };

        $scope.print = function (obj) {
            var response = {};
            $http.post('api/IssueNotePrint', obj, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.dataLoading = false;


            }).error(function (data) {
                alert('Error in processing report')
                $scope.dataLoading = false;
            });

        };

        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to delete?");
            if (retVal == true) {
                httpService.deletedata('api/ItemIssue' + '/' + id, function (response) {
                    if (response.status == 200) {
                        alert('Data deleted successfully');
                        getItemIssueNotes("all");
                    }
                })
            }
        };

        $scope.openCreateView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemIssue/Create.html' + '?ver=123',
                controller: 'isnCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

        ////Added ITEMS
        //$scope.openIssueItems = function (p, size) {
        //    var modalInstance = $modal.open({
        //        templateUrl: '/App/packing/ItemIssue/Items.html' + '?ver=123',
        //        controller: 'issueItemsController',
        //        windowClass: 'app-modal-md-window',
        //        resolve: {
        //            item: function () {
        //                return p;
        //            }
        //        }
        //    });
        //    //modalInstance.result.then(function (selectedObject) {
        //    //});
        //};

        $scope.openIssueNoteItems = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/ItemIssue/IssueNoteItems.html' + '?ver=123',
                controller: 'updateIssueItemController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            //modalInstance.result.then(function (selectedObject) {
            //});
        };

    });

    app.controller('isnCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.New = { 'BlendId': 0, 'IssueNoteNo': '', 'IsAutoDocNo': false };
        }
        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
            $scope.boxnodisabled = true;
        }
        else {
            $scope.New.IsAutoDocNo = true;
            AutoGenNo();
        }

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        globalService.openLoadingWindow('Loading..');

        function AutoGenNo() {
            //$scope.showLoader = true;
            globalService.openLoadingWindow('Loading..');
            httpService.getdata(urlDocument + '/GetDocumentNo/' + 5, function (results) {
                $scope.New.IssueNoteNo = results.data.FormattedDocNo;
                angular.element(document.getElementById('isnno'))[0].disabled = true;
                $scope.showLoader = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        }

        $scope.setAutoDocNo = function (status) {
            if (status === true) {
                AutoGenNo();
            }
            else {
                angular.element(document.getElementById('isnno'))[0].disabled = false;
                $scope.New.IssueNoteNo = '';
                angular.element('#' + 'isnno').focus()
            }
        };

        getBlendList('all');

        function getBlendList(searchstr) {
            httpService.getdata('api/Blend/GetBlends/' + searchstr, function (results) {
                $scope.blenddata = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        $scope.save = function () {
            if (angular.isUndefined(item)) {
                if ($scope.validate($scope.New)) {
                    httpService.savedata('api/ItemIssue', $scope.New, function (response) {
                        $scope.message = '';
                        if (response.status == 200) {
                            $scope.messageclass = "alert alert-success";
                        }
                        $scope.message = response.message;
                        $scope.$emit('ISN_EVENT', true);
                        $scope.dataLoading = false;
                        $scope.New = {};

                    })
                }
            }
            else {
                httpService.updatedata('api/ItemIssue', $scope.New, function (response) {
                    $scope.message = '';
                    if (response.status == 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.message = response.message;
                    $scope.$emit('ISN_EVENT', true);
                    $scope.dataLoading = false;
                    $scope.New = {};

                })
            }
        };

        $scope.validate = function (item) {
            if (item.IssueNoteNo == undefined || item.IssueNoteNo == 0) {
                alert('Invalid Issue Note No');
                return false;
            }
            if (item.BlendId == undefined || item.BlendId == 0) {
                alert('Invalid Blend');
                return false;
            }
            return true;
        }


    });

    app.controller('issueItemsController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        $scope.searchstr = '';
        $scope.loadingdata = false;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.selectedItem = item;
        $scope.selected = {};

        $scope.model = {
        };
        $scope.recordCount = 0;
        $scope.itemcolumns = [
       { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
       { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
       { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
       { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
       { text: "UOM", predicate: "UomName", sortable: true, hide: false },
       { text: "Stock Qty", predicate: "Qty", sortable: true, hide: false },
       { text: "Price", predicate: "Price", sortable: true, hide: false },
       { text: "Stock Date", predicate: "ReceivedDate", sortable: true, hide: false },
        ];

        //Added lines
        $scope.changeitem = function (item) {
            $scope.selected = angular.copy(item);
        };

        $scope.reset = function () {
            $scope.selected = {};
        };

        $scope.getTemplate = function (item) {
            if (item.Id === $scope.selected.Id) return 'edit';
            else return 'display';
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

        getItems('all');

        function getItems(id) {
            $scope.loadingdata = true;
            httpService.getdata('api/ItemIssue/GetAllStockItems/' + id, function (results) {
                $scope.stockitems = results.data;
                $scope.recordCount = results.data.length;
                $scope.loadingdata = false;
            });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

       

        $scope.additem = function addItems(item) {
            var additem = { 'IssueNoteId': 0, 'ItemId': 0, 'Qty': 0, 'StockId': 0 };
            additem.IssueNoteId = $scope.selectedItem.Id;
            additem.StockId = item.Id;
            additem.ItemId = item.ItemId;
            additem.Qty = item.Qty;
            if ($scope.validate(additem)) {
                globalService.openLoadingWindow('Wait..');
                httpService.savedata('api/IssueNoteAddItem', additem, function (response) {
                    $scope.message = '';
                    if (response.status == 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.message = response.message;
                    $scope.$emit('ISN_EVENT', item.ItemCode + '-' + item.Qty + ' ' + item.UomName);
                    $scope.dataLoading = false;
                    getItems($scope.searchstr === '' ? "all" : $scope.searchstr);
                    $scope.reset();
                    $scope.$emit('LOAD_DATA_EVENT', true);

                })
            }
        };

        $scope.validate = function (item) {
            if (item.Qty == undefined || item.Qty == 0 || item.Qty > $scope.selected.Qty) {
                alert('Invalid Issue Qty');
                return false;
            }
            
            return true;
        }

    });

})();