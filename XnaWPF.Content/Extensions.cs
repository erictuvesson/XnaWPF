namespace XnaWPF.Content
{
    using System.IO;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    public static class Extensions
    {
        #region TryLoad

        public static bool TryLoad<T>(this ContentBuilder builder, ContentManager content, string importer, string processor, string file, out T value)
        {
            builder.Clear();
            builder.Add(file, Path.GetFileName(file), importer, processor);

            string buildError = builder.Build();
            if (string.IsNullOrEmpty(buildError))
            {
                value = content.Load<T>(Path.GetFileName(file));
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Builds and load a texture.
        /// </summary>
        public static bool TryLoadTexture2D(this ContentBuilder builder, ContentManager content, string file, out Microsoft.Xna.Framework.Graphics.Texture2D value)
        {
            return TryLoad<Microsoft.Xna.Framework.Graphics.Texture2D>(builder, content, "TextureImporter", "TextureProcessor", file, out value);
        }

        /// <summary>
        /// Builds and load a model. Supported: .fbx and .x .
        /// </summary>
        public static bool TryLoadModel(this ContentBuilder builder, ContentManager content, string file, out Microsoft.Xna.Framework.Graphics.Model value)
        {
            return TryLoad<Microsoft.Xna.Framework.Graphics.Model>(builder, content, null, "ModelProcessor", file, out value);
        }

        #endregion

        #region Load

        public static T Load<T>(this ContentBuilder builder, ContentManager content, string file)
        {
            switch (typeof(T).FullName)
            {
                case "Microsoft.Xna.Framework.Graphics.Texture2D":
                    return BuildNLoad<T>(builder, content, file, "TextureProcessor", "TextureImporter");
                case "Microsoft.Xna.Framework.Graphics.Model":
                    return BuildNLoad<T>(builder, content, file, null, "ModelProcessor"); // importer? (FbxImporter, XImporter)

                    // TODO: Add more

                default:
                    throw new System.ArgumentException("Wat is what? " + typeof(T).Name + " ??");
            }
        }


        private static T BuildNLoad<T>(ContentBuilder builder, ContentManager content, string file, string processor, string importer)
        {
            builder.Clear();
            builder.Add(file, Path.GetFileName(file), importer, processor);

            string buildError = builder.Build();
            if (string.IsNullOrEmpty(buildError))
            {
                return content.Load<T>(Path.GetFileName(file));
            }
            throw new System.NullReferenceException(buildError);
        }

        #endregion
    }
}
