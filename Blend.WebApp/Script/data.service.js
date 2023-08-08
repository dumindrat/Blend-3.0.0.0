(function () {
    var response = { data: '', status: '', message: 'Success', headers: '' };

    app.factory('httpService', function ($http, $location, globalService) {
        var service = {};

        service.getdata = function (url, callback) {
            $http.get(url)
                .success(function (data, status, headers, config) {
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
                        //angular.element('.modal-dialog').hide();
                        globalService.RequestAuthentication();
                        $location.path('/Login');

                    }
                });
        };

        service.savedata = function (url, obj, callback) {
            $http.post( url, obj)
                .success(function (data, status, headers, config) {
                    response.status = status;
                    response.data = data;
                    response.headers = headers;
                    callback(response);
                })
                .error(function (data, status, headers, config) {
                    response.message = data;
                    response.status = status;
                    var message = globalService.gethttpmessage(status)
                    alert(message + ':' + response.message);
                    callback(response);
                    if (status === 401) {
                        globalService.RequestAuthentication();
                        $location.path('/Login');
                    }
                });
        };

        service.updatedata = function (url, obj, callback) {
            $http.put(url, obj)
                .success(function (data, status, headers, config) {
                    response.status = status;
                    response.headers = headers;
                    callback(response);
                })
                .error(function (data, status, headers, config) {
                    response.message = data;
                    response.status = status;
                    var message = globalService.gethttpmessage(status)
                    alert(message + ':' + response.message);
                    callback(response);

                    if (status === 401) {
                        globalService.RequestAuthentication();
                        $location.path('/Login');
                    }
                });
        };

        service.deletedata = function (url, callback) {
            $http.delete(url)
                .success(function (data, status, headers, config) {
                    response.status = status;
                    callback(response);
                })
                .error(function (data, status, headers, config) {
                    response.message = data;
                    response.status = status;
                    callback(response);
                    var message = globalService.gethttpmessage(status)
                    alert(message + ':' + response.message);
                    //callback(response);

                    if (status === 401) {
                        globalService.RequestAuthentication();
                        $location.path('/Login');
                    }
                });
        };

        return service;

    });

})();