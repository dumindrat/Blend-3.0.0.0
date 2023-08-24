'use strict';

//angular.module('Authentication')

app.factory('AuthenticationService',
    ['Base64', '$http', '$rootScope', '$timeout','$modal', 'httpService', 'globalService',
    function (Base64, $http, $rootScope, $timeout, $modal, httpService, globalService) {
        var loginUrl = 'api/User';

        var service = {};

        var createAuthData = function (username, password) {
            return 'Basic ' + Base64.encode(username + ':' + password);
        }

        service.Login = function (loginObj, callback) {

            var authlogindata = Base64.encode(loginObj.UserName + ':' + loginObj.Password);

            $rootScope.currentUser.authdata = createAuthData(loginObj.UserName, loginObj.Password); //This will added to the Request header by intercepter

            loginObj.CertId = authlogindata;
            loginObj.Password = 'lmnopqrstuvwxyz0123456789+/='; //fake password

            var response = {};

            $http.post(loginUrl)
               .success(function (data, status, headers, config) {
                   response.status = status;
                   response.data = data;
                   response.headers = headers;
                   callback(response);
               })
               .error(function (data, status, headers, config) {
                   response.message = globalService.gethttpmessage(status);
                   response.status = status;
                   callback(response);
               });
        };

        service.Logoff = function (loginObj, callback) {

            var authlogoffdata = Base64.encode(loginObj.UserName + ':' + 'xyz#');
            loginObj.CertId = authlogoffdata;
            loginObj.UserName = '';
            loginObj.Password = '';
            httpService.updatedata(loginUrl, loginObj, function (response) {
                callback(response);
            })

        };

        service.SetCredentials = function (username, password, token) {
            var authdata = createAuthData(username + ':' + password);  //'Basic ' + Base64.encode(username + ':' + password);
            //1 is default Module
            globalService.setcurrentuser(username, authdata, token);
            httpService.getdata('api/Menu/GetMenuByUserAndMainModule?id=' + username + '&mainmodule=' + 1, function (results) {
                $rootScope.$broadcast('menuChange', results.data);
            });
        };

        service.ClearCredentials = function () {
            globalService.clearcredentials();

        };

        service.GetPasswordExpireInfo = function (username) {

            httpService.getdata('api/User/GetPasswordExpireInfo/' + username, function (results) {
                $rootScope.PasswordExpireInfo = results.data;
                if ($rootScope.PasswordExpireInfo.IsRequireReset == true && $rootScope.PasswordExpireInfo.ExpirationDays < 14) {
                    
                    $rootScope.$broadcast('PASSWORD_EXPIRE', results.data);

                }
            });
        };
       
        return service;
    }])

.factory('Base64', function () {
    /* jshint ignore:start */

    var keyStr = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';

    return {
        encode: function (input) {
            var output = "";
            var chr1, chr2, chr3 = "";
            var enc1, enc2, enc3, enc4 = "";
            var i = 0;

            do {
                chr1 = input.charCodeAt(i++);
                chr2 = input.charCodeAt(i++);
                chr3 = input.charCodeAt(i++);

                enc1 = chr1 >> 2;
                enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
                enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
                enc4 = chr3 & 63;

                if (isNaN(chr2)) {
                    enc3 = enc4 = 64;
                } else if (isNaN(chr3)) {
                    enc4 = 64;
                }

                output = output +
                    keyStr.charAt(enc1) +
                    keyStr.charAt(enc2) +
                    keyStr.charAt(enc3) +
                    keyStr.charAt(enc4);
                chr1 = chr2 = chr3 = "";
                enc1 = enc2 = enc3 = enc4 = "";
            } while (i < input.length);

            return output;
        },

        decode: function (input) {
            var output = "";
            var chr1, chr2, chr3 = "";
            var enc1, enc2, enc3, enc4 = "";
            var i = 0;

            // remove all characters that are not A-Z, a-z, 0-9, +, /, or =
            var base64test = /[^A-Za-z0-9\+\/\=]/g;
            if (base64test.exec(input)) {
                window.alert("There were invalid base64 characters in the input text.\n" +
                    "Valid base64 characters are A-Z, a-z, 0-9, '+', '/',and '='\n" +
                    "Expect errors in decoding.");
            }
            input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");

            do {
                enc1 = keyStr.indexOf(input.charAt(i++));
                enc2 = keyStr.indexOf(input.charAt(i++));
                enc3 = keyStr.indexOf(input.charAt(i++));
                enc4 = keyStr.indexOf(input.charAt(i++));

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                output = output + String.fromCharCode(chr1);

                if (enc3 != 64) {
                    output = output + String.fromCharCode(chr2);
                }
                if (enc4 != 64) {
                    output = output + String.fromCharCode(chr3);
                }

                chr1 = chr2 = chr3 = "";
                enc1 = enc2 = enc3 = enc4 = "";

            } while (i < input.length);

            return output;
        }
    };

    /* jshint ignore:end */
});