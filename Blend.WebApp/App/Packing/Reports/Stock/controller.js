(function () {

    app.controller('packingStockController', function ($http, $scope, $route, $location, $modal, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.hideform = false;

        $scope.reportData = [{ 'ReportName': 'Packing Stock', 'ReportType': 1, 'HideAllocationCheck': true },
        ];

        $scope.gridOptions = {
            data: 'reportData',
            showGroupPanel: false,
            rowHeight: globalService.GridRowHeight(),
            columnDefs: [{ field: 'ReportName', displayName: 'Report Name', visible: true, width: '80%' },
                 { field: 'ReportType', displayName: 'ReportType', visible: false, width: '15%' },
                {
                    displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-click="PrintOption(row.entity)" name="view"  value="Print Options"></div>'
                    , width: '20%'
                }
            ]
        };

        $scope.PrintOption = function (p, size) {
            var templateLink = '';// p.ReportType === 1 ? 'App/Tea/Reports/Stock/StraightLineStockReportOptions.html' : 'App/Tea/Reports/Stock/StockReportOptions.html';
            if (p.ReportType === 1)
                templateLink = 'App/Packing/Reports/Stock/StockReportOptions.html';

            var modalInstance = $modal.open({
                templateUrl: templateLink,
                controller: 'packstockReportPrintController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            //modalInstance.result.then(function (selectedObject) {
            //    p.contractname = selectedObject.contractname;
            //});
        };     


    });

    app.controller('packstockReportPrintController', function ($http, $scope, $route, $modalInstance, $location, item, globalService) {
        $scope.New = {
           'IsWithAllocatedLine': false, 'IsExcelFormat': 0,
            'IsStoresSheet': 0, 'IsAvailableLineOnly': false, 'IsAllocatedLineOnly': false
        };

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.DateObj = { 'FromDate': completeDate, 'ToDate': completeDate, 'IsExcelFormat': false };

        $scope.ReportName = item.ReportName;
        $scope.hideallocation = item.HideAllocationCheck;
        $scope.cancel = function () {

            $modalInstance.dismiss('Close');
        };

        $scope.settype_availableonly = function (value) {
            if (value) {
                $scope.New.IsWithAllocatedLine = false;
                $scope.New.IsAllocatedLineOnly = false;

            }
        }
        $scope.settype_withallocate = function (value) {
            $scope.New.IsAvailableLineOnly = false;
            $scope.New.IsAllocatedLineOnly = false;

        }
        $scope.settype_allocateonly = function (value) {
            $scope.New.IsWithAllocatedLine = false;
            $scope.New.IsAvailableLineOnly = false;
        }

        $scope.printlines = function () {
            //SL Stock
            if (item.ReportType === 1) {
                globalService.openLoadingWindow("SL Stock");
                //item.ReportType === 1 ? $scope.New.IsStoresSheet = false : $scope.New.IsStoresSheet = true
                var doctype = $scope.New.IsExcelFormat === true ? 'application/vnd.ms-excel' : 'application/pdf';
                $http.post('api/PackingStock', $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
                    var file = new Blob([data], { type: doctype });
                    var fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                    $scope.$emit('LOAD_DATA_EVENT', true);

                }).error(function (data) {
                    alert('Error in processing report.Contact Administrator.')
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }
           
        };


    });

    //app.controller('ageAnalysisStockReportPrintController', function ($http, $scope, $route, $modalInstance, $location, item, globalService) {
    //    $scope.ReportName = "Age Analysis";
    //    var systemDate = globalService.getDate();
    //    var asatdate = globalService.convertDateyyyymmdd(systemDate);
    //    $scope.New = { 'IsExcelFormat': 0, 'IsWithValues': 0, 'Grade': '', 'Standard': '', 'AsAtDate': asatdate };

    //    $scope.cancel = function () {

    //        $modalInstance.dismiss('Close');
    //    };

    //    $scope.printlines = function () {

    //        globalService.openLoadingWindow("Age Analysis Stock");
    //        $scope.New.AsAtDate = globalService.convertDateToIST($scope.New.AsAtDate);
    //        var doctype = $scope.New.IsExcelFormat === true ? 'application/vnd.ms-excel' : 'application/pdf';
    //        $http.post('api/AgeAnalysis', $scope.New, { responseType: 'arraybuffer' }).success(function (data) {
    //            var file = new Blob([data], { type: doctype });
    //            var fileURL = URL.createObjectURL(file);
    //            window.open(fileURL);
    //            $scope.$emit('LOAD_DATA_EVENT', true);

    //        }).error(function (data) {
    //            alert('Error in processing report.Contact Administrator.')
    //            $scope.$emit('LOAD_DATA_EVENT', true);

    //        });
    //    }


    //});

})();