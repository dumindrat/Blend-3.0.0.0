'use strict';

app.factory('RegistrationService', ['$http', '$rootScope', '$timeout', 'httpService', function ($http, $rootScope, $timeout, httpService) {
    var service = {};
    var regUrl = 'api/UserRegistration';

    service.Register = function (registerObj, callback) {
        httpService.savedata(regUrl, registerObj, function (response) {
            alert('User Registered : ' + response.message);
            if (response.status = 200) {
                callback(response);
            }
            else {
                response.status = status;
                callback(response);
            }
        })
    };

    return service;
}]);

