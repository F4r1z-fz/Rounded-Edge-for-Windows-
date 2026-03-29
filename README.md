# Rounded Edge

A small WPF Windows overlay app that displays a rounded-corner screen mask above other applications with a control panel.

Screenshots :

<img width="1920" height="1080" alt="Screenshot (1)" src="https://github.com/user-attachments/assets/c6f9c525-4509-49d4-91dc-c2c159b0fc91" />

<img width="1920" height="1080" alt="Screenshot (2)" src="https://github.com/user-attachments/assets/a8988e2b-bad4-4bfe-aed2-0c6ab4626bbb" />

<img width="1920" height="1080" alt="Screenshot (3)" src="https://github.com/user-attachments/assets/64013aa8-842d-4031-89e5-3ec50bae47f7" />

## Build

Using .NET SDK 8.0 or later.


## How to use

1. Launch the app.
2. The settings window will appear with controls for `Roundness` and `Opacity`.
3. Use the sliders for fine-grain adjustment.
4. Type exact values directly into the numeric fields and press `Enter` or click outside the field.
   - `Roundness` accepts values from `0` to `200`, Default Value is `12` .
   - `Opacity` accepts values from `0` to `100`, Default Value is `100`.
5. Click `Reset` to Change the values to default.
5. Click `Minimize` to put application back into the Tray.
6. Right-click the tray icon to reopen the window or exit the application or click `exit` from application.

## Notes

- The overlay is click-through so underlying apps remain interactive.
- The mask covers the full virtual screen area (multi-monitor supported).
- The UI has a themed visual design and numeric entry support.
- Use the tray icon to restore or exit the app when the window is minimized.

## License

This project is open source under the MIT License. See `LICENSE` for details.
