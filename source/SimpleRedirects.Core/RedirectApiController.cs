using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SimpleRedirects.Core.Enums;
using SimpleRedirects.Core.Extensions;
using SimpleRedirects.Core.Models;
using SimpleRedirects.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace SimpleRedirects.Core
{
    [PluginController("SimpleRedirects")]
    public class RedirectApiController : UmbracoAuthorizedApiController
    {
        private readonly RedirectRepository _redirectRepository;
        private readonly ImportExportFactory _importExportFactory;

        public RedirectApiController(RedirectRepository redirectRepository, ImportExportFactory importExportFactory)
        {
            _redirectRepository = redirectRepository;
            _importExportFactory = importExportFactory;
        }

        /// <summary>
        /// GET all redirects
        /// </summary>
        /// <returns>Collection of all redirects</returns>
        [HttpGet]
        public IEnumerable<Redirect> GetAll()
        {
            return _redirectRepository.GetAllRedirects();
        }

        /// <summary>
        /// POST to add a new redirect
        /// </summary>
        /// <param name="request">Add redirect request</param>
        /// <returns>Response object detailing success or failure </returns>
        [HttpPost]
        public AddRedirectResponse Add(AddRedirectRequest request)
        {
            if (request == null) return new AddRedirectResponse() { Success = false, Message = "Request was empty" };
            if (!ModelState.IsValid)
                return new AddRedirectResponse() { Success = false, Message = "Missing required attributes" };

            try
            {
                var redirect = _redirectRepository.AddRedirect(request.IsRegex, request.OldUrl, request.NewUrl,
                    request.RedirectCode, request.Notes);
                return new AddRedirectResponse() { Success = true, NewRedirect = redirect };
            }
            catch (Exception e)
            {
                return new AddRedirectResponse()
                    { Success = false, Message = "There was an error adding the redirect : " + e.Message };
            }
        }

        /// <summary>
        /// POST to update a redirect
        /// </summary>
        /// <param name="request">Update redirect request</param>
        /// <returns>Response object detailing success or failure</returns>
        [HttpPost]
        public UpdateRedirectResponse Update(UpdateRedirectRequest request)
        {
            if (request == null) return new UpdateRedirectResponse() { Success = false, Message = "Request was empty" };
            if (!ModelState.IsValid)
                return new UpdateRedirectResponse() { Success = false, Message = "Missing required attributes" };

            try
            {
                var redirect = _redirectRepository.UpdateRedirect(request.Redirect);
                return new UpdateRedirectResponse() { Success = true, UpdatedRedirect = redirect };
            }
            catch (Exception e)
            {
                return new UpdateRedirectResponse()
                    { Success = false, Message = "There was an error updating the redirect : " + e.Message };
            }
        }

        /// <summary>
        /// DELETE to delete a redirect
        /// </summary>
        /// <param name="id">Id of redirect to delete</param>
        /// <returns>Response object detailing success or failure</returns>
        [HttpDelete]
        public DeleteRedirectResponse Delete(int id)
        {
            if (id == 0)
                return new DeleteRedirectResponse()
                    { Success = false, Message = "Invalid ID passed for redirect to delete" };

            try
            {
                _redirectRepository.DeleteRedirect(id);
                return new DeleteRedirectResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new DeleteRedirectResponse()
                    { Success = false, Message = "There was an error deleting the redirect : " + e.Message };
            }
        }

        /// <summary>
        /// DELETE to delete all redirects
        /// </summary>
        /// <returns>Response object detailing success or failure</returns>
        [HttpDelete]
        public DeleteRedirectResponse DeleteAll()
        {
            try
            {
                _redirectRepository.DeleteAllRedirects();
                return new DeleteRedirectResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new DeleteRedirectResponse()
                    { Success = false, Message = "There was an error deleting the redirects : " + e.Message };
            }
        }

        /// <summary>
        /// POST to clear cache
        /// </summary>
        [HttpPost]
        public void ClearCache()
        {
            _redirectRepository.ClearCache();
        }

        /// <summary>
        /// GET to export simple redirects to CSV
        /// </summary>
        [HttpGet]
        public ActionResult ExportRedirects(DataRecordProvider dataRecordProvider)
        {
            var dataRecordCollectionFile = _importExportFactory.GetDataRecordProvider(dataRecordProvider)
                .ExportDataRecordCollection();

            return dataRecordCollectionFile.AsFileContentResult();
        }

        /// <summary>
        /// Import redirects from CSV
        /// </summary>
        [HttpPost]
        public ImportRedirectsResponse ImportRedirects(bool overwriteMatches)
        {
            var file = HttpContext.Request.Form.Files.Any() ? HttpContext.Request.Form.Files[0] : null;
            if (file is null) return ImportRedirectsResponse.EmptyImportRecordResponse();
            if (!file.CanGetDataRecordProviderFromFile(out var provider))
                return ImportRedirectsResponse.EmptyImportRecordResponse(
                    "No redirects imported, provided file is not supported by the import process. Please provide a .csv or .xlsx file.");

            var response = _importExportFactory.GetDataRecordProvider(provider)
                .ImportRedirectsFromCollection(file, overwriteMatches);
            return response;
        }
    }
}