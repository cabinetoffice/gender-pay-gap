﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace GenderPayGap.WebUI.Classes
{
    public static class Extensions
    {

        public static void AddStringTrimmingProvider(this MvcOptions option)
        {
            IModelBinderProvider binderToFind =
                option.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (binderToFind == null)
            {
                return;
            }

            int index = option.ModelBinderProviders.IndexOf(binderToFind);
            option.ModelBinderProviders.Insert(index, new TrimmingModelBinderProvider());
        }
        
        public static GDSDateFormatter ToGDSDate(this DateTime dateTime)
        {
            return new GDSDateFormatter(dateTime);
        }
        
        #region IPrinciple

        private static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            IEnumerable<Claim> claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            Claim claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        private static long GetUserId(this IPrincipal principal)
        {
            return principal.GetClaim("user_id").ToLong();
        }

        #endregion

        #region User Entity

        public static User FindUser(this IDataRepository repository, IPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            //GEt the logged in users identifier
            long userId = principal.GetUserId();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0)
            {
                return repository.Get<User>(userId);
            }

            return null;
        }

        #endregion
        
        #region Encypt Decrypt

        public static bool DecryptToId(this string enc, out long decId)
        {
            decId = 0;
            if (string.IsNullOrWhiteSpace(enc))
            {
                return false;
            }

            long id;
            try
            {
                id = Encryption.DecryptQuerystring(enc).ToInt64();
            }
            catch (Exception e)
            {
                return false;
            }
            
            if (id <= 0)
            {
                return false;
            }

            decId = id;
            return true;
        }

        #endregion

    }
}
