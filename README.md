# TouchpadGestures Advanced

## What's this?

[![](https://img.youtube.com/vi/UiN2GjRNuCY/0.jpg)](https://www.youtube.com/watch?v=UiN2GjRNuCY)

This Application was created to extend the multi-fingered gesture experience of your Precision Touchpad.

The standard feature of Windows 10 allows users to switch foreground windows by sliding your touchpad with three-fingers to the right. This application also allows users to switch tabs within a window with similar touchpad gestures.

## What can this application do?

### Now

- You can type Ctrl (+ Shift) + Tab by stroking the touchpad up and down with 3 fingers.
- With [the browser extension](https://github.com/kamektx/WebExtensions_for_TGA), you can switch tabs by pop-up windows.
- You can install the extension from [here for Chrome](https://chrome.google.com/webstore/detail/webextension-for-touchpad/iheedacijllmhlfbahgjekdjmcpelkjk) and [here for Firefox](https://addons.mozilla.org/ja/firefox/addon/touchpadgestures-advanced/)

## How to Use?

- This application is for **Windows 10 and Windows 11** only.
- Download the installer in Releases page.
- Unzip the installer's zip.
- Run "setup.exe".
- Change your default touchpad gesture settings on Windows; Press start button, Go to Settings, Open "Devices", Select "Touchpad" tab, Click "Advanced gesture configuration" button, In "Configure your three finger gestures" paragraph, Set "Up" and "Down" to "Nothing". To set "Right" to "Switch apps" is recommended.

## How to Develop?

- First, install TouchpadGestures Advanced.

### TGA_NativeMessaging_Cliant.exe 

- You can't build it as debug build.
- Build it as release build.
- Move the generated TGA_NativeMessaging_Cliant.exe to "%AppData%/Local/TouchpadGestures_Advanced/bin/NMC/".
- Run Firefox.

### Other projects

- Exit installed TouchpadGestures_Advance.exe via TaskTray.
- Build and Debug.
