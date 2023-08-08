(function () {
    var urlTeaCollection = 'api/TeaCollection';
    var urlTeaLocationCollection = 'api/TeaLocation';


    app.controller('straightLineCollectionController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout,
        httpService, globalService) {

        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.searchbylot = "";
        $scope.searchbyinv = "";
        $scope.content = null;
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('LINECOLLECTION_EVENT', function (event, data) {
            getStraightLines('all');
        });

        $rootScope.$on('CREATE_COLLECTION_EVENT', function (event, data) {
            getCollectionNote();
            getCollectionNotes('all');
        });

        $scope.searchlines = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                var serchBox = $scope.searchstr === '' ? "all" : $scope.searchstr;
                var serchLot = $scope.searchbylot === '' ? "all" : $scope.searchbylot;
                var serchInv = $scope.searchbyinv === '' ? "all" : $scope.searchbyinv;
                getStraightLines(serchBox, serchLot, serchInv);
                _timeout = null;

            }, loadingDelay);

        };

        getCollectionNote();
        function getCollectionNote() {

            httpService.getdata(urlTeaCollection, function (results) {
                $scope.CollectionNote = results.data;
                $('#datasearch').focus();
            });
        }

        getCollectionNotes('all');
        function getCollectionNotes(id) {

            httpService.getdata(urlTeaCollection + '/GetCollectionNotes/' + id, function (results) {
                angular.forEach(results.data, function (value, key) {
                    $scope.addItem(value['CollectionNo']);
                })
            });
        }
        $scope.CollectionNotes = [];

        $scope.addItem = function (item) {
            var ret = $scope.CollectionNotes.indexOf(item);
            if (ret == -1) {
                $scope.CollectionNotes.push(item);
            }
            $scope.item = {};
        }

        getStraightLines('all', 'all', 'all');

        function getStraightLines(boxno, lotno, invno) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var seachline = globalService.replaceSlash(boxno);
            var seachlot = globalService.replaceSlash(lotno);
            var seachinv = globalService.replaceSlash(invno);

            httpService.getdata(urlTeaCollection + '/GetStraightLineByBoxLotAndInv?id=' + seachline + '&lot=' + lotno + '&inv=' + invno, function (results) {
                $scope.StraightLines = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });
        };

        $scope.create = function () {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Collection/CreateNote.html',
                controller: 'createNoteController',
                windowClass: 'app-modal-xsm-window',

            });

        };

        $scope.openCollectionWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Collection/MarkCollectionNote.html',
                controller: 'markCollectionController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    },
                    notes: function () {
                        return $scope.CollectionNotes;
                    },
                    currentnote: function () {
                        return $scope.CollectionNote
                    }
                }
            });

        };

        $scope.openLocationWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Collection/MarkStoreLocation.html',
                controller: 'markLocationController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };


        $scope.openPrintWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Collection/PrintCollectionNote.html',
                controller: 'printCollectionNoteController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return $scope.CollectionNotes;
                    }
                }
            });

        };
    });

    app.controller('createNoteController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.create = function () {
            $scope.dataLoading = true;
            httpService.savedata(urlTeaCollection, null, function (response) {
                //var status = response.status;
                alert(response.message);
                $scope.$emit('CREATE_COLLECTION_EVENT', true);
                $scope.dataLoading = false;
                $scope.cancel();
            })
        }

    });

    app.controller('printCollectionNoteController', function ($http, $scope, $rootScope, $modalInstance,$route, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.CollectionNotes = item;
        $scope.SelectedCollectioNoteNo = 0;
        $scope.IsExcelFormat = false;
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.print = function () {
            $scope.dataLoading = true;
            //httpService.getdata('api/PrintCollectionNote/GetCollectionNotePrint?Id=' + $scope.SelectedCollectioNoteNo, function (response) {
            //    //var status = response.status;
            //    alert(response.message);
            //    $scope.$emit('CREATE_COLLECTION_EVENT', true);
            //    $scope.dataLoading = false;
            //    $scope.cancel();
            //})
            $http.get('api/PrintCollectionNote/GetCollectionNotePrint?Id=' + $scope.SelectedCollectioNoteNo + '&download=' + $scope.IsExcelFormat, { responseType: 'arraybuffer' }).success(function (data) {
                var doctype = 'application/pdf';
                if ($scope.IsExcelFormat === true) {
                    doctype = 'application/vnd.ms-excel'
                }
                var file = new Blob([data], { type: doctype });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                //var file = new Blob([data], { type: 'application/pdf' });
                //var fileURL = URL.createObjectURL(file);
                //window.open(fileURL);
                $scope.dataLoading = false;

            }).error(function (data) {
                alert('Error in processing report.Contact Administrator.')
                $scope.dataLoading = false;
            });
        }

    });

    app.controller('markCollectionController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, notes, currentnote, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.CollectionNotes = notes;
        $scope.SelectedBoxNo = item;
        $scope.CurrentNote = currentnote;
        $scope.SelectedCollectioNoteNo = 0;

        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.stockreceived = true;
        $scope.currentcollection = true;
        $scope.New = {
            'StockReceived': true, 'CollectionNoteNo': $scope.CurrentNote.CollectionNo,
            'CollectionNoteId': 0
            , 'CollectionDate': initSaleDate, 'PurchaseId': $scope.SelectedBoxNo.PurchaseId
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        function SetMark() {
            if (!$scope.stockreceived) {
                $scope.New.StockReceived = false;
                $scope.New.CollectionNoteId = 0
            }
            if (!$scope.currentcollection)
                $scope.New.CollectionNoteNo = $scope.SelectedCollectioNoteNo;

        }

        $scope.create = function () {
            $scope.dataLoading = true;
            SetMark();
            $scope.New.CollectionDate = globalService.convertDateToIST($scope.New.CollectionDate);
            httpService.updatedata(urlTeaCollection, $scope.New, function (response) {
                alert(response.message);
                $scope.$emit('LINECOLLECTION_EVENT', true);
                $scope.dataLoading = false;
                $scope.cancel();
            })
        }

    });

    app.controller('markLocationController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.SelectedBoxNo = item;

        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.New = {
            'StoreLocation': $scope.SelectedBoxNo.StoreLocation, 'PurchaseId': $scope.SelectedBoxNo.PurchaseId
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };



        $scope.create = function () {
            $scope.dataLoading = true;
            //SetMark();
            httpService.savedata(urlTeaLocationCollection, $scope.New, function (response) {
                //var status = response.status;
                alert(response.message);
                $scope.$emit('LINECOLLECTION_EVENT', true);
                $scope.dataLoading = false;
                $scope.cancel();
            })
        }

    });

})();