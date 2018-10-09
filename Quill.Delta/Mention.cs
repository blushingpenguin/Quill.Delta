using System;

namespace Quill.Delta
{
    public class Mention
    {
        // [index: string]: string | undefined,
        public string Name { get; set; }
        public string Target { get; set; }
        public string Slug { get; set; }
        public string Class { get; set; }
        public string Avatar { get; set; }
        public string Id { get; set; }
        public string EndPoint { get; set; }

        public bool AnySet
        {
            get
            {
                return
                    !String.IsNullOrEmpty(Name) ||
                    !String.IsNullOrEmpty(Target) ||
                    !String.IsNullOrEmpty(Slug) ||
                    !String.IsNullOrEmpty(Class) ||
                    !String.IsNullOrEmpty(Avatar) ||
                    !String.IsNullOrEmpty(Id) ||
                    !String.IsNullOrEmpty(EndPoint);
            }
        }
    }
}
