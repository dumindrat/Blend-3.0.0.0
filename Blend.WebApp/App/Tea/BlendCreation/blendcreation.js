(function () {
    var urlBlenCreation = 'api/Blend';
    var urlBlenAddRemove = 'api/BlendAddRemove';
    var urlDocument = 'api/Document';
    var urlBlendEnableTeaBoard = 'api/BlendEnableTeaBoard';

    app.controller('blenCreationController', function ($http, $scope, $rootScope, $modal, $route, $timeout, $window, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.disablestatus = false;
        $scope.searchstr = "";
        $scope.recordCount = globalService.getDataLoadingMessage();

        $rootScope.$on('BLEND_EVENT', function (event, data) {
            getBlendList('all');
        });



        getBlendList('all');

        function getBlendList(searchstring) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var blendNo = globalService.replaceSlash(searchstring);
            httpService.getdata(urlBlenCreation + '/GetBlends/' + blendNo, function (results) {
                $scope.BlendData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#datasearch').focus();
            });
        };



        $scope.CheckCloseStatus = function (obj) {
            if (obj.IsClosed === true) {
                return true;
            }
            return false;
        }

        $scope.CheckAddedLine = function (obj) {
            if (obj.LineQty > 0) {
                return true;
            }
            return false;
        }

        $scope.CheckEnabledTeaBoard = function (obj) {
            return obj.IsTeaBoardEnabled;
        }

        $scope.GetSetProdButtonStyle = function (obj) {
            if (obj.IsTeaBoardEnabled) {
                return "btn btn-default btn-sm";
            }
            return "btn btn-default btn-sm";
        }

        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to delete?");
            if (retVal == true) {
                httpService.deletedata(urlBlenCreation + '/' + id, function (response) {
                    if (response.status == 200) {
                        alert('Data deleted successfully');
                        getBlendList("all");
                    }
                })
            }
        };



        $scope.print = function (blendId) {
            globalService.openLoadingWindow("Blend Sheet");
            $http.get('api/BlendReports/GetBlendSheet/' + blendId, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report.Contact Administrator.')
                $scope.$emit('LOAD_DATA_EVENT', true);
            });
        };



        $scope.searchblend = function searchblend() {
            if (_timeout) { // if there is already a timeout in process cancel it
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getBlendList($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        //Added lines
        $scope.openViewAddedLines = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/BlendDetail.html',
                controller: 'blendEditController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

        //Added lines
        $scope.openCreateBlendWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/CreateBlend.html',
                controller: 'blendCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

        $scope.openTraceBlendWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/TraceBlend.html',
                controller: 'blendTraceController',
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

        $scope.openStoresInsWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/StoresInstructions.html',
                controller: 'blendStoresInstructionController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.openDownloadWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/BlendDownload.html',
                controller: 'blendDownloadController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.enableTeaBoard = function (blend) {
            var retVal = confirm("Are you sure want to enable " + blend.BlendNo + " for the Production?");
            if (retVal == true) {
                globalService.openLoadingWindow("Enabling..");
                httpService.savedata(urlBlendEnableTeaBoard, blend.BlendId, function (response) {
                    if (response.status === 200) {
                        $scope.$emit('LOAD_DATA_EVENT', true);
                        getBlendList('all');
                    } else {
                        $scope.message = response.message;
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    }
                });
            }
        }

        $scope.disableTeaBoard = function (blend) {
            var retVal = confirm("Are you sure want to enable " + blend.BlendNo + " for the Trials?");
            if (retVal == true) {
                globalService.openLoadingWindow("Enabling..");
                httpService.updatedata(urlBlendEnableTeaBoard, blend.BlendId, function (response) {
                    if (response.status === 200) {
                        $scope.$emit('LOAD_DATA_EVENT', true);
                        getBlendList('all');
                    } else {
                        $scope.message = response.message;
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    }
                });
            }
        }
    });


})();