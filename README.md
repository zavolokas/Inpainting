# Inpainting
[![license](https://img.shields.io/github/license/mashape/apistatus.svg?style=flat-square)]()
[![Build Status](https://travis-ci.org/zavolokas/Inpainting.svg?branch=develop)](https://travis-ci.org/zavolokas/Inpainting)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.me/zavolokas)

.NET implementation of content-aware fill in image processing domain.

## What is it for?
Content aware fill is used to fill in unwanted or missing areas of photographs. See the example of such fill below where we don't want to have the man in blue t-shirt on our picture:

| Original image | Processed image |
|----------------|-----------------|
| ![t009] | ![r009] |

## How to use it?

```csharp
var inpainter = new Inpainter();
var result = inpainter.Inpaint(imageArgb, markupArgb, donors);
result
    .FromArgbToBitmap()
    .SaveTo(resultPath, ImageFormat.Png)
    .ShowFile();
```

The `Inpainter` takes as input 
- an image to inpaint
- a simitransparent image with a mask
- optionally it takes a set of simitransparent images that define donor areas for the parts of the area to inpaint.
- optionally is takes an instance of settings.

> Note: the images are not GDI+ images but images in an internal format and can be obtained from GDI+ `Bitmap`s using extensions.

### Examples

| Original | Markup | Process|
| ----------- | ------ |-------|
| ![t009]   | ![m009]|![p009]|
| ![t020]   | ![m020]|![p020]|
| ![t023]   | ![m023]|![p023]|
| ![t058]   | ![m058]|![p058]|
| ![t067]   | ![m067]|![p067]|

## Settings
The execution of the algorithm can be customized by adjusting the settings. 
- **MaxInpaintIterations**: determines how many iterations will be run to find better values for the area to fill. The more iterations you run, better result you'll get.
- **PatchDistanceCalculator**: determines algorithm to use for calculating a metrics how much one color is different from another. Possible values are:
  - Cie76 - fastest
  - Cie2000 - more accurate


### Donors

# Credits
The implementation is based on following publications:
- Yonatan Wexler, Eli Schechtman and Michal Irani *Space-time completion of video* IEEE. Trans. Pattern Analysis and Machine Intelligence, 29 (2007)
- Connelly Barnes, Eli Shechtman, Adam Finkelstein, and Dan B Goldman. *PatchMatch: A Randomized Correspondence Algorithm for Structural Image Editing*. ACM Transactions on Graphics (Proc. SIGGRAPH) 28(3), August 2009


[t009]: images/t009.jpg "original image"
[r009]: images/r009.png "original image"
[m009]: images/m009.png "markup"
[p009]: images/t009.gif "process"

[t020]: images/t020.jpg "original image"
[m020]: images/m020.png "markup"
[p020]: images/t020.gif "process"

[t023]: images/t023.jpg "original image"
[m023]: images/m023.png "markup"
[p023]: images/t023.gif "process"

[t058]: images/t058.jpg "original image"
[m058]: images/m058_1.png "markup"
[p058]: images/t058.gif "process"

[t067]: images/t067.jpg "original image"
[m067]: images/m067.png "markup"
[p067]: images/t067.gif "process"