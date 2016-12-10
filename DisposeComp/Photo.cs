using System;

namespace DisposeComp
{
    public class Photo
    {
        public int Id { get; set; }
        public string ImageUri { get; set; }

        public string Title => $"Image ID: {Id} - {ImageUri}";
    }
}