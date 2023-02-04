angular.module('umbraco').controller('SimpleRedirects.ImportExportController', function ($scope, Upload, editorService, notificationsService) {
    const vm = this;

    vm.loading = true;
    vm.buttonStates = 'init';
    vm.importing = false;
    vm.file = null;
    vm.overwriteMatches = false;
    vm.importResponse = null;
    vm.importErrorList = [];
    vm.importErrorListPageResult = [];
    vm.importErrorListResultsPerPage = 5;
    vm.redirectTotal = $scope.model.value.redirects;

    vm.pagination = {
        pageNumber: 1,
        totalPages: 1
    };

    vm.nextPage = nextPage;
    vm.prevPage = prevPage;
    vm.changePage = changePage;
    vm.goToPage = goToPage;

    function nextPage(pageNumber) {
        changePage(pageNumber);
    }

    function prevPage(pageNumber) {
        changePage(pageNumber);
    }

    function changePage(pageNumber) {
        if (pageNumber <= vm.pagination.totalPages && pageNumber > 0) {
            vm.importErrorListPageResult = vm.importErrorList.slice((pageNumber - 1) * vm.importErrorListResultsPerPage, pageNumber * vm.importErrorListResultsPerPage);
            vm.pagination.pageNumber = pageNumber;
        }
    }

    function goToPage(pageNumber) {
        changePage(pageNumber)
    }

    vm.exportFileName = {
        text: '',
        regex: '/[^a-z0-9_\\-]/gi',
        placeholder: 'Optionally type a filename'
    }

    vm.import = function importRedirects() {
        if (vm.file !== null) {
            toggleLoading();
            Upload.upload({
                url: "backoffice/SimpleRedirects/RedirectApi/ImportRedirects?overwriteMatches=" + vm.overwriteMatches,
                file: vm.file
            }).then(function (response) {
                if (!response || !response.data || response.status !== 200) {
                    notificationsService.error("Import failed", "An error occured while importing provided import list");
                } else {
                    notificationsService.success("Import completed", "Successfully ran through all import rows in provided file");
                    vm.importResponse = response.data;
                    vm.redirectTotal = vm.redirectTotal + vm.importResponse.addedRedirects;
                    vm.importErrorList = [];
                    if (vm.importResponse.errorRedirects.length > 0) {
                        for (let i = 0; i < vm.importResponse.errorRedirects.length; i++) {
                            const errorIndex = vm.importResponse.errorRedirects[i];
                            const error = {
                                entry: i + 1,
                                message: errorIndex.notes,
                                oldUrl: errorIndex.oldUrl,
                                newUrl: errorIndex.newUrl,
                                redirectCode: errorIndex.redirectCode,
                            }
                            vm.importErrorList.push(error);
                        }
                        UpdatePagination();
                    }
                }
                toggleLoading();
            });
        }
    }

    function UpdatePagination() {
        vm.pagination.pageNumber = 1;
        vm.pagination.totalPages = Math.ceil(vm.importErrorList.length / vm.importErrorListResultsPerPage);
        vm.goToPage(1);
    }

    vm.buttonGroup = {
        defaultButton: {
            labelKey: "actions_exportToCsv",
            handler: function () {
                if (!vm.loading) {
                    toggleLoading();
                    location.href = "/umbraco/backoffice/SimpleRedirects/RedirectApi/ExportRedirects?dataRecordProvider=Csv";
                    toggleLoading();
                }
            }
        },
        subButtons: [
            {
                labelKey: "actions_exportToExcel",
                handler: function () {
                    if (!vm.loading) {
                        toggleLoading();
                        location.href = "/umbraco/backoffice/SimpleRedirects/RedirectApi/ExportRedirects?dataRecordProvider=Excel";
                        toggleLoading();
                    }
                }
            }
        ]
    };
    
    vm.toggleOverwriteMatches = function () {
        vm.overwriteMatches = !vm.overwriteMatches;
    }

    $scope.handleFile = function ($file) {
        vm.file = $file;
    };

    function toggleLoading() {
        vm.loading = !vm.loading;
        vm.buttonStates = vm.loading ? "busy" : "success";
    }


    function init() {
        toggleLoading();
    }

    init();
});