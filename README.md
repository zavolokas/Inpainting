# Inpainting
[![license](https://img.shields.io/github/license/mashape/apistatus.svg?style=flat-square)]()
[![Build Status](https://travis-ci.org/zavolokas/Inpainting.svg?branch=develop)](https://travis-ci.org/zavolokas/Inpainting)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.me/zavolokas)

.NET implementation of **inpainting/image complition** algorithm based on following publications:
- Yonatan Wexler, Eli Schechtman and Michal Irani *Space-time completion of video* IEEE. Trans. Pattern Analysis and Machine Intelligence, 29 (2007)
- Connelly Barnes, Eli Shechtman, Adam Finkelstein, and Dan B Goldman. *PatchMatch: A Randomized Correspondence Algorithm for Structural Image Editing*. ACM Transactions on Graphics (Proc. SIGGRAPH) 28(3), August 2009

## What is it for?
It can be used in image editing tools.

## How to use it?
`Inpainter` class defines the only one method:
  - `Inpaint`

It take as arguments:
  - An original image
  - A markup
  - A set of donors
  - Settings that control algorithm execution

## Examples

```csharp
var inpainter = new Inpainter();
var result = inpainter.Inpaint(imageArgb, markupArgb, donors);
result
    .FromArgbToBitmap()
    .SaveTo(resultPath, ImageFormat.Png)
    .ShowFile();

```

| Original | Markup | Process|
| ----------- | ------ |-------|
| ![t009]   | ![m009]|![p009]|
| ![t020]   | ![m020]|![p020]|
| ![t023]   | ![m023]|![p023]|
| ![t058]   | ![m058]|![p058]|
| ![t067]   | ![m067]|![p067]|

### Donors
[t009]: images/t009.jpg "original image"
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