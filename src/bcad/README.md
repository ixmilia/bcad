To build on Linux I needed (as root):

- Newer version of npm: `npm install -g npm`
- unzip: `apt install unzip`

To run final app via WSL I needed:

- X10 server for Windows installed and configured, e.g., X410, VcxSrv, etc.
- Launch the app as root: `user:artifacts/bin/pack/bcad-linux-x64$ sudo ./bcad --no-sandbox`
