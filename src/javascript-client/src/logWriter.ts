import { Client } from './client';

export class LogWriter {
    private static instance: LogWriter | undefined;

    private constructor(private readonly client: Client) {
    }

    private write(message: string): void {
        this.client.writeOutputLine(message);
    }

    public static init(client: Client) {
        LogWriter.instance = new LogWriter(client);
    }

    public static write(message: string): void {
        if (LogWriter.instance) {
            LogWriter.instance.write(message);
        }
    }
}
