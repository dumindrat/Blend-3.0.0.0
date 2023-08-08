(function () {
    var urlBlendClose = 'api/BlendClose';
    var urlBlendCloseCancel = 'api/BlendCloseReversal';

    app.controller('blenCloseDetailController', function ($http, $scope, $rootScope, $modal, $route, $timeout, httpService, globalService) {
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.searchstr = "";
        $scope.recordCount = globalService.getDataLoadingMessage();

        getBlendList('all');

        function getBlendList(searchstring) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var blendNo = globalService.replaceSlash(searchstring);
            httpService.getdata(urlBlendClose + '/GetBlends/' + blendNo, function (results) {
                $scope.BlendData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
            });
        };

        $rootScope.$on('BLENDCLOSE_EVENT', function (event, data) {
            getBlendList('all');
            //$scope.openMoreOptionsView(data);

        });

        $rootScope.$on('BLENDCONVERT_EVENT', function (event, data) {
            getBlendList('all');
        });

        $rootScope.$on('BLENDCANCEL_EVENT', function (event, data) {
            getBlendList('all');
        });


        $scope.searchblend = function searchblend() {
            if (_timeout) { // if there is already a timeout in process cancel it
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getBlendList($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;
            }, loadingDelay);
        };

        $scope.CheckCloseStatus = function (obj) {
            if (obj.IsClosed === true) {
                return true;
            }
            return false;
        }

        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to delete?");
            if (retVal == true) {
                httpService.deletedata(urlBlenCreation + '/' + id, function () {
                    alert('Data deleted successfully');
                    getBlendList("all");
                })
            }
        };

        $scope.blenddatacolumns = [
                { text: "Id", predicate: "Id", hide: true, sortable: true, dataType: "number" },
                { text: "BlendId", predicate: "BlendId", hide: true, sortable: true, dataType: "number" },
                { text: "BlendNo", predicate: "BlendNo", hide: false, sortable: true },
                { text: "ContractId", predicate: "ContractId", hide: true, sortable: true },
                { text: "ContractNo", predicate: "ContractNo", hide: false, sortable: true },
                { text: "ContractNo", predicate: "ContractNo", hide: false, sortable: true },
                { text: "StandardId", predicate: "StandardId", hide: true, sortable: true },
                { text: "StandardName", predicate: "StandardName", hide: false, sortable: true },
                { text: "ExportQty", predicate: "ExportQty", hide: false, sortable: true },
                { text: "LineQty", predicate: "LineQty", hide: false, sortable: true },
                { text: "Rate", predicate: "Rate", hide: false, sortable: true },
                { text: "CloseStatus", predicate: "CloseStatus", hide: false, sortable: true },
                { text: "IsClosed", predicate: "IsClosed", hide: true, sortable: true },
                { text: "IsCancelRequest", predicate: "IsCancelRequest", hide: false, sortable: true },
                { text: "RowVersion", predicate: "RowVersion", hide: true, sortable: true },
                { text: "Action", predicate: "Action", hide: false, sortable: false }
        ];


        $scope.CancelConfirm = function (p) {
            $scope.message = "";
            if (Validate(p)) {
                $scope.openCancelWindow(p);
            }
        }

        var Validate = function (p) {
            if (p.IsCancelRequest == 1) {
                alert('Already sent a Cancel Request');
                return false
            }
            return true;
        }

        $scope.openCancelWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: 'App/Tea/BlendClose/BlendCloseCancel.html',
                controller: 'blendCloseController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                    //, box: function () {
                    //    return $scope.SelectedBoxNo;
                    //}
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

        $scope.setScope = function (obj, action) {
            $scope.action = action;
            $scope.getStandard(obj.ContractId);
            $scope.boxnodisabled = true;
            $scope.buttonstatus = "Update";
            $scope.New = obj;
        };

        $scope.searchblend = function searchblend() {
            getBlendList($scope.searchstr === '' ? "all" : $scope.searchstr);
        };

        $scope.openCloseView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendClose/BlendClose.html',
                controller: 'blendCloseController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {

            });
        };

        $scope.openMoreOptionsView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendClose/MoreOptions.html',
                controller: 'blendCloseOptionsController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('blendCloseController', function ($scope, $rootScope, $modal, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var systemDate = globalService.getDate();
        var asatdate = globalService.convertDateyyyymmdd(systemDate);

        $scope.SelectedBlend = {};
        $scope.SelectedBlend.Id = item.Id;
        $scope.SelectedBlend.StandardName = item.StandardName;
        $scope.SelectedBlend.ContractNo = item.ContractNo;
        $scope.SelectedBlend.BlendNo = item.BlendNo;
        $scope.SelectedBlend.BlendId = item.BlendId;
        $scope.SelectedBlend.Rate = item.Rate;
        $scope.SelectedBlend.LineQty = item.LineQty;
        $scope.SelectedBlend.ExportQty = item.ExportQty;

        $scope.SelectedBlend.BG = 0;
        $scope.SelectedBlend.BL = 0;
        $scope.SelectedBlend.STDBB = 0;
        $scope.SelectedBlend.STDBG = 0;
        $scope.SelectedBlend.SFTBB = 0;
        $scope.SelectedBlend.SFTBG = 0;
        $scope.SelectedBlend.CloseDate = asatdate;//$filter('date')(globalService.getDate(), 'yyyy-MM-dd');
        $scope.SelectedBlend.PhysicalQty = 0;
        $scope.SelectedBlend.SiftedQty = 0;
        $scope.SelectedBlend.RowVersion = item.RowVersion;
        $scope.SelectedBlend.FinalBlendQty = 0;//For popup
        $scope.SelectedBlend.SiftedEntryQty = 0//For popup

        $scope.siftedqtyerror = false;
        $scope.siftedqtyerrorclss = '';
        $scope.physicalqtyerror = false;
        $scope.physicalqtyerrorclss = ''; //glyphicon glyphicon-remove-sign
        $scope.showLoader = false;
        $scope.message = "";
        $scope.messageclass = "";

        CalculateBB();

        function CalculateBB() {
            $scope.SelectedBlend.BB = 0;
            if ($scope.SelectedBlend.ExportQty <= $scope.SelectedBlend.LineQty) {
                $scope.SelectedBlend.BB = $scope.SelectedBlend.LineQty - $scope.SelectedBlend.ExportQty;// - $scope.New.ReducedQtyR;
                $scope.SelectedBlend.STDBB = $scope.SelectedBlend.BB;

            }
        };

        function CalculateSTDQty(value) {
            var qty = value;
            $scope.SelectedBlend.STDBG = 0;
            $scope.SelectedBlend.SFTBG = 0;
            $scope.SelectedBlend.SFTBB = 0;
            $scope.SelectedBlend.STDBB = $scope.SelectedBlend.BB;

            //$scope.SelectedBlend.SiftedQty = 0;
            $scope.physicalqtyerror = true;
            if (angular.isUndefined(value) | isNaN(value)) {
                angular.element(document.getElementById('btnSubmit'))[0].disabled = true;
                setErrorIconForPhysicalQty();
            }
            if (value >= $scope.SelectedBlend.LineQty) {
                $scope.SelectedBlend.BG = value - ($scope.SelectedBlend.LineQty < $scope.SelectedBlend.ExportQty ? $scope.SelectedBlend.ExportQty : $scope.SelectedBlend.LineQty);
                $scope.SelectedBlend.STDBG = $scope.SelectedBlend.BG;
                $scope.SelectedBlend.BL = 0;
                $scope.isValidPhysicalQty = true;
                setOkIconForPhysicalQty();
            }
            if (value < $scope.SelectedBlend.LineQty && value >= $scope.SelectedBlend.ExportQty) {
                $scope.SelectedBlend.BL = ($scope.SelectedBlend.LineQty - value) > 0 ? ($scope.SelectedBlend.LineQty - value) : 0;
                $scope.SelectedBlend.BB = value - $scope.SelectedBlend.ExportQty;
                $scope.SelectedBlend.STDBB = value - $scope.SelectedBlend.ExportQty;
                $scope.SelectedBlend.BG = 0;
                $scope.isValidPhysicalQty = true;
                setOkIconForPhysicalQty();

            }
            if (value < $scope.SelectedBlend.ExportQty) {
                angular.element(document.getElementById('btnSubmit'))[0].disabled = true;
                $scope.isValidPhysicalQty = false;
                setErrorIconForPhysicalQty();
            }
            $scope.SelectedBlend.BB = $scope.SelectedBlend.STDBB;
        };

        function CalculateSFTQty(value) {
            if (angular.isUndefined(value) | isNaN(value)) {
                angular.element(document.getElementById('btnSubmit'))[0].disabled = true;
                setErrorIconForSiftedQty();
            }
            if (value <= $scope.SelectedBlend.BG && $scope.isValidPhysicalQty) {
                var difstdbg = $scope.SelectedBlend.BG - value;// $scope.SelectedBlend.PhysicalQty - $scope.SelectedBlend.LineQty - value;
                $scope.SelectedBlend.SFTBG = value;
                $scope.SelectedBlend.STDBG = difstdbg > 0 ? difstdbg : 0;
                $scope.SelectedBlend.STDBB = $scope.SelectedBlend.BB;
                $scope.SelectedBlend.SFTBB = 0;
                angular.element(document.getElementById('btnSubmit'))[0].disabled = false;
                setOkIconForSiftedQty();
            }
            if (value > $scope.SelectedBlend.BG && value <= $scope.SelectedBlend.BG + $scope.SelectedBlend.BB && $scope.isValidPhysicalQty) {
                var diff = $scope.SelectedBlend.PhysicalQty - $scope.SelectedBlend.LineQty;
                $scope.SelectedBlend.SFTBG = diff > 0 ? diff : 0;
                $scope.SelectedBlend.SFTBB = value - $scope.SelectedBlend.BG;
                $scope.SelectedBlend.STDBG = 0;
                $scope.SelectedBlend.STDBB = ($scope.SelectedBlend.STDBB - $scope.SelectedBlend.SFTBB) > 0 ? ($scope.SelectedBlend.STDBB - $scope.SelectedBlend.SFTBB) : 0;
                $scope.SelectedBlend.BB = $scope.SelectedBlend.STDBB;

                angular.element(document.getElementById('btnSubmit'))[0].disabled = false;
                setOkIconForSiftedQty();

            }
            if (value > $scope.SelectedBlend.BG + $scope.SelectedBlend.BB) {
                //Unsuccessful Blend
                angular.element(document.getElementById('btnSubmit'))[0].disabled = true;
                setErrorIconForSiftedQty();
            }
            $scope.SelectedBlend.BB = $scope.SelectedBlend.STDBB;

        };

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        function setOkIconForPhysicalQty() {
            $scope.physicalqtyerror = false;
            $scope.physicalqtyerrorclss = 'glyphicon glyphicon-ok-sign ok-iconcolor'
            $scope.message = "";
            $scope.messageclass = "alert alert-info";
        };

        function setErrorIconForPhysicalQty() {
            $scope.physicalqtyerror = false;
            $scope.physicalqtyerrorclss = 'glyphicon glyphicon-remove-sign error-iconcolor'
            $scope.message = "Add valid Closed Blend Balance to proceed";
            $scope.messageclass = "alert alert-danger";
        };

        function setOkIconForSiftedQty() {
            $scope.siftedqtyerror = false;
            $scope.siftedqtyerrorclss = 'glyphicon glyphicon-ok-sign ok-iconcolor'
            $scope.message = "";
            $scope.messageclass = "alert alert-info";
        };

        function setErrorIconForSiftedQty() {
            $scope.siftedqtyerror = false;
            $scope.siftedqtyerrorclss = 'glyphicon glyphicon-remove-sign error-iconcolor'
            $scope.message = "Error in Sifted tea Qty";
            $scope.messageclass = "alert alert-danger";
        };

        $scope.focusFunc = function (keyEvent, elementId) {
            if (keyEvent.which === 13)
                angular.element('#' + elementId).focus()
        }

        $scope.isValidPhysicalQty = false;

        $scope.$watch('SelectedBlend.PhysicalQty', function (value) {
            angular.element(document.getElementById('btnSubmit'))[0].disabled = false;
            CalculateBB();
            CalculateSTDQty(value);

            var siftedQty = $scope.SelectedBlend.SiftedQty
            if (siftedQty > 0)
                CalculateSFTQty(siftedQty);



        });

        $scope.$watch('SelectedBlend.SiftedQty', function (value) {
            var physicalQty = $scope.SelectedBlend.PhysicalQty;
            CalculateSTDQty(physicalQty);

            $scope.siftedqtyerror = true;
            if ($scope.SelectedBlend.SiftedQty > 0)
                CalculateSFTQty(value);


        });

        $scope.Validation = function () {
            if ($scope.SelectedBlend.PhysicalQty < $scope.SelectedBlend.ExportQty) {
                alert('Unsuccessfull Blend');
                return false;
            }
            if ($scope.SelectedBlend.PhysicalQty === 0) {
                alert('Invalid Physical Qty');
                return false;
            }
            if ($scope.SelectedBlend.CloseDate === null) {
                alert('Invalid Closing Date');
                return false;
            }
            return true;
        };

        $scope.save = function () {
            if ($scope.Validation()) {
                $scope.showLoader = true;
                $scope.SelectedBlend.CloseDate = globalService.convertDateToIST($scope.SelectedBlend.CloseDate);

                httpService.savedata(urlBlendClose, $scope.SelectedBlend, function (response) {
                    if (response.status === 200) {
                        alert('Successfully closed. Create a Shipment to dispose the Export Qty');
                        $scope.$emit('BLENDCLOSE_EVENT', $scope.SelectedBlend);
                        $scope.cancel();
                        $scope.openMoreOptionsView($scope.SelectedBlend);
                    } else {
                        $scope.error = response.message;
                    }
                    $scope.showLoader = false;
                });
            };
        };

        $scope.CancelRequest = function () {
            var retVal = confirm("Are you sure want to Reverse this Transaction ?");
            if (retVal == true) {
                globalService.openLoadingWindow('Saving..');

                var cancelObj = { 'Id': $scope.SelectedBlend.Id, 'BlendId': $scope.SelectedBlend.BlendId, 'Reason': $scope.confirmReason, 'RowVersion': $scope.SelectedBlend.RowVersion };
                httpService.savedata(urlBlendCloseCancel, cancelObj, function (response) {
                    if (response.status === 200) {
                        alert('Success');
                        $scope.$emit('BLENDCANCEL_EVENT', true);
                        $scope.cancel();

                    } else {
                        alert(response.message);
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }
        }

        //Added lines
        $scope.openAddCloseAmount = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendClose/AddCloseAmount.html',
                controller: 'blendCloseAddRemainBalController',
                windowClass: 'app-modal-xxsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            //modalInstance.result.then(function (selectedObject) {
            //    //alert(selectedObject.FinalBlendQty);
            //});
        };

        $scope.openSiftedAmount = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendClose/AddSiftedAmount.html',
                controller: 'blendCloseSiftedController',
                windowClass: 'app-modal-xxsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
        };

        $rootScope.$on('ADD_REMAIN_BAL', function (event, data) {
            $scope.SelectedBlend.PhysicalQty = $scope.SelectedBlend.ExportQty + data;
        });
        $rootScope.$on('ADD_SIFTED_QTY', function (event, data) {
            $scope.SelectedBlend.SiftedQty = data;
        });

        $scope.openMoreOptionsView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendClose/MoreOptions.html',
                controller: 'blendCloseOptionsController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('blendCloseAddRemainBalController', function ($scope, $rootScope, $modal, $modalInstance, item) {
        $scope.message = "";
        $scope.messageclass = "";

        $scope.New = { 'FinalBlendQty': 0 };
        $scope.New.FinalBlendQty = item.FinalBlendQty;

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.add = function () {
            if ($scope.New.FinalBlendQty != undefined) {
                $scope.message = "";
                $scope.messageclass = "alert alert-success";
                $scope.$emit('ADD_REMAIN_BAL', $scope.New.FinalBlendQty);
                $modalInstance.dismiss('Close');
            }
            else {
                $scope.message = "Error in Final BlendQty";
                $scope.messageclass = "alert alert-danger";
            }
        };
    });

    app.controller('blendCloseSiftedController', function ($scope, $rootScope, $modal, $modalInstance, item) {
        $scope.message = "";
        $scope.messageclass = "";
        $scope.New = { 'SiftedEntryQty': 0 };
        $scope.New.SiftedEntryQty = item.SiftedEntryQty;

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.add = function () {
            if ($scope.New.SiftedEntryQty != undefined) {
                $scope.message = "";
                $scope.messageclass = "alert alert-success";
                $scope.$emit('ADD_SIFTED_QTY', $scope.New.SiftedEntryQty);
                $modalInstance.dismiss('Close');
            }
            else {
                $scope.message = "Error in Final SiftedQty";
                $scope.messageclass = "alert alert-danger";
            }
        };
    });

    app.controller('blendCloseOptionsController', function ($scope, $rootScope, $modal, $modalInstance, item, httpService, globalService) {
        $scope.accessLevelsShipment = globalService.getaccess('/ShipTea');

        $scope.BlendInfo = [];
        $scope.Blend = {};
        $scope.showLoader = false;
        $scope.SelectedBlend = item;

        getBlendClose($scope.SelectedBlend.BlendId);
        function getBlendClose(blendId) {
            var info = { 'Item': '', 'Value': '' };
            $scope.showLoader = true;
            globalService.openLoadingWindow('Loading..');

            //httpService.getdata('api/BlendGainLossConvert/GetRate/' + blendId, function (results) {
            //    $scope.Blend.ConvertRate = results;

            //});

            httpService.getdata(urlBlendClose + '/GetBlendCloseInfo/' + blendId, function (results) {
                $scope.BlendInfo.push({ 'Item': 'Export Qty', 'Value': results.data.ExportQty });
                $scope.BlendInfo.push({ 'Item': 'Total Blend Qty', 'Value': results.data.LineQty });
                $scope.BlendInfo.push({ 'Item': 'Final Blended Qty', 'Value': results.data.PhysicalQty });
                $scope.BlendInfo.push({ 'Item': 'STDBB', 'Value': results.data.STDBB });
                $scope.BlendInfo.push({ 'Item': 'STDBG', 'Value': results.data.STDBG });
                $scope.BlendInfo.push({ 'Item': 'BG', 'Value': results.data.BG });
                $scope.BlendInfo.push({ 'Item': 'BL', 'Value': results.data.BL });
                $scope.BlendInfo.push({ 'Item': 'Sifted Qty', 'Value': results.data.SiftedQty });
                $scope.BlendInfo.push({ 'Item': 'Rate', 'Value': results.data.Rate });
                $scope.BlendInfo.push({ 'Item': 'Is Closed', 'Value': results.data.IsClosed === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Closed Rate', 'Value': results.data.ClosedRate });
                $scope.BlendInfo.push({ 'Item': 'Close Date', 'Value': results.data.CloseDate });
                $scope.BlendInfo.push({ 'Item': 'Is Shipment done', 'Value': results.data.IsShipment === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is Rate Converted', 'Value': results.data.IsConverted === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is Cancel Request Sent', 'Value': results.data.IsCancelRequest === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is PackGoods Create', 'Value': results.data.IsPackGoodsCreate === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'PackGoods Create Date', 'Value': results.data.PackGoodsCreateDate });

                $scope.showLoader = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

                $scope.Blend.IsConverted = results.data.IsConverted;
                $scope.Blend.BlendId = blendId;
                $scope.Blend.STDBG = results.data.STDBG;
                $scope.Blend.STDBB = results.data.STDBB;
                $scope.Blend.BL = results.data.BL;
                $scope.Blend.BG = results.data.STDBG;
                $scope.Blend.Rate = results.data.Rate;
                $scope.Blend.LineQty = results.data.LineQty;
                $scope.Blend.ConvertedQTy = results.data.STDBG > 0 ? results.data.STDBG : -results.data.BL;

                //$scope.Blend.ConvertRate = (results.data.Rate * results.data.LineQty) / (results.data.LineQty + $scope.Blend.ConvertedQTy);


            });
        };

        //getShipment($scope.SelectedBlend.BlendNo);
        //function getShipment(blendno) {
        //    httpService.getdata(urlBlendClose + '/GetBlendCloseInfo/' + blendno, function (results) {
        //        $scope.BlendShipmentData = results.data;
        //    });
        //}

        //$scope.checkShipmentRights = function () {
        //    debugger;
        //    var access = $scope.accessLevelsShipment.C;
        //    return access;
        //}

        $scope.sethide = function (obj) {
            if ((obj.STDBG > 0 || obj.BL > 0) && obj.IsConverted === false)
                return false;
            if (obj.IsConverted === true) {
                return true;
            }
            return true;
        }


        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        GetNewRate();

        function GetNewRate() {
            var url = 'GetRate';
            httpService.getdata('api/BlendGainLossConvert/' + url + '/' + $scope.SelectedBlend.BlendId, function (results) {
                $scope.ConvertRate = results.data;
                $scope.Blend.ConvertRate = results.data;

            });
        }

        $scope.convert = function (item) {
            var transType = 'BLEND GAIN';
            if (item.BL > 0) {
                transType = 'BLEND LOSS';
            }
            var retVal = confirm('Are you sure want to convert [' + transType + '] to the blend Blend Balance.?');
            if (retVal == true) {
                var obj = {
                    BlendId: $scope.Blend.BlendId,
                    'BB': $scope.Blend.STDBB, 'BG': $scope.Blend.STDBG, 'BL': $scope.Blend.BL,
                    ClosedRate: $scope.Blend.ConvertRate,
                    RowVersion: $scope.SelectedBlend.RowVersion
                };
                httpService.savedata('api/BlendGainLossConvert', obj, function (response) {
                    if (response.status === 200) {
                        alert('Successfully converted');
                        $scope.$emit('BLENDCONVERT_EVENT', true);
                        $scope.BlendInfo = [];
                        getBlendClose($scope.Blend.BlendId);
                    } else {
                        $scope.error = response.message;
                    }
                    $scope.showLoader = false;
                });
            }
        }

        $scope.openShipmentWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Shipments/Shipments.html',
                controller: 'teaShipmentCreateController',
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
    });

    app.controller('packGoodOptionsController', function ($scope, $rootScope, $modal, $modalInstance, item, httpService, globalService) {
        $scope.Blend = {};
        $scope.showLoader = false;
        $scope.SelectedBlend = item;
    });
})();