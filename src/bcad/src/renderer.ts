// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.

import { Arguments } from './args';
import { Client } from './client';
import { InputConsole } from './inputConsole';
import { OutputConsole } from './outputConsole';
import { Ribbon } from './ribbon';
import { remote } from 'electron';
import { ViewControl } from './viewControl';

let args = new Arguments(remote.process.argv);
let client = new Client(args);
client.start();
new InputConsole(client);
new OutputConsole(client);
new Ribbon(client);
new ViewControl(client);
