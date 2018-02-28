using System;

namespace Zavolokas.ImageProcessing.Inpainting
{
    /// <summary>
    /// Thrown when expected image was not provided.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ImageNotProvidedException: Exception
    {
    }

    /// <summary>
    /// Thrown when image has unappropriate size.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WrongImageSizeException: Exception
    { }
}
