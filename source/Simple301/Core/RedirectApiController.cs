using SimpleRedirects.Core.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace SimpleRedirects.Core
{
    /// <summary>
    /// Main plugin controller for Simple 301 - Redirect Manager.
    /// Handles making calls to the Redirect Repository for CRUD operations
    /// </summary>
    [PluginController("SimpleRedirects")]
    public class RedirectApiController : UmbracoAuthorizedApiController
    {
        private readonly RedirectRepository _redirectRepository;

        public RedirectApiController(RedirectRepository redirectRepository)
        {
            _redirectRepository = redirectRepository;
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
            if (!ModelState.IsValid) return new AddRedirectResponse() { Success = false, Message = "Missing required attributes" };

            try
            {
                var redirect = _redirectRepository.AddRedirect(request.IsRegex, request.OldUrl, request.NewUrl, request.RedirectCode, request.Notes);
                return new AddRedirectResponse() { Success = true, NewRedirect = redirect };
            }
            catch(Exception e)
            {
                return new AddRedirectResponse() { Success = false, Message = "There was an error adding the redirect : "+ e.Message };
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
            if (!ModelState.IsValid) return new UpdateRedirectResponse() { Success = false, Message = "Missing required attributes" };

            try
            {
                var redirect = _redirectRepository.UpdateRedirect(request.Redirect);
                return new UpdateRedirectResponse() { Success = true, UpdatedRedirect = redirect };
            }
            catch (Exception e)
            {
                return new UpdateRedirectResponse() { Success = false, Message = "There was an error updating the redirect : "+e.Message };
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
            if (id == 0) return new DeleteRedirectResponse() { Success = false, Message = "Invalid ID passed for redirect to delete" };

            try
            {
                _redirectRepository.DeleteRedirect(id);
                return new DeleteRedirectResponse() { Success = true };
            }
            catch(Exception e)
            {
                return new DeleteRedirectResponse() { Success = false, Message = "There was an error deleting the redirect : " + e.Message };
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
    }
}
