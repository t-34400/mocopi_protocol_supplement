# mocopi_protocol_supplement

Supplementary documentation and sample code for the UDP communication format of Sony's motion capture mocopi, based on the [unofficial documentation (seagetch/mcp-receiver)](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md).

## Verification Environment
- Sample scene of mocopi-receiver-plugin-for-unity_1.0.5

## Supplementary Documentation

### Data Format
- Regarding the [`skdf` packet](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md#skdf-packet-structure), it is as described.
- For the [`fram` packet](https://github.com/seagetch/mcp-receiver/blob/main/doc/Protocol.md#fram-packet-structure), as of the latest version (2023.12.29), an additional 8 bytes (`uttm`) field after the `time` field is required.
  - It appears to store Unix Time as a little-endian floating-point number.
  - Confirm if this field is necessary from the [version information for each SDK](https://www.sony.net/Products/mocopi-dev/jp/downloads/DownloadInfo.html).
- The coordinate system of the `tran` field is Y-Up right-handed.
  - The conversion to Unity is as follows:
    ```
    Position: (x, y, z) -> (-x, y, z)
    Rotation: (x, y, z, w) -> (-x, y, z, -w)
    ```

### mocopi-receiver-plugin-for-unity Specifications
- mocopi-receiver-plugin-for-unity determines processing based on packet size and skips processing if sent with different sizes.

## Sample Code

### [Mocopi Packet Parser](./parser)
Python sample code that parses binary files in `skdf` and `fram` formats and displays the results in the log.

#### Usage
```powershell
py mocopi_parser.py ./sample/sample_skdf.bin type=skdf # Parse skdf packet format data
py mocopi_parser.py ./sample/sample_fram.bin type=fram # Parse fram packet format data
```

### Mocopi Packet Sender(./MocopiSender)

Unity module that sends the pose of models in the scene in the same format as the mocopi app.

#### Usage
[README.md](./MocopiSender/README.md)
