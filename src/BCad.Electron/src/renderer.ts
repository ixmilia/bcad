// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.

import { Client } from './client'
import { Ribbon } from './ribbon';

let client = new Client();
client.start();
new Ribbon(client);
