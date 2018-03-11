using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;

namespace InpaintWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OnOpenFileClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pbMarkup.Image = new Bitmap(openFileDialog1.FileName);
                btnInpaint.Enabled = true;
            }
        }

        private async void OnInpaint(object s, EventArgs e)
        {
            btnInpaint.Enabled = false;
            var imageArgb = ConvertToArgbImage((Bitmap)pbMarkup.Image);
            var markupArgb = ConvertToArgbImage((Bitmap)pbMarkup.RemoveMarkup);

            var markupArea = markupArgb.FromArgbToArea2D();

            if (markupArea.IsEmpty)
            {
                MessageBox.Show("No area to remove!");
            }
            else
            {
                var inpainter = new Inpainter();
                var settings = new InpaintSettings
                {
                    MaxInpaintIterations = 15,
                    PatchDistanceCalculator = ImagePatchDistance.Cie76
                };

                inpainter.IterationFinished += (sender, eventArgs) =>
                {
                    pbMarkup.Image = eventArgs.InpaintedLabImage
                        .FromLabToRgb()
                        .FromRgbToBitmap();
                };

                await Task.Factory.StartNew(() => inpainter.Inpaint(imageArgb, markupArgb, settings));

                MessageBox.Show("Finished");
            }

            btnInpaint.Enabled = true;
        }

        private static ZsImage ConvertToArgbImage(Bitmap imageBitmap)
        {
            const double maxSize = 2048.0;

            if (imageBitmap.Width > maxSize || imageBitmap.Height > maxSize)
            {
                var tmp = imageBitmap;
                double percent = imageBitmap.Width > imageBitmap.Height
                    ? maxSize / imageBitmap.Width
                    : maxSize / imageBitmap.Height;
                imageBitmap =
                    imageBitmap.CloneWithScaleTo((int)(imageBitmap.Width * percent), (int)(imageBitmap.Height * percent));
                tmp.Dispose();
            }

            var imageArgb = imageBitmap.ToArgbImage();
            return imageArgb;
        }

        private void OnOpenMarkupClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pbMarkup.RemoveMarkup = new Bitmap(openFileDialog1.FileName);
            }
        }
    }
}
