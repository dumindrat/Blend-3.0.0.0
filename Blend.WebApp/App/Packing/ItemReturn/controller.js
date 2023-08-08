(function () {
    var urlDocument = 'api/Document';

    app.controller('itemReturnController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
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

        $rootScope.$on('RTN_EVENT', function (event, data) {
            getItemReturnNotes('all');
        });

        getItemReturnNotes('all');

        function getItemReturnNotes(searchstr) {
            init();
            var blendNo = globalService.replaceSlash(searchstr);
            httpService.getdata('api/ItemReturn/GetAllReturnNotes/' + searchstr, function (results) {
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


        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to delete?");
            if (retVal == true) {
                httpService.deletedata('api/ItemReturn' + '/' + id, function (response) {
                    if (response.status == 200) {
                        alert('Data deleted successfully');
                        getItemReturnNotes("all");
                    }
                })
            }
        };

        $scope.openCreateView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReturn/Create.html' + '?ver=123',
                controller: 'rtnCreateController',
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

        $scope.openReturnNoteItems = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/ItemReturn/ReturnNoteItems.html' + '?ver=123',
                controller: 'updateReturnItemController',
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

       

        $scope.options = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReturn/Options.html',
                controller: 'rtnOptionsController',
                windowClass: 'app-modal-xxsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

    });

    app.controller('rtnCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.New = { 'Id': 0, 'ReceiveNoteId': 0, 'ReturnNoteNo': '', 'IsAutoDocNo': false, 'Comment': '' };
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
            httpService.getdata(urlDocument + '/GetDocumentNo/' + 6, function (results) {
                $scope.New.ReturnNoteNo = results.data.FormattedDocNo;
                angular.element(document.getElementById('returnnoteno'))[0].disabled = true;
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
                $scope.New.ReturnNoteNo = '';
                angular.element('#' + 'returnnoteno').focus()
            }
        };

        getReceivedNoteList('all');

        function getReceivedNoteList(searchstr) {
            httpService.getdata('api/ItemReturn/GetAllReceiveNotes/' + searchstr, function (results) {
                $scope.receivednotedata = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        $scope.save = function () {
            if (angular.isUndefined(item)) {
                if ($scope.validate($scope.New)) {
                    httpService.savedata('api/ItemReturn', $scope.New, function (response) {
                        $scope.message = '';
                        if (response.status == 200) {
                            $scope.messageclass = "alert alert-success";
                        }
                        $scope.message = response.message;
                        $scope.$emit('RTN_EVENT', true);
                        $scope.dataLoading = false;
                        $scope.New = {};
                        $scope.cancel();
                    })
                }
            }
            else {
                httpService.updatedata('api/ItemReturn', $scope.New, function (response) {
                    $scope.message = '';
                    if (response.status == 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.message = response.message;
                    $scope.$emit('RTN_EVENT', true);
                    $scope.dataLoading = false;
                    $scope.New = {};
                    $scope.cancel();
                })
            }
        };

        $scope.validate = function (item) {
            if (item.ReturnNoteNo == undefined || item.ReturnNoteNo == 0) {
                alert('Invalid Return Note No');
                return false;
            }
            if (item.ReceiveNoteId == undefined || item.ReceiveNoteId == 0) {
                alert('Invalid Received Note No');
                return false;
            }
            return true;
        }


    });

    app.controller('updateReturnItemController', function ($http, $scope, $rootScope, $route, $modal, $modalInstance, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.messageclass = "";
        $scope.message = "";
        $scope.selectedItem = item;
        $scope.returnitems = [];
        $scope.recordCount = 0;
        $scope.itemcolumns = [
       { text: "Id", predicate: "Id", sortable: true, dataType: "number", hide: true },
       { text: "Item Code", predicate: "ItemCode", sortable: false, hide: false },
       { text: "Item Name", predicate: "ItemName", sortable: false, hide: false },
       { text: "Description", predicate: "ShortDescription", sortable: false, hide: false },
       { text: "UOM", predicate: "UomName", sortable: true, hide: false },
       { text: "Return Qty", predicate: "ReturnQty", sortable: true, hide: false },
       { text: "Price", predicate: "Price", sortable: true, hide: false },
       { text: "By", predicate: "CreateUser", sortable: true, hide: false },
       { text: "Return Date", predicate: "CreateDate", sortable: true, hide: false },
       { text: "Reason", predicate: "Reason", sortable: true, hide: false },
        ];

        $rootScope.$on('RTNI_EVENT', function (event, data) {
            $scope.messageclass = "alert alert-success";
            $scope.message = "Successfully" + data;
            getItems($scope.selectedItem.Id);
        });


        getItems($scope.selectedItem.Id);

        function getItems(id) {
            httpService.getdata('api/ItemReturn/GetReturnNoteItemsById/' + id, function (results) {
                $scope.returnitems = results.data;
                $scope.recordCount = results.data.length;
            });
        };

        $scope.openReturnItemWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/ItemReturn/Items.html' + '?ver=123',
                controller: 'returnItemsController',
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
                httpService.updatedata('api/ItemReturnAddItem', item, function (response) {
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

    app.controller('returnItemsController', function ($http, $scope, $rootScope, $modal, $modalInstance, $route, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        $scope.loadingdata = false;
        var loadingDelay = globalService.getDataLoadingDelay();
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
       { text: "Received Qty", predicate: "ReceivedQty", sortable: true, hide: false },
       { text: "Price", predicate: "ReceivedPrice", sortable: true, hide: false },
       { text: "Available Stock Qty", predicate: "StockQty", sortable: true, hide: false },
        ];
        $scope.checkstock = function (qty) {
            if (qty > 0) {
                return false;
            }
            return true;
        }

        $rootScope.$on('RTNI_EVENT', function (event, data) {
            $scope.messageclass = "alert alert-success";
            $scope.message = "Successfully Returned Item : " + data;
            getItems($scope.selectedItem.ReceiveNoteId);
        });

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

        getItems($scope.selectedItem.ReceiveNoteId);

        function getItems(id) {
            $scope.loadingdata = true;
            httpService.getdata('api/ItemReturn/GetReceivedItems/' + id, function (results) {
                $scope.stockitems = results.data;
                $scope.recordCount = results.data.length;
                $scope.loadingdata = false;
            });
        };

        $scope.openAddItemWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/ItemReturn/ReturnItem.html' + '?ver=123',
                controller: 'addItemsToReturnNoteController',
                windowClass: 'app-modal-xxsm-window',
                resolve: {
                    item: function () {
                        return p;
                    },
                    returnnote: function () {
                        return $scope.selectedItem.Id;
                    }
                }
            });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

    });

    app.controller('addItemsToReturnNoteController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, $timeout, item, returnnote, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        $scope.searchstr = '';
        $scope.loadingdata = false;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.selectedItem = item;
        $scope.New = {
            'Id': item.Id,
            'ItemId': item.ItemId,
            'ItemCode': item.ItemCode,
            'UomName': item.UomName,
            'StockItemId': item.StockItemId,
            'StockQty': item.StockQty,
            'ReturnQty': item.StockQty,
            'Reason': '',
            'StockId': item.StockId,
            'ReturnNoteId': returnnote
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };



        $scope.additem = function () {
            if ($scope.validate($scope.New)) {
                globalService.openLoadingWindow('Wait..');
                httpService.savedata('api/ItemReturnAddItem', $scope.New, function (response) {
                    $scope.message = '';
                    if (response.status == 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.message = response.message;
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.$emit('RTNI_EVENT', " Add to Return " + $scope.New.ItemCode + '-' + $scope.New.ReturnQty + ' ' + $scope.New.UomName);
                    $scope.dataLoading = false;
                    $scope.cancel();
                })
            }
        };

        $scope.validate = function (item) {
            if (item.ReturnQty == undefined || item.ReturnQty == 0 || item.ReturnQty > item.StockQty) {
                alert('Invalid Return Qty');
                return false;
            }

            return true;
        }

    });

    app.controller('returnNoteEmailHistoryController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.emaildata = [];

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
        getEmailHistory(item.ReferenceId);

        function getEmailHistory(poid) {
            httpService.getdata('api/POSendMail/GetPOEmailHistory/' + poid, function (results) {
                $scope.emaildata = results.data;
            });

        };
    });

    app.controller('retutnNoteSendMailController', function ($http, $scope, $rootScope, $modal, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;
        $scope.disablestatus = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.Email = {
                'ReferenceId': item.Id,
                'To': item.SupplierEmail,
                'From': 'continentaltea@blendandexport.com',
                'Subject': 'Supplier Returns: ' + item.ReturnNoteNo, 'Message': 'Plese refer the attached Return Note for deliveries.', 'CC': '',
                'IsAttachment': true
            };
            $scope.boxnodisabled = false;
        }


        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        (function GetSupplier() {
            httpService.getdata('api/Supplier/GetSupplier/' + item.SupplierId, function (results) {
                $scope.supplier = results.data;
                $scope.Email.To = $scope.supplier.Email;
            });
        })();

        $scope.setAttachment = function (status) {
            if (status === true) {
                $scope.Email.IsAttachment = true;
            }
        }
        $scope.send = function () {
            if (validate($scope.Email)) {
                $scope.disablestatus = true;
                globalService.openLoadingWindow('Sending..');
                httpService.savedata('api/ItemReturnSendMail', $scope.Email, function (response) {
                    alert(response.message);
                    $scope.disablestatus = false;
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.$emit('CLOSE_POPUP', true);
                })

            }
        };

        var validate = function (po) {
            if (po.To == '' || po.To == undefined) {
                alert('Invalid Receipient');
                return false
            }
            if (po.Subject == '' || po.Subject == undefined) {
                alert('Invalid Subject');
                return false
            }
            if (po.Message == '' || po.Message == undefined) {
                alert('Invalid Message');
                return false
            }
            return true;
        };

        $scope.viewHistory = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReturn/EmailHistory.html',
                controller: 'retutnNoteEmailHistoryController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

    });

    app.controller('retutnNoteEmailHistoryController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.emaildata = [];

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
        getEmailHistory(item.ReferenceId);

        function getEmailHistory(id) {
            httpService.getdata('api/ItemReturnSendMail/GetEmailHistory/' + id, function (results) {
                $scope.emaildata = results.data;
            });

        };
    });

    app.controller('rtnOptionsController', function ($http, $scope, $rootScope, $modalInstance,$modal, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.emaildata = [];

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
        getEmailHistory(item.ReferenceId);

        function getEmailHistory(id) {
            httpService.getdata('api/ItemReturnSendMail/GetEmailHistory/' + id, function (results) {
                $scope.emaildata = results.data;
            });

        };

        $scope.print = function () {
            var response = {};
            globalService.openLoadingWindow('Wait..');
            $http.post('api/ReturnNotePrint', item, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.dataLoading = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report')
                $scope.dataLoading = false;
                $scope.$emit('LOAD_DATA_EVENT', true);
            });

        };

        $scope.email = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReturn/Email.html',
                controller: 'retutnNoteSendMailController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return item;
                    }
                }
            });

        };

    });

})();