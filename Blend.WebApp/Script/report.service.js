(function () {
    var response = { data: '', status: '', message: 'Success', headers: '' };

    app.factory('reportService', function ($http, $location, $modal, globalService) {

        var service = {};

        service.postdownload = function (url, obj, callback) {
            $http.post(url, obj, { responseType: 'arraybuffer' })
				.success(function (data, status, headers, config) {
				    var file = new Blob([data], { type: 'application/vnd.ms-excel' });
				    var fileURL = URL.createObjectURL(file);
				    window.open(fileURL);
				    response.status = status;
				    response.data = data;
				    response.headers = headers;
				    callback(response);
				})
            .error(function (data, status, headers, config) {
                response.status = status;
                var message = globalService.gethttpmessage(status)
                alert(message);
                callback(response);

                if (status === 401) {
                    globalService.RequestAuthentication();
                    $location.path('/Login');
                }
            });
        };

        service.getdownload = function (url, callback) {
            $http.post(url, obj, { responseType: 'arraybuffer' })
				.success(function (data, status, headers, config) {
				    var file = new Blob([data], { type: 'application/vnd.ms-excel' });
				    var fileURL = URL.createObjectURL(file);
				    window.open(fileURL);
				    response.status = status;
				    response.data = data;
				    response.headers = headers;
				    callback(response);
				})
            .error(function (data, status, headers, config) {
                response.status = status;
                var message = globalService.gethttpmessage(status)
                alert(message);
                callback(response);

                if (status === 401) {
                    globalService.RequestAuthentication();
                    $location.path('/Login');
                }
            });
        };

        service.postviewreport = function (obj, callback) {

            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/ReportView.html',
                controller: 'postReportViewController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return obj;
                    }
                }

            });
        }

        service.getviewreport = function (obj, callback) {

            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Reports/ReportView.html',
                controller: 'getReportViewController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return obj;
                    }
                }

            });
        }

        return service;

    });
})();