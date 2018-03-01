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
    /// Thrown when Init method was not called.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InitializationException: Exception
    {}

    /// <summary>
    /// Thrown when image has unappropriate size.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WrongImageSizeException: Exception
    { }

    /// <summary>
    /// Thrown when area to inpaint on Image is empty.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class NoAreaToInpaintException: Exception { }

    /// <summary>
    /// Thrown when images supposed to have same size but they don't.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ImageSizeNotMatchException: Exception
    { }

    /// <summary>
    /// Thrown when mapping supposed to have different size.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WrongMappingSizeException : Exception
    {
    }

    /// <summary>
    /// Thrown when inpaint area exceeds an image size.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WrongInpaintAreaSizeException: Exception { }
}
