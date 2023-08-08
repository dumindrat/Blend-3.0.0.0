(function () {
    var urlDocument = 'api/Document';
    app.controller('resetDocumetController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout, $window, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        $scope.documentheader = [
                { text: "DocType", predicate: "DocType", sortable: true, dataType: "number", hide: true },
                 { text: "Document Type", predicate: "Description", sortable: true, dataType: "number", hide: false },
                 { text: "Current DocNo", predicate: "DocNo", sortable: true, dataType: "number", hide: false },
                 { text: "Prefix", predicate: "Description", sortable: true, dataType: "number", hide: false },
                 { text: "Format", predicate: "Format", sortable: true, dataType: "text", hide: false },
        { text: "", predicate: "", sortable: true, dataType: "text", hide: false }

        ];

        $rootScope.$on('DOCR_ESET_EVENT', function (event, data) {
            getDocuments();
        });

        getDocuments();

        function getDocuments() {
            globalService.openLoadingWindow('Loading..');
            httpService.getdata(urlDocument + '/GetDocumentNosByModule/' + $rootScope.moduleId, function (results) {
                $scope.DocumentData = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        $scope.Reset = function (doc) {
            var retVal = confirm("Are you sure want to Reset this Document No ?");
            if (retVal == true) {
                if (validation(doc)) {
                    globalService.openLoadingWindow('Saving..');
                    httpService.updatedata(urlDocument, doc, function (response) {
                        alert(response.message);
                        $scope.$emit('DOCR_ESET_EVENT', true);
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    });
                }
            }
        }

        var validation = function (doc) {
            if (doc.DocNo === '' || doc.DocNo === undefined) {
                alert('Invalid Doc No');
                return false;
            }
            if (doc.Prefix === '' || doc.Prefix === undefined) {
                alert('Invalid Prefix');
                return false;
            }
            if (doc.DocFormat === '' || doc.DocFormat === undefined) {
                alert('Invalid Doc Format');
                return false;
            }
            return true;
        }
    });
})();