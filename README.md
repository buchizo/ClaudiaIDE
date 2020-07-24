ClaudiaIDE
==========

This is a Visual Studio extension that lets you set a custom background image or slideshow.

## ver 2.2.x ##

Flickering has maybe been decreased, however the image fade interval option isn't supported for slideshows because problem effect by this update. If you want to expand the image to the full IDE window ("expand to IDE" option), ClaudiaIDE does not require theme editor. ClaudiaIDE automaticaly makes some backgrounds transparent (e.g. the text editor). However, for now, some windows, such as the solution explorer, still need to be made transaperent with theme editor, like as [Color Theme Editor for Visual Studio 2019](https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.VisualStudio2019ColorThemeEditor).

Enabling the option "User hardware graphics acceleration if available" will reduce flickering.

![hardware acceleration](Images/workaround01.png)

## ver 2.1.0 ##

Support versions changed as follows:

* Visual Studio 2017 (15.8 or later)
* Visual Studio 2019 (preview or RTM)

### Braking Changes ###

Visual Studio 2019 (16.1) extensions can support AsyncPackage only. ClaudiaIDE's this version change to AsyncPackage.

AsyncPackage can support Visual Studio 2015 or later. I decided move to forward and I don't support old Visual Studio versions (2017 15.8 or earlier). If you want to use this extension on old Visual Studio versions (2017 15.8 or earlier), you can use [1.28.6](https://github.com/buchizo/ClaudiaIDE/releases/tag/Release1.28.6). 

* If you already install 2.0.x to Visual Studio 2015 or Visual Studio 2017 (15.8 or ealier), you will uninstall 2.0.x and re-install 1.28.6 ClaudiaIDE.vsix.

## ver 1.28.6 ##

Support Visual Studio 2019 Preview 1

## ver 1.26 ##

Support Visual Studio 2017 (RTM)

## ver 1.24 ##

* Add feature (beta) : background image expand to IDE.
  * Known issue: If background image expand to IDE, editor background does not show when editor was moved to floating window with slideshow until next image.

### How to expand to IDE ###

1. Open option menu in Visual Studio (Tools -> Options)
2. Expand to IDE property set to True in ClaudiaIDE option page.

    ![copy theme](Images/howto01.png)

3. Customise Visual Studio theme using color theme editor (E.g. [Visual Studio 2015 Color Theme Editor](https://visualstudiogallery.msdn.microsoft.com/6f4b51b6-5c6b-4a81-9cb5-f2daa560430b))
4. Click "Create Copy of Theme" button, And "Edit Theme"

    ![copy theme](Images/howto02.png) ![Edit theme](Images/howto03.png)

5. Customise some colors opacity as follows (your own risk) :

    ![Edit opacity](Images/howto04.png)

* Solution Explorer
  * TreeView -> Background
* IDE and text editor
  * Environment -> Window
  * Environment -> EnvironmentBackground
  * Environment -> EnvironmentBackgroundGradientBegin
  * Environment -> EnvironmentBackgroundGradientEnd
  * Environment -> EnvironmentBackgroundGradientMiddle1
  * Environment -> EnvironmentBackgroundGradientMiddle2
* Window Title
  * Environment -> MainWindowActiveCaption
  * Environment -> MainWindowInactiveCaption
* Command Bar
  * Environment -> CommandShelfBackgroundGradientBegin
  * Environment -> CommandShelfBackgroundGradientEnd
  * Environment -> CommandShelfBackgroundGradientMiddle
  * Environment -> CommandShelfHighlightGradientBegin
  * Environment -> CommandShelfHighlightGradientEnd
  * Environment -> CommandShelfHighlightGradientMiddle
  * Environment -> CommandBarGradientBegin
  * Environment -> CommandBarGradientEnd
  * Environment -> CommandBarGradientMiddle
  * Environment -> CommandBarToolBarBorder

* Example:

    ![sample](Images/example01.png)

## ver 1.23 ##

* Add image stretch feature and improve performance

## ver 1.19 ##

* Additional support Visual Studio "15" Preview
* Can specifie max image size.

## ver 1.18 ##

* Add slideshow feature.
* Support Visual Studio 2012/2013/2015

Download:

[ClaudiaIDE (Visual Studio Gallery)](http://visualstudiogallery.msdn.microsoft.com/9ba50f8d-f30c-4e33-ab19-bfd9f56eb817 "ClaudiaIDE (Visual Studio Gallery)") 

# ライセンス #

## Microsoft Public License (Ms-PL) ##

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.

A "contributor" is any person that distributes its contribution under this license.

"Licensed patents" are a contributor's patent claims that read directly on its contribution.

Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
