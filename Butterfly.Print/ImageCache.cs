namespace Butterfly.Print
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Summary description for ImageCache.
    /// </summary>
    public class ImageCache
    {
        private Dictionary<string, byte[]> cachedImageData;
        private static ImageCache instance;
        private static object syncRoot = new Object();

        private ImageCache()
        {
            cachedImageData = new Dictionary<string, byte[]>();
        }

        public static ImageCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ImageCache();
                    }
                }
                return instance;
            }
        }

        public void AddToCache(string name, byte[] image)
        {
            if (!cachedImageData.ContainsKey(name))
            {
                cachedImageData.Add(name, image);
            }
        }

        public void RemoveFromCache(string name)
        {
            if (cachedImageData.ContainsKey(name))
            {
                cachedImageData.Remove(name);
            }
        }

        public byte[] GetImageData(string name)
        {
            return cachedImageData.ContainsKey(name) ? cachedImageData[name] : null;
        }
    }
}