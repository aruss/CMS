﻿using System.Collections.Generic;
using System.Linq;
using Kooboo.CMS.Account.Models;
using System.Security.Claims;
using System.Web;
using Kooboo.Web.Css;
using User = Kooboo.CMS.Sites.Models.User;

namespace Kooboo.CMS.Web.Areas.Account
{
    public class ClaimsAuthenticationManager : System.Security.Claims.ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            var principal = ClaimsAuthenticationManager.MapToLocalPrincipal(incomingPrincipal);

            if (principal.Identity.IsAuthenticated)
            {
                this.CreateOrUpdateKoobooUser(principal);
                // this.CreateOrUpdateBringsyProfile(principal);
            }

            return base.Authenticate(resourceName, principal);
        }

        public static ClaimsPrincipal MapToLocalPrincipal(ClaimsPrincipal principal)
        {
            var identity = new ClaimsIdentity(principal.Claims, "Kooboo",
                ClaimsAuthenticationManager.GetNameClaimType(principal),
                ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }

        private static string GetNameClaimType(ClaimsPrincipal principal)
        {
            if (principal.FindFirst(ClaimTypes.NameIdentifier) != null)
                return ClaimTypes.NameIdentifier;

            if (principal.FindFirst(ClaimTypes.Name) != null)
                return ClaimTypes.Name;

            return ClaimTypes.Email;
        }

        public void CreateOrUpdateKoobooUser(ClaimsPrincipal principal)
        {
            // find uuid claim 
            var nameClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (nameClaim == null)
                nameClaim = principal.FindFirst(ClaimTypes.Name);
            var uuid = nameClaim.Value;

            #region Create a user if not already created

            var user = Kooboo.CMS.Account.Services.ServiceFactory.UserManager.Get(uuid);

            if (user == null)
            {
                // Create user 
                Kooboo.CMS.Account.Services.ServiceFactory.UserManager.Add(new Kooboo.CMS.Account.Models.User
                {
                    UUID = uuid,
                    UserName = uuid,
                    IsAdministrator = principal.IsInRole("Administrator") || principal.IsInRole("Administrators"),
                    Email = principal.FindFirst(ClaimTypes.Email).Value,
                });
            }
            else
            {
                // Update user 
                user.IsAdministrator = principal.IsInRole("Administrator") || principal.IsInRole("Administrators");
                user.Email = principal.FindFirst(ClaimTypes.Email).Value;
                Kooboo.CMS.Account.Services.ServiceFactory.UserManager.Update(user.UserName, user);
            }

            #endregion

            #region update roles 

            foreach (var roleClaim in  principal.FindAll(ClaimTypes.Role))
            {
                var role = Kooboo.CMS.Account.Services.ServiceFactory.RoleManager.Get(roleClaim.Value);
                if (role == null)
                {
                    Kooboo.CMS.Account.Services.ServiceFactory.RoleManager.Add(new Role
                    {
                        Name = roleClaim.Value
                    });
                }
            }
           
            #endregion 

            #region Assign user to current site if not already assigned

            var site = Kooboo.CMS.Sites.Persistence.Providers.SiteProvider.GetSiteByHostNameNPath(
                HttpContext.Current.Request.Url.Host, "");

            if (site != null)
            {
                var siteUser = Kooboo.CMS.Sites.Services.ServiceFactory.UserManager.Get(site,
                    principal.Identity.Name);

                if (siteUser == null)
                {
                    siteUser = new User
                    {
                        UUID = uuid,
                        UserName = principal.Identity.Name,
                        Profile = new Dictionary<string, string>(),
                        Site = site,
                        Roles = principal.FindAll(ClaimTypes.Role).Select(s => s.Value).ToList()
                    };

                    siteUser.Profile.Add("Email", principal.FindFirst(ClaimTypes.Email).Value);

                    Kooboo.CMS.Sites.Services.ServiceFactory.UserManager.Add(site, siteUser);
                }
                else
                {
                    var newSiteUser = new User
                    {
                        UUID = siteUser.UUID,
                        UserName = siteUser.UserName,
                        Profile = siteUser.Profile,
                        Site = siteUser.Site,
                        Roles = principal.FindAll(ClaimTypes.Role).Select(s => s.Value).ToList()
                    };

                    if (newSiteUser.Profile == null)
                        newSiteUser.Profile = new Dictionary<string, string>();

                    newSiteUser.Profile["Email"] = principal.FindFirst(ClaimTypes.Email).Value;

                    Kooboo.CMS.Sites.Services.ServiceFactory.UserManager.Update(site, newSiteUser, siteUser);
                }
            }

            #endregion
        }
    }
}