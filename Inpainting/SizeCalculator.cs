namespace Zavolokas.ImageProcessing.Inpainting
{
    //internal sealed class SizeCalculator : ISizeCalculator
    //{
    //    public Size Calculate(int width, int height, byte levels)
    //    {
    //        int resultWidth = width;
    //        int resultHeight = height;

    //        if (width % 2 != 0)
    //            resultWidth--;

    //        if (height % 2 != 0)
    //            resultHeight--;

    //        while (!IsValid(resultWidth, levels))
    //        {
    //            resultWidth -= 2;
    //        }

    //        while (!IsValid(resultHeight, levels))
    //        {
    //            resultHeight -= 2;
    //        }

    //        return new Size(resultWidth, resultHeight);
    //    }

    //    private static bool IsValid(int val, byte levels)
    //    {
    //        bool wNoRest = true;
    //        for (int levelIndex = 0; levelIndex < (levels - 1) && wNoRest; levelIndex++)
    //        {
    //            if (val % 2 != 0)
    //                wNoRest = false;

    //            val /= 2;
    //        }

    //        return wNoRest;
    //    }
    //}

    //internal sealed class InputWindowCalculator : IInputWindowCalculator
    //{
    //    public Vector2D CalculateOffset(int width, int height, NewMarkup markup, NewMarkup[] constraints)
    //    {
    //        int x = 0;
    //        int y = 0;

    //        Point bottomRight = markup.BottomRight;

    //        if (constraints != null && constraints.Length > 0)
    //        {
    //            int right = bottomRight.X;
    //            int bottom = bottomRight.Y;

    //            for (int i = 0; i < constraints.Length; i++)
    //            {
    //                var b = constraints[i].BottomRight;
    //                if (right < b.X) right = b.X;
    //                if (bottom < b.Y) bottom = b.Y;
    //            }

    //            bottomRight = new Point(right, bottom);
    //        }

    //        while (x + width <= bottomRight.X || x + width <= markup.BottomRight.X) // markup.Width - width < x)
    //        {
    //            x++;
    //        }

    //        while (y + height <= bottomRight.Y || y + height <= markup.BottomRight.Y) //markup.Height - height < y)
    //        {
    //            y++;
    //        }

    //        return new Vector2D(x, y);
    //    }
    //}
}