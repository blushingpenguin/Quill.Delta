using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Quill.Delta
{
    public static class MentionSanitizer
    {
        public static Mention Sanitize(JObject dirtyObj)
        {
            var cleanObj = new Mention();

            if (dirtyObj == null)
            {
                return cleanObj;
            }

            var class_ = dirtyObj.GetStringValue("class");
            if (!String.IsNullOrEmpty(class_) && IsValidClass(class_))
            {
                cleanObj.Class = class_;
            }

            var id = dirtyObj.GetStringValue("id");
            if (!String.IsNullOrEmpty(id) && IsValidId(id))
            {
                cleanObj.Id = id;
            }

            var target = dirtyObj.GetStringValue("target");
            if (!String.IsNullOrEmpty(target) && IsValidTarget(target))
            {
                cleanObj.Target = target;
            }

            var avatar = dirtyObj.GetStringValue("avatar");
            if (!String.IsNullOrEmpty(avatar))
            {
                cleanObj.Avatar = UrlHelpers.Sanitize(avatar);
            }

            var endPoint = dirtyObj.GetStringValue("end-point");
            if (!String.IsNullOrEmpty(endPoint))
            {
                cleanObj.EndPoint = UrlHelpers.Sanitize(endPoint);
            }

            var slug = dirtyObj.GetStringValue("slug");
            if (!String.IsNullOrEmpty(slug))
            {
                cleanObj.Slug = slug;
            }

            return cleanObj;
        }

        readonly static Regex s_classAttrRe = new Regex("^[a-zA-Z0-9_\\-]{1,500}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidClass(string classAttr)
        {
            return s_classAttrRe.IsMatch(classAttr);
        }

        readonly static Regex s_idAttrRe = new Regex("^[a-zA-Z0-9_\\-\\:\\.]{1,500}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static bool IsValidId(string idAttr)
        {
            return s_idAttrRe.IsMatch(idAttr);
        }

        readonly static HashSet<string> validTargets =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "_self", "_blank", "_parent", "_top"
            };

        static bool IsValidTarget(string target)
        {
            return validTargets.Contains(target);
        }
    }
}
