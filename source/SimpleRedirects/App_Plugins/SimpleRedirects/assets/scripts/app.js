app.requires.push('ngTable');

/*
* SimpleRedirects CONTROLLER
* -----------------------------------------------------
* Main Simple 301 controller used to render out the Simple 301 content section
*/
angular.module("umbraco").controller("SimpleRedirectsController", function ($scope, $filter, SimpleRedirectsApi, ngTableParams) {

    //Property to display error messages
    $scope.errorMessage = '';
    //App state
    $scope.initialLoad = false;
    $scope.cacheCleared = false;

    /*
    * Refresh the table. Uses $scope.redirects for data
    */
    $scope.refreshTable = function () {
        //If we aren't set up yet, return
        if (!$scope.tableParams) return;

        $scope.tableParams.total($scope.redirects.length);
        $scope.tableParams.reload();
    }

    /*
    * Handles clearing the cache by
    * calling to get all redirects again
    */
    $scope.clearCache = function () {
        $scope.cacheCleared = true;
        return SimpleRedirectsApi.clearCache().then($scope.fetchRedirects.bind(this));
    }

    /*
    * Handles exporting all simple redirects to a csv file
    */
    $scope.exportRedirects = function () {
        location.href = "backoffice/SimpleRedirects/RedirectApi/ExportRedirects";
    }
    
    /*
    * Handles fetching all redirects from the server.
    */
    $scope.fetchRedirects = function () {
        return SimpleRedirectsApi.getAll().then($scope.onReceiveAllRedirectsResponse.bind(this));
    };

    /*
    * Response handler for requesting all redirects
    */
    $scope.onReceiveAllRedirectsResponse = function (response) {
        //Something went wrong. Error out
        if (!response || !response.data) {
            $scope.errorMessage = "Error fetching redirects from server";
            return;
        }

        //We received data. Continue
        $scope.redirects = response.data;
        $scope.refreshTable();
    }

    /*
    * Handles adding a new redirect to the redirect collection.
    * Sends request off to API.
    */
    $scope.addRedirect = function (redirect) {
        SimpleRedirectsApi.add(redirect.isRegex, redirect.oldUrl, redirect.newUrl, redirect.redirectCode, redirect.notes)
            .then($scope.onAddRedirectResponse.bind(this));
    };

    /*
    * Handles the Add Redirect response from the API. Checks
    * for errors and updates table.
    */
    $scope.onAddRedirectResponse = function (response) {
        //Check for error
        if (!response || !response.data) {
            $scope.errorMessage = "Error sending request to add a new redirect.";
            return;
        }

        //Handle success from API
        if (response.data.success) {
            $scope.errorMessage = '';
            $scope.redirects.push(response.data.newRedirect);
            $scope.refreshTable();
        }
        else {
            $scope.errorMessage = response.data.message;
        }
    }

    /*
    * Handles sending a redirect to the API to as a reference for
    * updating the redirects collection server side.
    */
    $scope.updateRedirect = function (redirect) {
        SimpleRedirectsApi.update(redirect).then($scope.onUpdateRedirectResponse.bind(this, redirect));
    }

    /*
    * Handler for receiving a response from the Update Redirect API call
    * Will update the table with the returned, updated redirect
    */
    $scope.onUpdateRedirectResponse = function (redirect, response) {
        //Check for error
        if (!response || !response.data) {
            $scope.errorMessage = "Error sending request to update a redirect.";
            return;
        }

        if (response.data.success) {
            $scope.errorMessage = '';
            redirect.lastUpdated = response.data.updatedRedirect.lastUpdated;
            redirect.$edit = false;
        }
        else {
            $scope.errorMessage = response.data.message;
        }
    }

    /*
    * Handles the delete request to delete a redirect.
    * Calls the Delete API method passing in the redirect ID
    */
    $scope.deleteRedirect = function (redirect) {
        if (confirm("Are you sure you want to delete this redirect?")) {
            SimpleRedirectsApi.remove(redirect.id).then($scope.onDeleteRedirectResponse.bind(this, redirect));
        }
    }

    /*
    * Handles the DeleteRedirect response from the API. If successful,
    * remove the redirect from the table.
    */
    $scope.onDeleteRedirectResponse = function (redirect, response) {
        //Check for error
        if (!response || !response.data) {
            $scope.errorMessage = "Error sending request to delete a redirect.";
            return;
        }

        //Remove the item from the table. Splice redirect array
        if (response.data.success) {
            $scope.errorMessage = '';
            var index = $scope.redirects.indexOf(redirect);
            if (index > -1) {
                $scope.redirects.splice(index, 1);
                $scope.tableParams.total($scope.redirects.length);
                $scope.tableParams.reload();
            }

        }
        else {
            $scope.errorMessage = response.data.errorMessage;
        }
    }

    /*
    * Clears the global error message
    */
    $scope.clearErrorMessage = function () {
        $scope.errorMessage = '';
    }

    /*
    * Defines a new ngTable. 
    */
    $scope.tableParams = new ngTableParams({
        page: 1,            // show first page
        count: 10,          // count per page
        sorting: {
            LastUpdated: 'desc'     // initial sorting
        },
        filter: {
            Message: ''       // initial filter
        },
        data: $scope.initialData
    }, {
        total: 0,
        getData: function ($defer, params) {

            //Do we have redirects yet?
            var data = $scope.redirects || [];

            //Do we have a search term set in the search box?
            //If so, filter the redirects for that text
            var searchTerm = params.filter().Search;
            var searchedData = searchTerm ?
                data.filter(function (redirect) {
                    return redirect.notes.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 ||
                        redirect.oldUrl.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 ||
                        redirect.newUrl.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1
                }) : data;

            //Are we ordering the results?
            var orderedData = params.sorting() ?
                    $filter('orderBy')(searchedData, params.orderBy()) :
                    searchedData;

            //Set totals and page counts
            params.total(orderedData.length);
            var pagedResults = orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count());

            //Cheat and add a blank redirect so the user can add a new redirect right from the table
            pagedResults.push({ id: "-1", isRegex: false, oldUrl: "", newUrl: "", redirectCode: 301, notes: "", lastUpdated: "", $edit: true });
            $defer.resolve(pagedResults);
        }
    })

    /*
    * Initial load function to set loaded state
    */
    $scope.initLoad = function () {
        if (!$scope.initialLoad) {
            //Get the available log dates to view log entries for.
            $scope.fetchRedirects()
                .then(function () { $scope.initialLoad = true; });
        }
    }

    $(function () {
        $scope.$tab = $('a:contains("Manage Redirects")');

        //If we have a tab, set the click handler so we only
        //load the content on tab click. 
        if ($scope.$tab && $scope.$tab.length > 0) {
            var $parent = $scope.$tab.parent();

            // bind click event
            $scope.$tab.on('click', $scope.initLoad.bind(this));

            // if it is selected already or there is only one tab, init load
            if ($parent.hasClass('active') || $parent.children().length == 1)
                $scope.initLoad();
        }
        else {
            $scope.initLoad();
        }
    });

});

/*
* SimpleRedirects API
* -----------------------------------------------------
* Resource to handle making requests to the backoffice API to handle CRUD operations
* for redirect management
*/
angular.module("umbraco.resources").factory("SimpleRedirectsApi", function ($http) {
    return {
        //Get all redirects from the server
        getAll: function () {
            return $http.get("backoffice/SimpleRedirects/RedirectApi/GetAll");
        },
        //Send data to add a new redirect
        add: function (isRegex, oldUrl, newUrl, redirectCode, notes) {
            return $http.post("backoffice/SimpleRedirects/RedirectApi/Add", JSON.stringify({ isRegex: isRegex, oldUrl: oldUrl, newUrl: newUrl, redirectCode: redirectCode, notes: notes }));
        },
        //Send request to update an existing redirect
        update: function (redirect) {
            return $http.post("backoffice/SimpleRedirects/RedirectApi/Update", JSON.stringify({ redirect: redirect }));
        },
        //Remove / Delete an existing redirect
        remove: function (id) {
            return $http.delete("backoffice/SimpleRedirects/RedirectApi/Delete?id=" + id);
        },
        //Clear cache
        clearCache: function () {
            return $http.post("backoffice/SimpleRedirects/RedirectApi/ClearCache");
        }
    };
});
