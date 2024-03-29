﻿using System;
using System.Linq;
using GenderPayGap.Extensions;
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
