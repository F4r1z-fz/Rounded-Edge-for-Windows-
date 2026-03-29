# Rounded Edge

A small WPF Windows overlay app that displays a rounded-corner screen mask above other applications with a kawaii anime-inspired control panel.

## Build

Requires .NET SDK 8.0 or later.


## How to use

1. Launch the app.
2. The settings window will appear with controls for `Roundness` and `Opacity`.
3. Use the sliders for fine-grain adjustment.
4. Type exact values directly into the numeric fields and press `Enter` or click outside the field.
   - `Roundness` accepts values from `0` to `200`.
   - `Opacity` accepts values from `0` to `100`.
5. Click `Show Overlay` to bring the settings window back if it is hidden.
6. Minimize the settings window to send it to the system tray.
7. Right-click the tray icon to reopen the window or exit the application.

## Notes

- The overlay is click-through so underlying apps remain interactive.
- The mask covers the full virtual screen area (multi-monitor supported).
- The UI has a themed visual design and numeric entry support.
- Use the tray icon to restore or exit the app when the window is minimized.

## License

This project is open source under the MIT License. See `LICENSE` for details.
