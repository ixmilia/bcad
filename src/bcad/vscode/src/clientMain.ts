const client = require('./client/main.js');
client.start([]).catch((err: any) => {
    const errorMessage = `error: ${err}`;
    console.error(errorMessage);
    let output = <HTMLTextAreaElement>document.getElementById("outputConsole");
    if (output.value.length > 0) {
        output.value += '\n';
    }
    output.value += errorMessage;
});;
